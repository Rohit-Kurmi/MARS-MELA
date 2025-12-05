/* ============================================================
   MARS — Mela Allotment & Registration System
   MS SQL Server DDL (Single Script)
   Created: (Auto-generated)
   Notes: Run on an empty database/schema. Adjust as needed.
   ============================================================ */
CREATE DATABASE MARS;
GO
USE MARS;
GO
SET NOCOUNT ON;
GO

/* ---------------------------
   0. Optional: Drop existing objects if re-running
   --------------------------- */
-- Drop tables in dependency order (if exists)
IF OBJECT_ID('dbo.AuditLog','U') IS NOT NULL DROP TABLE dbo.AuditLog;
IF OBJECT_ID('dbo.NotificationLog','U') IS NOT NULL DROP TABLE dbo.NotificationLog;
IF OBJECT_ID('dbo.RefundTransaction','U') IS NOT NULL DROP TABLE dbo.RefundTransaction;
IF OBJECT_ID('dbo.ShopClosure','U') IS NOT NULL DROP TABLE dbo.ShopClosure;
IF OBJECT_ID('dbo.Blacklist','U') IS NOT NULL DROP TABLE dbo.Blacklist;
IF OBJECT_ID('dbo.VendorHistory','U') IS NOT NULL DROP TABLE dbo.VendorHistory;
IF OBJECT_ID('dbo.FireSafetyCertificate','U') IS NOT NULL DROP TABLE dbo.FireSafetyCertificate;
IF OBJECT_ID('dbo.Allotment','U') IS NOT NULL DROP TABLE dbo.Allotment;
IF OBJECT_ID('dbo.PaymentTransaction','U') IS NOT NULL DROP TABLE dbo.PaymentTransaction;
IF OBJECT_ID('dbo.BidApplication','U') IS NOT NULL DROP TABLE dbo.BidApplication;
IF OBJECT_ID('dbo.QRPass','U') IS NOT NULL DROP TABLE dbo.QRPass;
IF OBJECT_ID('dbo.SitePlan','U') IS NOT NULL DROP TABLE dbo.SitePlan;
IF OBJECT_ID('dbo.UtilityPricing','U') IS NOT NULL DROP TABLE dbo.UtilityPricing;
IF OBJECT_ID('dbo.CategoryMaxLimit','U') IS NOT NULL DROP TABLE dbo.CategoryMaxLimit;
IF OBJECT_ID('dbo.CategoryReservation','U') IS NOT NULL DROP TABLE dbo.CategoryReservation;
IF OBJECT_ID('dbo.ShopUnit','U') IS NOT NULL DROP TABLE dbo.ShopUnit;
IF OBJECT_ID('dbo.Block','U') IS NOT NULL DROP TABLE dbo.Block;
IF OBJECT_ID('dbo.SubSector','U') IS NOT NULL DROP TABLE dbo.SubSector;
IF OBJECT_ID('dbo.Sector','U') IS NOT NULL DROP TABLE dbo.Sector;
IF OBJECT_ID('dbo.TradeFair','U') IS NOT NULL DROP TABLE dbo.TradeFair;
IF OBJECT_ID('dbo.Company','U') IS NOT NULL DROP TABLE dbo.Company;
IF OBJECT_ID('dbo.UserRoleMapping','U') IS NOT NULL DROP TABLE dbo.UserRoleMapping;
IF OBJECT_ID('dbo.UserRoles','U') IS NOT NULL DROP TABLE dbo.UserRoles;
IF OBJECT_ID('dbo.QRPass','U') IS NOT NULL DROP TABLE dbo.QRPass;
IF OBJECT_ID('dbo.Banners','U') IS NOT NULL DROP TABLE dbo.Banners;
IF OBJECT_ID('dbo.EmergencyContact','U') IS NOT NULL DROP TABLE dbo.EmergencyContact;
IF OBJECT_ID('dbo.Users','U') IS NOT NULL DROP TABLE dbo.Users;
GO

/* ---------------------------
   1. Users & Roles
   --------------------------- */
