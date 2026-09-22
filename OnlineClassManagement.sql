-- ============================================
-- Hệ thống Quản lý Lớp học Trực tuyến
-- Nhóm: WNC.G08
-- ============================================

USE master;
GO

-- Xóa database cũ nếu tồn tại
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'OnlineClassManagement')
BEGIN
    ALTER DATABASE OnlineClassManagement SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE OnlineClassManagement;
END
GO

CREATE DATABASE OnlineClassManagement;
GO

USE OnlineClassManagement;
GO

-- ============================================
-- TABLES
-- ============================================

-- Bảng Users: Quản lý người dùng
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    Password NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL CHECK (Role IN ('Student', 'Teacher', 'Admin')),
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Bảng Classes: Quản lý lớp học
CREATE TABLE Classes (
    ClassId INT IDENTITY(1,1) PRIMARY KEY,
    ClassName NVARCHAR(255) NOT NULL,
    ClassCode NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(1000),
    TeacherId INT NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (TeacherId) REFERENCES Users(UserId)
);

-- Bảng ClassEnrollments: Đăng ký lớp học
CREATE TABLE ClassEnrollments (
    EnrollmentId INT IDENTITY(1,1) PRIMARY KEY,
    ClassId INT NOT NULL,
    StudentId INT NOT NULL,
    EnrollmentDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ClassId) REFERENCES Classes(ClassId),
    FOREIGN KEY (StudentId) REFERENCES Users(UserId),
    UNIQUE(ClassId, StudentId)
);

-- Bảng Assignments: Bài tập
CREATE TABLE Assignments (
    AssignmentId INT IDENTITY(1,1) PRIMARY KEY,
    ClassId INT NOT NULL,
    Title NVARCHAR(255) NOT NULL,
    Description NVARCHAR(2000),
    DueDate DATETIME NOT NULL,
    MaxScore DECIMAL(5,2) DEFAULT 100,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ClassId) REFERENCES Classes(ClassId)
);

-- Bảng Submissions: Bài nộp
CREATE TABLE Submissions (
    SubmissionId INT IDENTITY(1,1) PRIMARY KEY,
    AssignmentId INT NOT NULL,
    StudentId INT NOT NULL,
    FileUrl NVARCHAR(500),
    SubmittedAt DATETIME DEFAULT GETDATE(),
    Score DECIMAL(5,2),
    Feedback NVARCHAR(2000),
    FOREIGN KEY (AssignmentId) REFERENCES Assignments(AssignmentId),
    FOREIGN KEY (StudentId) REFERENCES Users(UserId),
    UNIQUE(AssignmentId, StudentId)
);

-- Bảng CourseMaterials: Tài liệu học tập
CREATE TABLE CourseMaterials (
    MaterialId INT IDENTITY(1,1) PRIMARY KEY,
    ClassId INT NOT NULL,
    Title NVARCHAR(255) NOT NULL,
    FileUrl NVARCHAR(500) NOT NULL,
    UploadedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ClassId) REFERENCES Classes(ClassId)
);

GO

-- Bảng Announcements: Thông báo trong lớp học
CREATE TABLE Announcements (
    AnnouncementId INT IDENTITY(1,1) PRIMARY KEY,
    ClassId INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Content NVARCHAR(2000) NOT NULL,
    CreatedBy INT NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    IsImportant BIT NOT NULL DEFAULT 0,
    ExpiryDate DATETIME NULL,
    ViewCount INT DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (ClassId) REFERENCES Classes(ClassId),
    FOREIGN KEY (CreatedBy) REFERENCES Users(UserId)
);

-- ============================================
-- ALTER TABLES
-- ============================================

-- Add UpdatedAt to Assignments
ALTER TABLE Assignments ADD UpdatedAt DATETIME NOT NULL DEFAULT GETDATE();

