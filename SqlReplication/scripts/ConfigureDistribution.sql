USE master
GO
sp_configure 'advanced',1;
reconfigure;
GO
sp_configure 'xp_cmdshell', 1;
reconfigure;
GO

IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'distribution')
BEGIN
	/****** Installing the server as a Distributor.******/
	DECLARE @replfolder NVARCHAR(1024)
	DECLARE @pemissioncmd NVARCHAR(1024)
	DECLARE @instanceName NVARCHAR(128) = (SELECT @@SERVERNAME)
	DECLARE @DefaultDir VARCHAR(1024) = (SELECT CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS nvarchar(1024)))

	SET @replfolder = @DefaultDir + 'ReplData'
	SET @pemissioncmd = 'icacls "'+@replfolder+'" /grant Everyone:(OI)(CI)F /T'

	EXEC MASTER.dbo.xp_create_subdir @replfolder
	EXEC MASTER.dbo.xp_cmdshell @pemissioncmd

	EXEC sp_adddistributor @distributor = @instanceName
	EXEC sp_adddistributiondb @database = N'distribution', 
							@data_folder = @DefaultDir,
							@log_folder = @DefaultDir, 
							@log_file_size = 2, 
							@min_distretention = 0, 
							@max_distretention = 72, 
							@history_retention = 48, 
							@deletebatchsize_xact = 5000, 
							@deletebatchsize_cmd = 2000, 
							@security_mode = 1
END
GO

USE distribution
GO
DECLARE @replfolder NVARCHAR(1024)
DECLARE @datapath NVARCHAR(1024)
DECLARE @servername NVARCHAR(256)
select @datapath = CAST(serverproperty('InstanceDefaultDataPath') AS nvarchar(1024))
SET @servername = (SELECT @@SERVERNAME)
SELECT @replfolder = @datapath + 'ReplData'

if (not exists (select * from distribution.sys.sysobjects where name = 'UIProperties' and type = 'U ')) 
	create table UIProperties(id int) 
if (exists (select * from ::fn_listextendedproperty('SnapshotFolder', 'user', 'dbo', 'table', 'UIProperties', null, null))) 
	EXEC sp_updateextendedproperty N'SnapshotFolder', @replfolder, 'user', dbo, 'table', 'UIProperties' 
else 
	EXEC sp_addextendedproperty N'SnapshotFolder', @replfolder, 'user', dbo, 'table', 'UIProperties'

exec sp_adddistpublisher @publisher = @servername, @distribution_db = N'distribution', @security_mode = 1, @working_directory = @replfolder, @trusted = N'false', @thirdparty_flag = 0, @publisher_type = N'MSSQLSERVER'