CREATE TABLE dbo.Users (
    UserID BIGINT IDENTITY(1,1) PRIMARY KEY,
    MobileNo NVARCHAR(15) NULL,
    EmailID NVARCHAR(150) NULL,
    PasswordHash NVARCHAR(512) NULL,
    OTPCode NVARCHAR(20) NULL,
    OTPGeneratedAt DATETIME2 NULL,
    EmailVerified BIT NOT NULL CONSTRAINT DF_Users_EmailVerified DEFAULT (0),
    MobileVerified BIT NOT NULL CONSTRAINT DF_Users_MobileVerified DEFAULT (0),
    Status TINYINT NOT NULL CONSTRAINT DF_Users_Status DEFAULT (1), -- 1=Active,0=Inactive
    FirstName NVARCHAR(100) NULL,
    LastName NVARCHAR(100) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    LastLoginAt DATETIME2 NULL,
    CreatedBy VARCHAR(50) NULL
);
GO

CREATE TABLE dbo.UserRoles (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(100) NOT NULL UNIQUE -- e.g., Citizen, SuperAdmin, MelaAdmin, Finance, GateStaff, Inspector
);
GO

CREATE TABLE dbo.UserRoleMapping (
    MapID BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserID BIGINT NOT NULL,
    RoleID INT NOT NULL,
    AssignedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    AssignedBy BIGINT NULL,
    CONSTRAINT FK_UserRoleMapping_User FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID),
    CONSTRAINT FK_UserRoleMapping_Role FOREIGN KEY (RoleID) REFERENCES dbo.UserRoles(RoleID)
);
GO

CREATE INDEX IX_UserRoleMapping_UserID ON dbo.UserRoleMapping(UserID);
CREATE INDEX IX_UserRoleMapping_RoleID ON dbo.UserRoleMapping(RoleID);
GO

/* ---------------------------
   2. Company / Firm
   --------------------------- */
CREATE TABLE dbo.Company (
    CompanyID BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserID BIGINT NOT NULL, -- owner or the registering user
    CompanyName NVARCHAR(200) NOT NULL,
    RegistrationNumber NVARCHAR(100) NULL,
    GSTIN NVARCHAR(20) NULL,
    PAN NVARCHAR(20) NULL,
    ConstitutionOfFirm NVARCHAR(100) NULL,
    Address NVARCHAR(500) NULL,
    Tehsil NVARCHAR(200) NULL,
    District NVARCHAR(200) NULL,
    State NVARCHAR(200) NULL,
    ContactNo NVARCHAR(30) NULL,
    AuthorizedPerson NVARCHAR(200) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Company_User FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID)
);
GO

CREATE INDEX IX_Company_UserID ON dbo.Company(UserID);
CREATE INDEX IX_Company_GSTIN ON dbo.Company(GSTIN);
CREATE INDEX IX_Company_PAN ON dbo.Company(PAN);
GO

/* ---------------------------
   3. Trade Fair / Carnival
   --------------------------- */