-- Add IsActive and UpdatedAt to Classes
ALTER TABLE Classes ADD IsActive BIT NOT NULL DEFAULT 1;
ALTER TABLE Classes ADD UpdatedAt DATETIME NOT NULL DEFAULT GETDATE();
ALTER TABLE Classes ADD AcademicYear NVARCHAR(10) NOT NULL DEFAULT '2023-2024';
ALTER TABLE Classes ADD MaxStudents INT NULL;
ALTER TABLE Classes ADD Semester NVARCHAR(20) NOT NULL DEFAULT 'Hoc Ky 1';

-- Add PhoneNumber, IsActive, and UpdatedAt to Users
ALTER TABLE Users ADD PhoneNumber NVARCHAR(20) NULL;
ALTER TABLE Users ADD IsActive BIT NOT NULL DEFAULT 1;
ALTER TABLE Users ADD UpdatedAt DATETIME NOT NULL DEFAULT GETDATE();

-- Add Grade, Notes, and Status to ClassEnrollments
ALTER TABLE ClassEnrollments ADD Grade DECIMAL(3,2) NULL;
ALTER TABLE ClassEnrollments ADD Notes NVARCHAR(500) NULL;
ALTER TABLE ClassEnrollments ADD Status INT NOT NULL DEFAULT 1;
GO

-- ============================================
-- DỮ LIỆU MẪU
-- ============================================

-- Thêm Admin
INSERT INTO Users (Email, Password, FullName, Role) 
VALUES ('admin@classroom.com', '123456', N'Admin', 'Admin');

-- Thêm Giảng viên
INSERT INTO Users (Email, Password, FullName, Role) 
VALUES 
('teacher1@classroom.com', '123456', N'Lê Hữu Dũng', 'Teacher'),
('teacher2@classroom.com', '123456', N'Nguyễn Văn A', 'Teacher');

-- Thêm Học viên
INSERT INTO Users (Email, Password, FullName, Role) 
VALUES 
('student1@classroom.com', '123456', N'Hoàng Văn Trung', 'Student'),
('student2@classroom.com', '123456', N'Vương Đức An', 'Student'),
('student3@classroom.com', '123456', N'Đậu Huy Văn', 'Student');

-- Thêm Lớp học
INSERT INTO Classes (ClassName, ClassCode, Description, TeacherId) 
VALUES 
(N'Lập trình Web nâng cao', 'WNC2025', N'Khóa học ASP.NET Core MVC', 2),
(N'Cơ sở dữ liệu', 'DB2025', N'Khóa học SQL Server', 3);

-- Thêm Đăng ký lớp học
INSERT INTO ClassEnrollments (ClassId, StudentId) 
VALUES 
(1, 4), (1, 5), (1, 6),
(2, 4);

-- Thêm Bài tập
INSERT INTO Assignments (ClassId, Title, Description, DueDate, MaxScore) 
VALUES 
(1, N'Bài tập 1: Form đăng ký', N'Thiết kế form đăng ký người dùng', DATEADD(DAY, 7, GETDATE()), 100),
(1, N'Bài tập 2: CRUD cơ bản', N'Xây dựng chức năng CRUD', DATEADD(DAY, 14, GETDATE()), 100),
(2, N'Bài tập 1: Thiết kế DB', N'Thiết kế cơ sở dữ liệu', DATEADD(DAY, 10, GETDATE()), 100);

-- Thêm Bài nộp
INSERT INTO Submissions (AssignmentId, StudentId, FileUrl, Score, Feedback) 
VALUES 
(1, 4, '/files/baitap1_student1.pdf', 85, N'Làm tốt, cần cải thiện giao diện'),
(1, 5, '/files/baitap1_student2.pdf', 90, N'Rất tốt!');

-- Thêm Tài liệu
INSERT INTO CourseMaterials (ClassId, Title, FileUrl) 
VALUES 
(1, N'Slide 1: Giới thiệu ASP.NET', '/materials/slide1.pdf'),
(1, N'Slide 2: MVC Pattern', '/materials/slide2.pdf'),
(2, N'Tài liệu SQL Server', '/materials/sql.pdf');

GO

-- ============================================
-- KIỂM TRA DỮ LIỆU
-- ============================================

