use {{database_name}}
GO

/* 
	Please change the variables value for your subscriber.
	Here,
	@replpublicationname variable is for a Replication Publication name. This must be same as publication side added by user.
	@subscriber_ip is for a SQL connection with subscriber or destination SQL Server instance.
	@subscriberDB is for a destination database.
	@subscriberLoginuser is for a SQL Login name for the subscriber or destination.
	@subscriberPassword is for a password of SQL Login [@subscriberLoginuser]
*/
DECLARE @replpublicationname NVARCHAR(256)= N'{{publication_name}}'
DECLARE @subscriber_ip NVARCHAR(256)= N'{{subscriber_ip}}'
DECLARE @subscriberDB NVARCHAR(256)= N'{{sub_db}}'
DECLARE @subscriberLoginuser NVARCHAR(256)= N'{{sql_user}}'
DECLARE @subscriberPassword NVARCHAR(256)= N'{{user_pwd}}'

-----------------BEGIN: Script to be run at Publisher Server-----------------
use {{database_name}}
exec sp_addsubscription @publication = @replpublicationname, @subscriber = @subscriber_ip, @destination_db = @subscriberDB, @subscription_type = N'Push', @sync_type = N'automatic', @article = N'all', @update_mode = N'read only', @subscriber_type = 0
exec sp_addpushsubscription_agent @publication = @replpublicationname, @subscriber = @subscriber_ip, @subscriber_db = @subscriberDB, @job_login = null, @job_password = null, @subscriber_security_mode = 0, @subscriber_login = @subscriberLoginuser, @subscriber_password = @subscriberPassword, @frequency_type = 64, @frequency_interval = 0, @frequency_relative_interval = 0, @frequency_recurrence_factor = 0, @frequency_subday = 0, @frequency_subday_interval = 0, @active_start_time_of_day = 0, @active_end_time_of_day = 235959, @active_start_date = 20210107, @active_end_date = 99991231, @enabled_for_syncmgr = N'False', @dts_package_location = N'Distributor'
GO
-----------------END: Script to be run at Publisher Server-----------------