CREATE TABLE dbo.TradeFair (
    FairID BIGINT IDENTITY(1,1) PRIMARY KEY,
    FairName NVARCHAR(300) NOT NULL,
    Division NVARCHAR(200) NULL,
    District NVARCHAR(200) NULL,
    Tehsil NVARCHAR(200) NULL,
    City NVARCHAR(200) NULL,
    StartDate DATE NULL,
    EndDate DATE NULL,
    ApplyStartDate DATE NULL,
    ApplyEndDate DATE NULL,
    FairLogoPath NVARCHAR(500) NULL,
    ContactMobile1 NVARCHAR(20) NULL,
    ContactMobile2 NVARCHAR(20) NULL,
    ContactEmail NVARCHAR(150) NULL,
    Status TINYINT NOT NULL DEFAULT 1, -- 1=Active,0=Inactive/Archived
    CreatedBy BIGINT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

CREATE INDEX IX_TradeFair_Status ON dbo.TradeFair(Status);
CREATE INDEX IX_TradeFair_Dates ON dbo.TradeFair(StartDate, EndDate);
GO

/* ---------------------------
   4. Sector / SubSector / Block
   --------------------------- */
CREATE TABLE dbo.Sector (
    SectorID BIGINT IDENTITY(1,1) PRIMARY KEY,
    FairID BIGINT NOT NULL,
    SectorName NVARCHAR(200) NOT NULL,
    SectorGroup NVARCHAR(100) NULL,
    Area NVARCHAR(200) NULL,
    Description NVARCHAR(1000) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Sector_Fair FOREIGN KEY (FairID) REFERENCES dbo.TradeFair(FairID)
);
GO

CREATE TABLE dbo.SubSector (
    SubSectorID BIGINT IDENTITY(1,1) PRIMARY KEY,
    SectorID BIGINT NOT NULL,
    SubSectorName NVARCHAR(200) NOT NULL,
    GroupName NVARCHAR(100) NULL,
    Description NVARCHAR(1000) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_SubSector_Sector FOREIGN KEY (SectorID) REFERENCES dbo.Sector(SectorID)
);
GO

CREATE TABLE dbo.Block (
    BlockID BIGINT IDENTITY(1,1) PRIMARY KEY,
    SubSectorID BIGINT NOT NULL,
    BlockName NVARCHAR(200) NOT NULL,
    BlockGroup NVARCHAR(100) NULL,
    Description NVARCHAR(1000) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Block_SubSector FOREIGN KEY (SubSectorID) REFERENCES dbo.SubSector(SubSectorID)
);
GO

CREATE INDEX IX_Sector_FairID ON dbo.Sector(FairID);
CREATE INDEX IX_SubSector_SectorID ON dbo.SubSector(SectorID);
CREATE INDEX IX_Block_SubSectorID ON dbo.Block(SubSectorID);
GO

/* ---------------------------
   5. Shop Units
   --------------------------- */
CREATE TABLE dbo.ShopUnit (
    ShopUnitID BIGINT IDENTITY(1,1) PRIMARY KEY,
    BlockID BIGINT NOT NULL,
    UnitName NVARCHAR(200) NOT NULL,
    UnitInitials NVARCHAR(50) NULL,
    LengthFeet DECIMAL(10,2) NULL,
    BreadthFeet DECIMAL(10,2) NULL,
    NumShops INT NULL DEFAULT 1,
    FormFee DECIMAL(18,2) NOT NULL DEFAULT 0,
    EstimatedAmount DECIMAL(18,2) NULL,
    GSTAmount DECIMAL(18,2) NULL,
    MinBidAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Category NVARCHAR(100) NULL,
    SecurityDeposit DECIMAL(18,2) NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_ShopUnit_Block FOREIGN KEY (BlockID) REFERENCES dbo.Block(BlockID)
);
GO

CREATE INDEX IX_ShopUnit_BlockID ON dbo.ShopUnit(BlockID);
CREATE INDEX IX_ShopUnit_Category ON dbo.ShopUnit(Category);
GO

/* ---------------------------
   6. Category Reservation & Limits
   --------------------------- */
CREATE TABLE dbo.CategoryReservation (
    ReservationID BIGINT IDENTITY(1,1) PRIMARY KEY,
    FairID BIGINT NOT NULL,
    CategoryName NVARCHAR(100) NOT NULL,
    ReservedCount INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_CategoryReservation_Fair FOREIGN KEY (FairID) REFERENCES dbo.TradeFair(FairID)
);
GO

CREATE TABLE dbo.CategoryMaxLimit (
    LimitID BIGINT IDENTITY(1,1) PRIMARY KEY,
    FairID BIGINT NOT NULL,
    CategoryName NVARCHAR(100) NOT NULL,
    MaxAllowedUnits INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_CategoryMaxLimit_Fair FOREIGN KEY (FairID) REFERENCES dbo.TradeFair(FairID)
);
GO

/* ---------------------------
   7. Site Plan & Utility Pricing
   --------------------------- */
CREATE TABLE dbo.SitePlan (
    PlanID BIGINT IDENTITY(1,1) PRIMARY KEY,
    FairID BIGINT NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    UploadedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UploadedBy BIGINT NULL,
    CONSTRAINT FK_SitePlan_Fair FOREIGN KEY (FairID) REFERENCES dbo.TradeFair(FairID)
);
GO

CREATE TABLE dbo.UtilityPricing (
    UtilityID BIGINT IDENTITY(1,1) PRIMARY KEY,
    FairID BIGINT NOT NULL,
    ElectricityRate DECIMAL(18,2) NULL,
    WaterRate DECIMAL(18,2) NULL,
    WasteCharge DECIMAL(18,2) NULL,
    EffectiveFrom DATE NULL,
    EffectiveTo DATE NULL,
    CONSTRAINT FK_UtilityPricing_Fair FOREIGN KEY (FairID) REFERENCES dbo.TradeFair(FairID)
);
GO

/* ---------------------------
   8. Bidding Module
   --------------------------- */
CREATE TABLE dbo.BidApplication (
    BidID BIGINT IDENTITY(1,1) PRIMARY KEY,
    FairID BIGINT NOT NULL,
    UserID BIGINT NOT NULL,
    CompanyID BIGINT NULL,
    ShopUnitID BIGINT NOT NULL,
    BidAmount DECIMAL(18,2) NOT NULL,
    FormFee DECIMAL(18,2) NOT NULL DEFAULT 0,
    PlatformFee DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalPayable DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Paid, UnderReview, Allotted, Rejected, Cancelled
    AppliedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    PaymentDueAt DATETIME2 NULL,
    PaymentRef NVARCHAR(200) NULL,
    IsWithdrawn BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Bid_Fair FOREIGN KEY (FairID) REFERENCES dbo.TradeFair(FairID),
    CONSTRAINT FK_Bid_User FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID),
    CONSTRAINT FK_Bid_Company FOREIGN KEY (CompanyID) REFERENCES dbo.Company(CompanyID),
    CONSTRAINT FK_Bid_ShopUnit FOREIGN KEY (ShopUnitID) REFERENCES dbo.ShopUnit(ShopUnitID)
);
GO

