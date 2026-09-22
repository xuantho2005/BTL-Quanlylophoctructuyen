-- ============================================
-- Script cập nhật schema cho OnlineClassManagement
-- Script này có thể chạy độc lập trên máy khác
-- ============================================

-- Đảm bảo bảng __EFMigrationsHistory tồn tại
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

-- ============================================
-- Migration: AddUserColumnsOnly
-- ============================================
BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251103074830_AddUserColumnsOnly'
)
BEGIN
    -- Kiểm tra bảng Users tồn tại
    IF OBJECT_ID(N'[dbo].[Users]', N'U') IS NOT NULL
    BEGIN
        IF COL_LENGTH('Users', 'DateOfBirth') IS NULL
        BEGIN
            ALTER TABLE [Users] ADD [DateOfBirth] datetime2 NULL;
        END
        IF COL_LENGTH('Users', 'Hometown') IS NULL
        BEGIN
            ALTER TABLE [Users] ADD [Hometown] nvarchar(200) NULL;
        END
    END

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251103074830_AddUserColumnsOnly', N'8.0.0');
END;
GO

COMMIT;
GO

-- ============================================
-- Migration: CreateSchedules
-- ============================================
BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251103075653_ManualCreateSchedulesSql'
)
BEGIN
    -- Kiểm tra bảng Classes tồn tại trước khi tạo Schedules
    IF OBJECT_ID(N'[dbo].[Classes]', N'U') IS NOT NULL
    BEGIN
        IF OBJECT_ID(N'[dbo].[Schedules]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[Schedules]
            (
                [ScheduleId] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Schedules] PRIMARY KEY,
                [StartDate] datetime2 NOT NULL,
                [EndDate] datetime2 NOT NULL,
                [DayOfWeek] int NOT NULL,
                [StartTime] time NOT NULL,
                [EndTime] time NOT NULL,
                [Location] nvarchar(255) NULL,
                [ClassId] int NOT NULL
            );

            ALTER TABLE [dbo].[Schedules] WITH CHECK
            ADD CONSTRAINT [FK_Schedules_Classes_ClassId]
                FOREIGN KEY([ClassId]) REFERENCES [dbo].[Classes]([ClassId])
                ON DELETE CASCADE;

            CREATE INDEX [IX_Schedules_ClassId] ON [dbo].[Schedules]([ClassId]);
        END
    END

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251103075653_ManualCreateSchedulesSql', N'8.0.0');
END;
GO

COMMIT;
GO

-- ============================================
-- Migration: Fix_ClassEnrollment_Status_ToInt_WithData
-- ============================================
BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251103084338_Fix_ClassEnrollment_Status_ToInt_WithData'
)
BEGIN
    -- Kiểm tra bảng ClassEnrollments tồn tại
    IF OBJECT_ID(N'[dbo].[ClassEnrollments]', N'U') IS NOT NULL
    BEGIN
        -- Kiểm tra và convert dữ liệu từ string sang int
        IF EXISTS (
            SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'ClassEnrollments' AND COLUMN_NAME = 'Status' 
              AND DATA_TYPE IN ('nvarchar','varchar')
        )
        BEGIN
            -- Convert dữ liệu từ string sang int
            UPDATE ce
            SET [Status] = CASE 
                WHEN [Status] IN ('Pending','pending') THEN '1'
                WHEN [Status] IN ('Approved','approved') THEN '2'
                WHEN [Status] IN ('Rejected','rejected') THEN '3'
                WHEN TRY_CONVERT(int, [Status]) IS NOT NULL THEN [Status]
                ELSE '1'
            END
            FROM [dbo].[ClassEnrollments] ce;

            -- Xóa default constraint nếu có
            DECLARE @constraintName1 sysname;
            SELECT @constraintName1 = [d].[name]
            FROM [sys].[default_constraints] [d]
            INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
            WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ClassEnrollments]') AND [c].[name] = N'Status');
            IF @constraintName1 IS NOT NULL 
                EXEC(N'ALTER TABLE [ClassEnrollments] DROP CONSTRAINT [' + @constraintName1 + '];');

            -- Thay đổi kiểu dữ liệu
            ALTER TABLE [dbo].[ClassEnrollments]
            ALTER COLUMN [Status] int NOT NULL;
        END
    END

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251103084338_Fix_ClassEnrollment_Status_ToInt_WithData', N'8.0.0');
END;
GO

COMMIT;
GO