--SELECT 'Users' AS TableName, COUNT(*) AS RowCount FROM Users
--UNION ALL
--SELECT 'Classes', COUNT(*) FROM Classes
--UNION ALL
--SELECT 'ClassEnrollments', COUNT(*) FROM ClassEnrollments
--UNION ALL
--SELECT 'Assignments', COUNT(*) FROM Assignments
--UNION ALL
--SELECT 'Submissions', COUNT(*) FROM Submissions
--UNION ALL
--SELECT 'CourseMaterials', COUNT(*) FROM CourseMaterials;

GO

PRINT 'Database created successfully!';
-- Add missing columns to CourseMaterials table
ALTER TABLE CourseMaterials ADD [Description] nvarchar(500) NULL;
ALTER TABLE CourseMaterials ADD OriginalFileName nvarchar(255) NOT NULL DEFAULT '';
ALTER TABLE CourseMaterials ADD FileType nvarchar(20) NOT NULL DEFAULT '';
ALTER TABLE CourseMaterials ADD FileSize bigint NOT NULL DEFAULT 0;
ALTER TABLE CourseMaterials ADD UploadedBy int NOT NULL DEFAULT 0;
ALTER TABLE CourseMaterials ADD IsPublic bit NOT NULL DEFAULT 1;
ALTER TABLE CourseMaterials ADD DisplayOrder int NOT NULL DEFAULT 0;
ALTER TABLE CourseMaterials ADD DownloadCount int NOT NULL DEFAULT 0;

-- Add foreign key constraint for UploadedBy
ALTER TABLE CourseMaterials ADD CONSTRAINT FK_CourseMaterials_Users_UploadedBy FOREIGN KEY (UploadedBy) REFERENCES Users(UserId);

-- Insert sample data into CourseMaterials table
-- Make sure you have classes with ClassId = 1 and users with UserId = 1 in your database
INSERT INTO CourseMaterials (ClassId, Title, Description, FileUrl, OriginalFileName, FileType, FileSize, UploadedBy, IsPublic, DisplayOrder, DownloadCount) 
VALUES 
(1, 'Bài giảng 1', 'Nội dung bài giảng chương 1', '/materials/bai-giang-1.pdf', 'bai-giang-1.pdf', 'PDF', 1024000, 1, 1, 1, 0),
(1, 'Bài tập 1', 'Bài tập về nhà chương 1', '/materials/bai-tap-1.docx', 'bai-tap-1.docx', 'DOCX', 256000, 1, 1, 2, 0);
ALTER TABLE Assignments ADD Instructions nvarchar(2000) NULL;
ALTER TABLE Assignments ADD AssignmentType int NOT NULL DEFAULT 0;
ALTER TABLE Assignments ADD IsPublished bit NOT NULL DEFAULT 0;
ALTER TABLE Assignments ADD AllowLateSubmission bit NOT NULL DEFAULT 0;
ALTER TABLE Assignments ADD MaxLateDays int NOT NULL DEFAULT 0;
ALTER TABLE Assignments ADD CreatedBy int NOT NULL DEFAULT 0;
ALTER TABLE Assignments ADD UpdatedBy int NOT NULL DEFAULT 0;
-- Add foreign key constraints for CreatedBy and UpdatedBy
ALTER TABLE Assignments ADD CONSTRAINT FK_Assignments_Users_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(UserId);
ALTER TABLE Assignments ADD CONSTRAINT FK_Assignments_Users_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(UserId);

-- Insert sample data into Assignments table
-- Make sure you have classes with ClassId = 1 and users with UserId = 1 in your database
INSERT INTO Assignments (ClassId, Title, Description, DueDate, MaxScore, Instructions, AssignmentType, IsPublished, AllowLateSubmission, MaxLateDays, CreatedBy, UpdatedBy) 
VALUES 
(1, N'Bài tập 3: Xây dựng API', N'Xây dựng API RESTful với ASP.NET Core', DATEADD(DAY, 21, GETDATE()), 100, N'Hướng dẫn chi tiết về cách xây dựng API...', 1,       1, 0, 0, 1, 1),
(   1, N'Bài tập 4: Bảo mật ứng dụng', N'Triển khai các biện pháp bảo mật cho ứng dụng web', DATEADD(DAY, 28, GETDATE()), 100, N'Hướng dẫn chi tiết về bảo mật...', 1, 1, 1, 3, 1, 1);