CREATE INDEX IX_BidApplication_Fair_Shop ON dbo.BidApplication(FairID, ShopUnitID);
CREATE INDEX IX_BidApplication_User ON dbo.BidApplication(UserID);
CREATE INDEX IX_BidApplication_Status ON dbo.BidApplication(Status);

/* ---------------------------
   9. Payment Transactions
   --------------------------- */
CREATE TABLE dbo.PaymentTransaction (
    PaymentID BIGINT IDENTITY(1,1) PRIMARY KEY,
    BidID BIGINT NULL,
    TransactionRef NVARCHAR(300) NULL,
    PGProvider NVARCHAR(100) NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(50) NOT NULL, -- Success, Failed, Pending, Refunded
    PGResponse NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Payment_Bid FOREIGN KEY (BidID) REFERENCES dbo.BidApplication(BidID)
);
GO

CREATE INDEX IX_Payment_BidID ON dbo.PaymentTransaction(BidID);
CREATE INDEX IX_Payment_Status ON dbo.PaymentTransaction(Status);

/* ---------------------------
   10. Allotment
   --------------------------- */
CREATE TABLE dbo.Allotment (
    AllotmentID BIGINT IDENTITY(1,1) PRIMARY KEY,
    BidID BIGINT NOT NULL UNIQUE,
    ShopUnitID BIGINT NOT NULL,
    UserID BIGINT NOT NULL,
    AllotmentLetterPath NVARCHAR(500) NULL,
    AllottedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IsFireCertificateRequired BIT NOT NULL DEFAULT 1,
    FireCertificateDueBy DATETIME2 NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Active', -- Active, Cancelled, Completed
    CONSTRAINT FK_Allotment_Bid FOREIGN KEY (BidID) REFERENCES dbo.BidApplication(BidID),
    CONSTRAINT FK_Allotment_Shop FOREIGN KEY (ShopUnitID) REFERENCES dbo.ShopUnit(ShopUnitID),
    CONSTRAINT FK_Allotment_User FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID)
);
GO

