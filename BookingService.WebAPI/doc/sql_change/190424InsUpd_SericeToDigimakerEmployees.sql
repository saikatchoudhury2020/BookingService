USE [BraathenEiendom]
GO
/****** Object:  StoredProcedure [dbo].[InsUpd_SericeToDigimakerEmployees]    Script Date: 24-04-2019 12:04:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[InsUpd_SericeToDigimakerEmployees]
	--@PersonID						INT = NULL OUTPUT,
	--@AdditionalNames		NVARCHAR(100) = NULL, 
	--@NamePrefix					NVARCHAR(100) = NULL, 
	--@NameSuffix					NVARCHAR(100) = NULL, 
	
	@GivenName					NVARCHAR(100) = NULL, 
	
	@EmployeeNo					NVARCHAR(20) = NULL,
	@DisplayName				NVARCHAR(100) = NULL, 
	@FamilyName					NVARCHAR(100) = NULL, 
	@LoginName					NVARCHAR(100) = NULL, 
	--@NickName						NVARCHAR(100) = NULL, 
	@PasswordHash				VARCHAR(50) = NULL, 
	--@Priority						INT = NULL,
	@PasswordSalt				VARCHAR(50) = NULL, 
	--@Title							NVARCHAR(100) = NULL, 
	--@OfficeFunction			NVARCHAR(255) = NULL, 
	--@Birthdate					DATETIME = NULL, 
	--@DateOfJoining			DATETIME = NULL, 
	@CanReceiveHTMLMail	BIT = NULL, 
	--@IMAddress					NVARCHAR(100) = NULL, 
	@Email							NVARCHAR(175) = NULL, 
	--@Email2							NVARCHAR(175) = NULL, 
	--@Email3							NVARCHAR(175) = NULL, 
	--@Url								NVARCHAR(255) = NULL, 
	--@Fax								VARCHAR(25) = NULL, 
	--@SMSNumber					VARCHAR(20) = NULL, 
	--@WorkPhone					VARCHAR(25) = NULL, 
	--@Telephone2					VARCHAR(25) = NULL, 
	--@Telephone3					VARCHAR(25) = NULL, 
	@MobilePhone				VARCHAR(25) = NULL, 
	--@HomePhone					VARCHAR(25) = NULL, 
	--@Status							TINYINT = 0, 
	--@MartialStatus			NVARCHAR(20) = NULL, 
	--@PartnerName				NVARCHAR(100) = NULL, 
	--@PartnersBirthdate	DATETIME = NULL, 
	--@Comment						NVARCHAR(250) = NULL, 
	--@Motto							NVARCHAR(150) = NULL, 
	--@Education					NVARCHAR(150) = NULL, 
	--@TimeZone						DECIMAL(4,2) = NULL, 
	--@Course							NVARCHAR(150) = NULL, 
	--@Expertise					NVARCHAR(150) = NULL, 
	--@Certification			NVARCHAR(150) = NULL,	 
	@Custom1						NVARCHAR(100) = NULL, 
	@PictureID					INT = NULL, 
	--@Custom2						NVARCHAR(100) = NULL, 
	--@Custom3						NVARCHAR(100) = NULL,
	--@Modified						DATETIME = NULL OUTPUT,
	--@LongDescription		NTEXT = NULL,
	--@RTL								BIT = NULL,
	--@Culture						VARCHAR(10) = NULL,
	--@UICulture					VARCHAR(10) = NULL,
	--@WorkingHoursStart	VARCHAR(4) = NULL,
	--@WorkingHoursEnd		VARCHAR(4) = NULL,
	--@ExternalId					NVARCHAR(200) = NULL,
	--@PasswordExpiryDate	DATETIME =NULL,
	--@ExpireDate					DATETIME =NULL
@RoleID INT,
@OrganizationUnitID INT,
@PersonNumber int Output
 AS
DECLARE  @newId int
DECLARE  @PersonID int 

    IF EXISTS (SELECT * FROM dbo.Person WHERE LoginName = @LoginName)
        BEGIN
            UPDATE dbo.Person  SET GivenName=@GivenName,FamilyName=@FamilyName,LoginName=@LoginName,@PasswordHash=PasswordHash,DisplayName = @DisplayName, PictureID = @PictureID WHERE  LoginName = @LoginName
         select @PersonID = PersonID from Person where LoginName=@LoginName
         UPDATE dbo.AccessRole_Person  SET RoleID = @RoleID WHERE PersonID=@PersonID and RoleID = @RoleID
        END
    ELSE
        BEGIN
            INSERT INTO dbo.Person (GivenName, FamilyName, DisplayName, LoginName, PasswordHash, PasswordSalt, CanReceiveHTMLMail, Email,EmployeeNo,Custom1,MobilePhone, PictureID) 
VALUES (@GivenName, @FamilyName, @DisplayName, @LoginName, @PasswordHash, @PasswordSalt, 1, @Email,@EmployeeNo,@Custom1,@MobilePhone, @PictureID)
select @newId = Scope_Identity() 
INSERT INTO dbo.AccessRole_Person (PersonID, RoleID) VALUES (@newId,@RoleID)
INSERT INTO dbo.OrganizationUnit_Person (PersonID,	OrganizationUnitID,	AssociationID,	Priority) VALUES (@newId,	@OrganizationUnitID,	11,	1)
    SELECT  @PersonNumber  = @newId
	    END 

