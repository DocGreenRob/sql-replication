use [publisher]
GO

/* 
	Please change the variables value for your Publisher. 
	Here, 
	@replpublicationname variable is for a Replication Publication name.
	@tablenames variable is for comma separated table name that you wants to add in Transaction Replication.
	@sqlloginuser is for an authentication user at Publisher side.
	@sqlloginpassword is for a password of SQL Login [@sqlloginuser]
*/
DECLARE @replpublicationname NVARCHAR(256)= N'CDCTransactionRepl'
DECLARE @tablenames NVARCHAR(1024) = 'user_details, user_type'
DECLARE @sqlloginuser NVARCHAR(128) = N'sa'
DECLARE @sqlloginpassword NVARCHAR(128) = N'1234'

/*----------------Adding Publication by DMV's----------------*/
DECLARE @databasename NVARCHAR(128)
DECLARE @sqlservreinstance NVARCHAR(256)
DECLARE @publisherdesc NVARCHAR(256)
SET  @databasename = DB_NAME()
SET @sqlservreinstance = (SELECT @@servername)
SET @publisherdesc = CONCAT('Transactional publication of database [', @databasename, '] from Publisher ', @sqlservreinstance, '.')

IF NOT EXISTS(select * from distribution.dbo.MSpublications WHERE publication = @replpublicationname)
BEGIN

	exec sp_replicationdboption @dbname = @databasename, @optname = N'publish', @value = N'true'
	
	/* Adding the transactional publication */
	exec sp_addpublication @publication = @replpublicationname, @description = @publisherdesc, @sync_method = N'concurrent', @retention = 0, @allow_push = N'true', @allow_pull = N'true', @allow_anonymous = N'true', @enabled_for_internet = N'false', @snapshot_in_defaultfolder = N'true', @compress_snapshot = N'false', @ftp_port = 21, @ftp_login = N'anonymous', @allow_subscription_copy = N'false', @add_to_active_directory = N'false', @repl_freq = N'continuous', @status = N'active', @independent_agent = N'true', @immediate_sync = N'true', @allow_sync_tran = N'false', @autogen_sync_procs = N'false', @allow_queued_tran = N'false', @allow_dts = N'false', @replicate_ddl = 1, @allow_initialize_from_backup = N'false', @enabled_for_p2p = N'false', @enabled_for_het_sub = N'false'
	exec sp_addpublication_snapshot @publication = @replpublicationname, @frequency_type = 1, @frequency_interval = 0, @frequency_relative_interval = 0, @frequency_recurrence_factor = 0, @frequency_subday = 0, @frequency_subday_interval = 0, @active_start_time_of_day = 0, @active_end_time_of_day = 235959, @active_start_date = 0, @active_end_date = 0, @job_login = null, @job_password = null, @publisher_security_mode = 0, @publisher_login = @sqlloginuser, @publisher_password = @sqlloginpassword
	EXEC sp_grant_publication_access @publication = @replpublicationname, @login = @sqlloginuser

	/*----------------Adding Articles by DMV's----------------*/
	DECLARE @sql_xml XML = Cast('<root><U>'+ Replace(@tablenames, ',', '</U><U>')+ '</U></root>' AS XML)
		
	SELECT TRIM(f.x.value('.', 'NVARCHAR(256)')) AS tablename, IDENTITY(INT, 1, 1) AS ident
	INTO #tables
	FROM @sql_xml.nodes('/root/U') f(x)
		

	DECLARE @count INT = 0, @start INT = 1
	SET @count = ISNULL((SELECT COUNT(1) FROM #tables), 0)

	WHILE(@count >= @start)
	BEGIN
		DECLARE @repltable NVARCHAR(256), @replinssp NVARCHAR(510), @replupdsp NVARCHAR(510), @repldltsp NVARCHAR(510)
		SELECT @repltable = tablename FROM #tables where ident = @start
		SET @replinssp = CONCAT(N'CALL sp_MSins_dbo', @repltable)
		SET @replupdsp = CONCAT(N'SCALL sp_MSupd_dbo', @repltable)
		SET @repldltsp = CONCAT(N'CALL sp_MSdel_dbo', @repltable)

		exec sp_addarticle @publication = @replpublicationname, @article = @repltable, @source_owner = N'dbo', @source_object = @repltable, @type = N'logbased', @description = null, @creation_script = null, @pre_creation_cmd = N'drop', @schema_option = 0x000000000803509F, @identityrangemanagementoption = N'manual', @destination_table = @repltable, @destination_owner = N'dbo', @vertical_partition = N'false', @ins_cmd = @replinssp, @del_cmd = @repldltsp, @upd_cmd = @replupdsp

		SET @start = @start + 1
	END
	DROP TABLE #tables
	
	/* Generaing intial snapshot for the publication */
	EXEC sp_startpublication_snapshot @publication = @replpublicationname;
END
ELSE
	PRINT 'Publication '+@replpublicationname+' already exists'