-- ============================================
-- Migration: ForceStatusIntParam (đảm bảo Status là int)
-- ============================================
BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251103085513_ForceStatusIntParam'
)
BEGIN
    -- Kiểm tra bảng ClassEnrollments tồn tại
    IF OBJECT_ID(N'[dbo].[ClassEnrollments]', N'U') IS NOT NULL
    BEGIN
        -- Kiểm tra nếu Status chưa phải là int
        IF EXISTS (
            SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'ClassEnrollments' AND COLUMN_NAME = 'Status' 
              AND DATA_TYPE NOT IN ('int', 'bigint', 'smallint', 'tinyint')
        )
        BEGIN
            DECLARE @var0 sysname;
            SELECT @var0 = [d].[name]
            FROM [sys].[default_constraints] [d]
            INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
            WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ClassEnrollments]') AND [c].[name] = N'Status');
            IF @var0 IS NOT NULL 
                EXEC(N'ALTER TABLE [ClassEnrollments] DROP CONSTRAINT [' + @var0 + '];');
            
            ALTER TABLE [ClassEnrollments] ALTER COLUMN [Status] int NOT NULL;
        END
    END

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251103085513_ForceStatusIntParam', N'8.0.0');
END;
GO

COMMIT;
GO

-- ============================================
-- Migration: Fix_AssignmentType_Int
-- ============================================
BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251103093124_Fix_AssignmentType_Int'
)
BEGIN
    -- Kiểm tra bảng Assignments tồn tại
    IF OBJECT_ID(N'[dbo].[Assignments]', N'U') IS NOT NULL
    BEGIN
        -- Kiểm tra và convert AssignmentType từ string sang int
        IF EXISTS (
            SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Assignments' AND COLUMN_NAME = 'AssignmentType' 
              AND DATA_TYPE IN ('nvarchar','varchar')
        )
        BEGIN
            -- Convert dữ liệu từ string sang int
            UPDATE a
            SET AssignmentType = CASE 
                WHEN AssignmentType IN ('Homework','homework') THEN '1'
                WHEN AssignmentType IN ('Quiz','quiz') THEN '2'
                WHEN AssignmentType IN ('Exam','exam') THEN '3'
                WHEN TRY_CONVERT(int, AssignmentType) IS NOT NULL THEN AssignmentType
                ELSE '1'
            END
            FROM [dbo].[Assignments] a;

            -- Xóa default constraint nếu có
            DECLARE @constraintName2 sysname;
            SELECT @constraintName2 = [d].[name]
            FROM [sys].[default_constraints] [d]
            INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
            WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Assignments]') AND [c].[name] = N'AssignmentType');
            IF @constraintName2 IS NOT NULL 
                EXEC(N'ALTER TABLE [Assignments] DROP CONSTRAINT [' + @constraintName2 + '];');

            -- Thay đổi kiểu dữ liệu
            ALTER TABLE [dbo].[Assignments]
            ALTER COLUMN [AssignmentType] int NOT NULL;
        END
    END

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251103093124_Fix_AssignmentType_Int', N'8.0.0');
END;
GO

COMMIT;
GO

-- ============================================
-- Migration: Fix_Submission_Status_Int
-- ============================================
BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251103093534_Fix_Submission_Status_Int'
)
BEGIN
    -- Kiểm tra bảng Submissions tồn tại
    IF OBJECT_ID(N'[dbo].[Submissions]', N'U') IS NOT NULL
    BEGIN
        -- Kiểm tra và convert Status từ string sang int
        IF EXISTS (
            SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Submissions' AND COLUMN_NAME = 'Status' 
              AND DATA_TYPE IN ('nvarchar','varchar')
        )
        BEGIN
            -- Convert dữ liệu từ string sang int
            UPDATE s
            SET [Status] = CASE 
                WHEN [Status] IN ('Submitted','submitted') THEN '1'
                WHEN [Status] IN ('Graded','graded') THEN '2'
                WHEN [Status] IN ('Returned','returned') THEN '3'
                WHEN TRY_CONVERT(int, [Status]) IS NOT NULL THEN [Status]
                ELSE '1'
            END
            FROM [dbo].[Submissions] s;

            -- Xóa default constraint nếu có
            DECLARE @constraintName3 sysname;
            SELECT @constraintName3 = [d].[name]
            FROM [sys].[default_constraints] [d]
            INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
            WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Submissions]') AND [c].[name] = N'Status');
            IF @constraintName3 IS NOT NULL 
                EXEC(N'ALTER TABLE [Submissions] DROP CONSTRAINT [' + @constraintName3 + '];');

            -- Thay đổi kiểu dữ liệu
            ALTER TABLE [dbo].[Submissions]
            ALTER COLUMN [Status] int NOT NULL;
        END
    END

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251103093534_Fix_Submission_Status_Int', N'8.0.0');
END;
GO

COMMIT;
GO

PRINT 'Schema update completed successfully!';
GO