CREATE INDEX IX_Allotment_ShopID ON dbo.Allotment(ShopUnitID);
CREATE INDEX IX_Allotment_UserID ON dbo.Allotment(UserID);

/* ---------------------------
   11. Fire Safety Certificate
   --------------------------- */
CREATE TABLE dbo.FireSafetyCertificate (
    CertificateID BIGINT IDENTITY(1,1) PRIMARY KEY,
    AllotmentID BIGINT NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    UploadedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UploadedBy BIGINT NULL,
    VerifiedByAdmin BIT NOT NULL DEFAULT 0,
    VerifiedAt DATETIME2 NULL,
    CONSTRAINT FK_FireCert_Allotment FOREIGN KEY (AllotmentID) REFERENCES dbo.Allotment(AllotmentID)
);
GO

CREATE INDEX IX_FireSafetyCertificate_AllotmentID ON dbo.FireSafetyCertificate(AllotmentID);

/* ---------------------------
   12. Vendor History & Blacklist
   --------------------------- */
CREATE TABLE dbo.VendorHistory (
    HistoryID BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserID BIGINT NOT NULL,
    FairID BIGINT NULL,
    Notes NVARCHAR(2000) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy BIGINT NULL,
    CONSTRAINT FK_VendorHistory_User FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID),
    CONSTRAINT FK_VendorHistory_Fair FOREIGN KEY (FairID) REFERENCES dbo.TradeFair(FairID)
);
GO

CREATE TABLE dbo.Blacklist (
    BlacklistID BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserID BIGINT NOT NULL,
    Reason NVARCHAR(2000) NULL,
    ActionDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IsActive BIT NOT NULL DEFAULT 1,
    ActionedBy BIGINT NULL,
    CONSTRAINT FK_Blacklist_User FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID)
);
GO

CREATE INDEX IX_Blacklist_UserID ON dbo.Blacklist(UserID);

/* ---------------------------
   13. Shop Closure, Penalty & Refund
   --------------------------- */
CREATE TABLE dbo.ShopClosure (
    ClosureID BIGINT IDENTITY(1,1) PRIMARY KEY,
    AllotmentID BIGINT NOT NULL,
    DamagePenalty DECIMAL(18,2) NOT NULL DEFAULT 0,
    UtilityDues DECIMAL(18,2) NOT NULL DEFAULT 0,
    RefundableAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    FinalStatus NVARCHAR(100) NOT NULL DEFAULT 'Pending', -- Pending, Settled, Dispute
    ClosedAt DATETIME2 NULL,
    ClosedBy BIGINT NULL,
    CONSTRAINT FK_ShopClosure_Allotment FOREIGN KEY (AllotmentID) REFERENCES dbo.Allotment(AllotmentID)
);
GO

CREATE INDEX IX_ShopClosure_AllotmentID ON dbo.ShopClosure(AllotmentID);

/* Refund transactions */
CREATE TABLE dbo.RefundTransaction (
    RefundID BIGINT IDENTITY(1,1) PRIMARY KEY,
    PaymentID BIGINT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    RefundStatus NVARCHAR(50) NOT NULL, -- Initiated, Processed, Failed
    ProcessedAt DATETIME2 NULL,
    ProcessedBy BIGINT NULL,
    Remarks NVARCHAR(1000) NULL,
    CONSTRAINT FK_Refund_Payment FOREIGN KEY (PaymentID) REFERENCES dbo.PaymentTransaction(PaymentID)
);
GO

CREATE INDEX IX_Refund_PaymentID ON dbo.RefundTransaction(PaymentID);

/* ---------------------------
   14. Notifications (SMS/Email/In-App)
   --------------------------- */
CREATE TABLE dbo.NotificationLog (
    NotificationID BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserID BIGINT NULL,
    Type NVARCHAR(20) NOT NULL, -- SMS, Email, System
    TemplateName NVARCHAR(200) NULL,
    MessageBody NVARCHAR(MAX) NULL,
    Status NVARCHAR(50) NULL, -- Sent, Failed, Pending
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Notification_User FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID)
);
GO