--- Add missing columns to CourseMaterials table
ALTER TABLE CourseMaterials ADD [Description] nvarchar(500) NULL;
ALTER TABLE CourseMaterials ADD OriginalFileName nvarchar(255) NOT NULL DEFAULT '';
ALTER TABLE CourseMaterials ADD FileType nvarchar(20) NOT NULL DEFAULT '';
ALTER TABLE CourseMaterials ADD FileSize bigint NOT NULL DEFAULT 0;
ALTER TABLE CourseMaterials ADD UploadedBy int NOT NULL DEFAULT 0;
ALTER TABLE CourseMaterials ADD IsPublic bit NOT NULL DEFAULT 1;
ALTER TABLE CourseMaterials ADD DisplayOrder int NOT NULL DEFAULT 0;
ALTER TABLE CourseMaterials ADD DownloadCount int NOT NULL DEFAULT 0;

--- Add foreign key constraint for UploadedBy
ALTER TABLE CourseMaterials ADD CONSTRAINT FK_CourseMaterials_Users_UploadedBy FOREIGN KEY (UploadedBy) REFERENCES Users(UserId);

--- Insert sample data into CourseMaterials table
--- Make sure you have classes with ClassId = 1 and users with UserId = 1 in your database
INSERT INTO CourseMaterials (ClassId, Title, Description, FileUrl, OriginalFileName, FileType, FileSize, UploadedBy, IsPublic, DisplayOrder, DownloadCount) 
VALUES 
(1, 'Bài giảng 1', 'Nội dung bài giảng chương 1', '/materials/bai-giang-1.pdf', 'bai-giang-1.pdf', 'PDF', 1024000, 1, 1, 1, 0),
(1, 'Bài tập 1', 'Bài tập về nhà chương 1', '/materials/bai-tap-1.docx', 'bai-tap-1.docx', 'DOCX', 256000, 1, 1, 2, 0);
ALTER TABLE Submissions ADD Content nvarchar(max) NULL;
ALTER TABLE Submissions ADD OriginalFileName nvarchar(255) NULL;
ALTER TABLE Submissions ADD FileSize bigint NULL;
ALTER TABLE Submissions ADD Status int NOT NULL DEFAULT 0;
ALTER TABLE Submissions ADD LateSubmission bit NOT NULL DEFAULT 0;
ALTER TABLE Submissions ADD GradedAt datetime NULL;

-- Bảng Schedules: Lịch học và lịch thi
DROP TABLE IF EXISTS Schedules;

CREATE TABLE Schedules (
    ScheduleId INT IDENTITY(1,1) PRIMARY KEY,
    ClassId INT NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    DayOfWeek INT NOT NULL, -- 0 = Sunday, 1 = Monday, ..., 6 = Saturday
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    Location NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ClassId) REFERENCES Classes(ClassId)
);

-- Thêm Lịch học
-- Lớp 1 (Lập trình Web nâng cao) học vào thứ 2 và thứ 4 hàng tuần từ 8h-10h
INSERT INTO Schedules (ClassId, StartDate, EndDate, DayOfWeek, StartTime, EndTime, Location) 
VALUES 
(1, '2025-08-11', '2025-10-19', 2, '08:00:00', '10:00:00', N'Phòng 401'),
(1, '2025-08-11', '2025-10-19', 4, '08:00:00', '10:00:00', N'Phòng 401');

-- Lớp 2 (Cơ sở dữ liệu) học vào thứ 3 và thứ 5 hàng tuần từ 14h-16h
INSERT INTO Schedules (ClassId, StartDate, EndDate, DayOfWeek, StartTime, EndTime, Location) 
VALUES 
(2, '2025-08-12', '2025-10-16', 3, '14:00:00', '16:00:00', N'Phòng 301'),
(2, '2025-08-12', '2025-10-16', 5, '14:00:00', '16:00:00', N'Phòng 301');
