USE master;
GO

-- ============================================================
-- Configuration: Add one row per database you want to copy
-- ============================================================
DECLARE @Databases TABLE (
    SourceDatabaseName              varchar(200),
    SourceDatabaseLogicalName       varchar(200),
    SourceDatabaseLogicalNameForLog varchar(200),
    TargetDatabaseName              varchar(200)
)

INSERT INTO @Databases VALUES
--  Source DB Name                  Logical Name                    Logical Log Name                    Target DB Name
    ('ISAI.Lessons.Web.Portal',     'ISAI.Lessons.Web.Portal',      'ISAI.Lessons.Web.Portal_log',      'ISAI.Lessons.Web.Portal.Test'),
    ('UmbracoLessons',    'UmbracoLessons',     'UmbracoLessons_log',     'UmbracoLessons.Test')
    -- Add more rows here as needed...

-- ============================================================
-- Shared settings
-- ============================================================
DECLARE @BackupFolder       varchar(2000) = 'C:\Temp\'
DECLARE @TargetDatabaseFolder varchar(2000) = 'C:\Program Files\Microsoft SQL Server\MSSQL14.MSSQLSERVER\MSSQL\DATA\'

-- ============================================================
-- Internal variables (do not edit below)
-- ============================================================
DECLARE @SourceDatabaseName              varchar(200)
DECLARE @SourceDatabaseLogicalName       varchar(200)
DECLARE @SourceDatabaseLogicalNameForLog varchar(200)
DECLARE @TargetDatabaseName              varchar(200)
DECLARE @query                           varchar(2000)
DECLARE @DataFile                        varchar(2000)
DECLARE @LogFile                         varchar(2000)
DECLARE @BackupFile                      varchar(2000)
DECLARE @RowNum                          int = 1
DECLARE @TotalRows                       int

SELECT @TotalRows = COUNT(*) FROM @Databases

PRINT '============================================================'
PRINT 'Starting copy of ' + CAST(@TotalRows AS varchar) + ' database(s)'
PRINT '============================================================'

-- ============================================================
-- Loop through each database
-- ============================================================
WHILE @RowNum <= @TotalRows
BEGIN
    -- Pick the current row
    SELECT
        @SourceDatabaseName              = SourceDatabaseName,
        @SourceDatabaseLogicalName       = SourceDatabaseLogicalName,
        @SourceDatabaseLogicalNameForLog = SourceDatabaseLogicalNameForLog,
        @TargetDatabaseName              = TargetDatabaseName
    FROM (
        SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS RN
        FROM @Databases
    ) AS Numbered
    WHERE RN = @RowNum

    SET @BackupFile = @BackupFolder + @SourceDatabaseName + '.bak'
    SET @DataFile   = @TargetDatabaseFolder + @TargetDatabaseName + '.mdf'
    SET @LogFile    = @TargetDatabaseFolder + @TargetDatabaseName + '.ldf'

    PRINT ''
    PRINT '------------------------------------------------------------'
    PRINT 'Processing database ' + CAST(@RowNum AS varchar) + ' of ' + CAST(@TotalRows AS varchar) + ': ' + @SourceDatabaseName
    PRINT '------------------------------------------------------------'

    -- Step 1: Backup source database
    SET @query = 'BACKUP DATABASE ' + QUOTENAME(@SourceDatabaseName) + ' TO DISK = ' + QUOTENAME(@BackupFile, '''')
    PRINT 'Executing : ' + @query
    EXEC (@query)
    PRINT 'Backup complete.'

    -- Step 2: Drop target database if it already exists
    IF EXISTS (SELECT * FROM sysdatabases WHERE name = @TargetDatabaseName)
    BEGIN
        SET @query = 'ALTER DATABASE ' + QUOTENAME(@TargetDatabaseName) + ' SET SINGLE_USER WITH ROLLBACK IMMEDIATE'
        PRINT 'Executing : ' + @query
        EXEC (@query)

        SET @query = 'DROP DATABASE ' + QUOTENAME(@TargetDatabaseName)
        PRINT 'Executing : ' + @query
        EXEC (@query)
        PRINT 'Existing target database dropped.'
    END

    -- Step 3: Restore into target database
    SET @query = 'RESTORE DATABASE ' + QUOTENAME(@TargetDatabaseName)
              + ' FROM DISK = '      + QUOTENAME(@BackupFile, '''')
              + ' WITH MOVE '        + QUOTENAME(@SourceDatabaseLogicalName, '''')       + ' TO ' + QUOTENAME(@DataFile, '''')
              + ' , MOVE '           + QUOTENAME(@SourceDatabaseLogicalNameForLog, '''') + ' TO ' + QUOTENAME(@LogFile, '''')
    PRINT 'Executing : ' + @query
    EXEC (@query)
    PRINT 'Restore complete.'

    SET @RowNum = @RowNum + 1
END

PRINT ''
PRINT '============================================================'
PRINT 'All ' + CAST(@TotalRows AS varchar) + ' database(s) copied successfully.'
PRINT '============================================================'