CREATE INDEX IX_NotificationLog_UserID ON dbo.NotificationLog(UserID);

/* ---------------------------
   15. QR Pass Module
   --------------------------- */
CREATE TABLE dbo.QRPass (
    QRPassID BIGINT IDENTITY(1,1) PRIMARY KEY,
    AllotmentID BIGINT NULL,
    PassType NVARCHAR(20) NOT NULL, -- Vehicle, Staff
    QRCodeData NVARCHAR(MAX) NULL,
    QRCodePath NVARCHAR(500) NULL,
    IssuedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IssuedBy BIGINT NULL,
    ValidFrom DATETIME2 NULL,
    ValidTo DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_QRPass_Allotment FOREIGN KEY (AllotmentID) REFERENCES dbo.Allotment(AllotmentID)
);
GO

CREATE INDEX IX_QRPass_AllotmentID ON dbo.QRPass(AllotmentID);

/* ---------------------------
   16. Emergency Contacts & Banners
   --------------------------- */
CREATE TABLE dbo.EmergencyContact (
    ContactID BIGINT IDENTITY(1,1) PRIMARY KEY,
    FairID BIGINT NOT NULL,
    DepartmentName NVARCHAR(200) NOT NULL,
    ContactNumber NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_EmergencyContact_Fair FOREIGN KEY (FairID) REFERENCES dbo.TradeFair(FairID)
);
GO

CREATE TABLE dbo.Banners (
    BannerID BIGINT IDENTITY(1,1) PRIMARY KEY,
    FairID BIGINT NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    Description NVARCHAR(1000) NULL,
    UploadedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UploadedBy BIGINT NULL,
    CONSTRAINT FK_Banners_Fair FOREIGN KEY (FairID) REFERENCES dbo.TradeFair(FairID)
);
GO

CREATE INDEX IX_Banners_FairID ON dbo.Banners(FairID);

/* ---------------------------
   17. Audit Log (Immutable activity logs)
   --------------------------- */
CREATE TABLE dbo.AuditLog (
    AuditID BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserID BIGINT NULL,
    Action NVARCHAR(200) NOT NULL,
    EntityName NVARCHAR(100) NULL,
    EntityID BIGINT NULL,
    OldValue NVARCHAR(MAX) NULL,
    NewValue NVARCHAR(MAX) NULL,
    IpAddress NVARCHAR(100) NULL,
    Timestamp DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

CREATE INDEX IX_AuditLog_UserID ON dbo.AuditLog(UserID);

/* ---------------------------
   18. Misc / Lookup tables (optional but helpful)
   --------------------------- */
CREATE TABLE dbo.Lookup_Categories (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(200) NOT NULL UNIQUE,
    Description NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

CREATE TABLE dbo.Lookup_StatusCodes (
    StatusCode NVARCHAR(50) PRIMARY KEY,
    Description NVARCHAR(500) NULL
);
GO

/* ---------------------------
   19. Helpful Views (examples)
   --------------------------- */
-- Example view: Latest bid per shop per fair (simple)
IF OBJECT_ID('dbo.vw_LatestBidPerShop','V') IS NOT NULL DROP VIEW dbo.vw_LatestBidPerShop;
GO
CREATE VIEW dbo.vw_LatestBidPerShop
AS
SELECT b.FairID, b.ShopUnitID, MAX(b.BidAmount) AS MaxBidAmount
FROM dbo.BidApplication b
WHERE b.Status IN ('Paid','UnderReview','Pending','Allotted')
GROUP BY b.FairID, b.ShopUnitID;
GO

/* ---------------------------
   20. Final index suggestions (add as needed)
   --------------------------- */
-- Add composite index for frequent lookups
CREATE INDEX IX_Bid_FairShopStatus ON dbo.BidApplication(FairID, ShopUnitID, Status);
CREATE INDEX IX_Allotment_FairUser ON dbo.Allotment(UserID, AllotmentID);
GO

PRINT 'MARS database schema created successfully.';
GO
