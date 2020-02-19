
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 10/16/2018 15:42:01
-- Generated from EDMX file: F:\git code\braathen\BookingService.WebAPI\Models\BookingModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [BraathenEiendom];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_FoodService_BookingDetail]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[FoodService] DROP CONSTRAINT [FK_FoodService_BookingDetail];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Article_Article]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Article_Article];
GO
IF OBJECT_ID(N'[dbo].[Article_Banner]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Article_Banner];
GO
IF OBJECT_ID(N'[dbo].[Article_MenuItem]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Article_MenuItem];
GO
IF OBJECT_ID(N'[dbo].[BookingDetail]', 'U') IS NOT NULL
    DROP TABLE [dbo].[BookingDetail];
GO
IF OBJECT_ID(N'[dbo].[FoodService]', 'U') IS NOT NULL
    DROP TABLE [dbo].[FoodService];
GO
IF OBJECT_ID(N'[dbo].[Orderlines]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Orderlines];
GO
IF OBJECT_ID(N'[dbo].[OrganizationUnit_Person]', 'U') IS NOT NULL
    DROP TABLE [dbo].[OrganizationUnit_Person];
GO
IF OBJECT_ID(N'[dbo].[Perferences]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Perferences];
GO
IF OBJECT_ID(N'[dbo].[UserGroup_User]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserGroup_User];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'MenuItems'
CREATE TABLE [dbo].[MenuItems] (
    [MenuItemID] int IDENTITY(1,1) NOT NULL,
    [MenuItemName] nvarchar(255)  NOT NULL,
    [Parentid] int  NULL,
    [Status] tinyint  NOT NULL,
    [Priority] int  NOT NULL,
    [Title] nvarchar(100)  NULL,
    [Protected] smallint  NOT NULL,
    [TemplateID] int  NOT NULL,
    [Type] tinyint  NOT NULL,
    [Showdate] datetime  NOT NULL,
    [Expiredate] datetime  NULL,
    [URL] nvarchar(500)  NULL,
    [Target] nvarchar(20)  NULL,
    [Pictureid] int  NULL,
    [Sortorder] varchar(30)  NOT NULL,
    [DataObjectID] int  NULL,
    [DataObjectRecordID] int  NULL,
    [Lvl] int  NULL,
    [hierarchy] varchar(900)  NULL,
    [Modified] datetime  NOT NULL,
    [Fullpath] nvarchar(900)  NULL,
    [MetaKey] nvarchar(200)  NULL,
    [MetaDescription] nvarchar(300)  NULL,
    [Alias] nvarchar(25)  NULL,
    [TypeHierarchy] varchar(100)  NULL,
    [AliasHierarchy] nvarchar(900)  NULL,
    [LCID] int  NULL,
    [RSSEnabled] smallint  NOT NULL,
    [RSSMaxNoOfArticles] int  NULL,
    [ArticleTemplateID] int  NULL
);
GO

-- Creating table 'People'
CREATE TABLE [dbo].[People] (
    [PersonID] int IDENTITY(1,1) NOT NULL,
    [GivenName] nvarchar(100)  NULL,
    [FamilyName] nvarchar(100)  NULL,
    [AdditionalNames] nvarchar(100)  NULL,
    [NamePrefix] nvarchar(100)  NULL,
    [NameSuffix] nvarchar(100)  NULL,
    [NickName] nvarchar(100)  NULL,
    [DisplayName] nvarchar(100)  NULL,
    [LoginName] nvarchar(100)  NULL,
    [Title] nvarchar(100)  NULL,
    [OfficeFunction] nvarchar(255)  NULL,
    [PictureID] int  NULL,
    [EmployeeNo] nvarchar(20)  NULL,
    [PasswordHash] varchar(50)  NULL,
    [PasswordSalt] varchar(50)  NULL,
    [Priority] int  NULL,
    [Birthdate] datetime  NULL,
    [DateOfJoining] datetime  NULL,
    [CanReceiveHTMLMail] bit  NOT NULL,
    [IMAddress] nvarchar(100)  NULL,
    [Email] nvarchar(175)  NULL,
    [Email2] nvarchar(175)  NULL,
    [Email3] nvarchar(175)  NULL,
    [Url] nvarchar(255)  NULL,
    [Fax] varchar(25)  NULL,
    [SMSNumber] varchar(20)  NULL,
    [WorkPhone] varchar(25)  NULL,
    [Telephone2] varchar(25)  NULL,
    [Telephone3] varchar(25)  NULL,
    [MobilePhone] varchar(25)  NULL,
    [HomePhone] varchar(25)  NULL,
    [MartialStatus] nvarchar(20)  NULL,
    [PartnerName] nvarchar(100)  NULL,
    [PartnersBirthdate] datetime  NULL,
    [Comment] nvarchar(250)  NULL,
    [Motto] nvarchar(150)  NULL,
    [Education] nvarchar(150)  NULL,
    [TimeZone] decimal(4,2)  NULL,
    [Course] nvarchar(150)  NULL,
    [Expertise] nvarchar(150)  NULL,
    [Certification] nvarchar(150)  NULL,
    [LongDescription] nvarchar(max)  NULL,
    [Status] tinyint  NOT NULL,
    [Modified] datetime  NOT NULL,
    [GUID] uniqueidentifier  NULL,
    [Custom1] nvarchar(100)  NULL,
    [Custom2] nvarchar(100)  NULL,
    [Custom3] nvarchar(100)  NULL,
    [RTL] bit  NOT NULL,
    [Culture] varchar(10)  NULL,
    [UICulture] varchar(10)  NULL,
    [WorkingHoursStart] varchar(4)  NULL,
    [WorkingHoursEnd] varchar(4)  NULL,
    [ExternalID] nvarchar(100)  NULL,
    [PasswordExpiryDate] datetime  NULL,
    [ExpireDate] datetime  NULL,
    [Theme] int  NULL,
    [DeviceId] nvarchar(max)  NULL,
    [LastLogin] datetime  NULL,
    [PlatformType] int  NULL
);
GO

-- Creating table 'OrganizationUnits'
CREATE TABLE [dbo].[OrganizationUnits] (
    [OrganizationUnitID] int IDENTITY(1,1) NOT NULL,
    [OrganizationUnitName] nvarchar(255)  NULL,
    [Type] tinyint  NOT NULL,
    [PictureID] int  NULL,
    [Url] nvarchar(255)  NULL,
    [Code] nvarchar(20)  NULL,
    [OrgNumber] nvarchar(50)  NULL,
    [RefNumber] nvarchar(20)  NULL,
    [Telephone] varchar(25)  NULL,
    [Telephone2] varchar(25)  NULL,
    [Fax] varchar(25)  NULL,
    [Email] nvarchar(255)  NULL,
    [Email2] nvarchar(255)  NULL,
    [Branch] tinyint  NULL,
    [Priority] int  NOT NULL,
    [Comment] nvarchar(max)  NULL,
    [LongDescription] nvarchar(max)  NULL,
    [ParentID] int  NULL,
    [Lvl] int  NULL,
    [Hierarchy] varchar(900)  NULL,
    [FullPath] nvarchar(900)  NULL,
    [Status] tinyint  NOT NULL,
    [Custom1] nvarchar(255)  NULL,
    [Custom2] nvarchar(255)  NULL,
    [Custom3] nvarchar(255)  NULL,
    [Homepage] nvarchar(max)  NULL,
    [ExternalID] nvarchar(100)  NULL,
    [WorkingHoursStart] varchar(4)  NULL,
    [WorkingHoursEnd] varchar(4)  NULL,
    [Modified] datetime  NOT NULL
);
GO

-- Creating table 'OrganizationUnit_Person'
CREATE TABLE [dbo].[OrganizationUnit_Person] (
    [ObjectID] int IDENTITY(1,1) NOT NULL,
    [PersonID] int  NOT NULL,
    [OrganizationUnitID] int  NOT NULL,
    [AssociationID] int  NOT NULL,
    [Priority] int  NOT NULL,
    [Modified] datetime  NOT NULL
);
GO

-- Creating table 'PictureProperties'
CREATE TABLE [dbo].[PictureProperties] (
    [PicturePropertyID] int IDENTITY(1,1) NOT NULL,
    [PictureMainID] int  NOT NULL,
    [Filepath] nvarchar(255)  NULL,
    [Height] int  NOT NULL,
    [Width] int  NOT NULL,
    [FileSize] int  NULL,
    [Modified] datetime  NULL,
    [IsDefault] bit  NOT NULL,
    [IsThumbnail] bit  NOT NULL,
    [IsOriginal] bit  NOT NULL,
    [Comment] nvarchar(255)  NULL,
    [PersonID] int  NULL
);
GO

-- Creating table 'Article_Banner'
CREATE TABLE [dbo].[Article_Banner] (
    [ObjectId] int IDENTITY(1,1) NOT NULL,
    [ArticleId] int  NOT NULL,
    [BannerId] int  NOT NULL,
    [AssociationId] int  NOT NULL,
    [Priority] int  NOT NULL,
    [Modified] datetime  NOT NULL
);
GO

-- Creating table 'Banners'
CREATE TABLE [dbo].[Banners] (
    [BannerId] int IDENTITY(1,1) NOT NULL,
    [BannerName] nvarchar(50)  NOT NULL,
    [CategoryId] int  NOT NULL,
    [ShowDate] datetime  NOT NULL,
    [ExpireDate] datetime  NULL,
    [Status] smallint  NOT NULL,
    [Impressions] int  NOT NULL,
    [MaxImpressions] int  NULL,
    [Clicks] int  NOT NULL,
    [MaxClicks] int  NULL,
    [DestinationLinkId] int  NULL,
    [ContentType] tinyint  NOT NULL,
    [PictureId] int  NULL,
    [PictureLinkId] int  NULL,
    [HtmlText] nvarchar(3000)  NULL,
    [DocumentId] int  NULL,
    [Width] int  NOT NULL,
    [Height] int  NOT NULL,
    [FlashVersion] smallint  NOT NULL,
    [DataObjectId] int  NULL,
    [DataObjectRecordId] int  NULL,
    [MetaKeywords] nvarchar(300)  NULL,
    [PersonId] int  NOT NULL,
    [Created] datetime  NOT NULL,
    [Modified] datetime  NOT NULL
);
GO

-- Creating table 'UserGroup_User'
CREATE TABLE [dbo].[UserGroup_User] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserGroupId] int  NULL,
    [UserId] int  NULL
);
GO

-- Creating table 'UserGroups'
CREATE TABLE [dbo].[UserGroups] (
    [UserGroupId] int IDENTITY(1,1) NOT NULL,
    [UserGroupName] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'Articles'
CREATE TABLE [dbo].[Articles] (
    [ArticleID] int IDENTITY(1,1) NOT NULL,
    [MainID] int  NULL,
    [Version] smallint  NOT NULL,
    [Status] smallint  NOT NULL,
    [PublishStatus] smallint  NOT NULL,
    [PersonID] int  NOT NULL,
    [Headline] nvarchar(255)  NOT NULL,
    [CreateDate] datetime  NOT NULL,
    [ShowDate] datetime  NOT NULL,
    [ExpireDate] datetime  NULL,
    [Abstract] nvarchar(max)  NULL,
    [Fullstory] nvarchar(max)  NULL,
    [TOC] nvarchar(max)  NULL,
    [Priority] int  NOT NULL,
    [Author] nvarchar(100)  NULL,
    [Discuss] bit  NOT NULL,
    [PictureID] int  NULL,
    [meta_description] nvarchar(255)  NULL,
    [meta_keywords] nvarchar(255)  NULL,
    [LockStatus] bit  NOT NULL,
    [LockTime] datetime  NULL,
    [LockedBy] int  NULL,
    [DataObjectID] int  NULL,
    [DataObjectRecordID] int  NULL,
    [Modified] datetime  NOT NULL,
    [Alias] nvarchar(25)  NULL,
    [CompleteArticle] nvarchar(max)  NULL,
    [PictureDescription] nvarchar(255)  NULL
);
GO

-- Creating table 'Article_MenuItem'
CREATE TABLE [dbo].[Article_MenuItem] (
    [ObjectID] int IDENTITY(1,1) NOT NULL,
    [AssociationID] int  NOT NULL,
    [ArticleID] int  NOT NULL,
    [MainID] int  NOT NULL,
    [MenuItemID] int  NOT NULL,
    [Modified] datetime  NOT NULL,
    [Priority] int  NOT NULL
);
GO

-- Creating table 'Article_Article'
CREATE TABLE [dbo].[Article_Article] (
    [ObjectID] int IDENTITY(1,1) NOT NULL,
    [MainID] int  NOT NULL,
    [RelatedMainID] int  NOT NULL,
    [AssociationID] int  NOT NULL,
    [Modified] datetime  NOT NULL,
    [Priority] int  NOT NULL
);
GO

-- Creating table 'Orderheads'
CREATE TABLE [dbo].[Orderheads] (
    [OrderNo] int IDENTITY(1,1) NOT NULL,
    [Orderdate] datetime  NOT NULL,
    [CustomerName] nvarchar(500)  NOT NULL,
    [CustomerNo] nvarchar(500)  NOT NULL,
    [OurReference] nvarchar(255)  NULL,
    [YourReference] nvarchar(255)  NULL,
    [EmailAddress] nvarchar(255)  NULL,
    [VAT] int  NOT NULL,
    [Ordertype] nvarchar(250)  NOT NULL,
    [ERPClient] nvarchar(255)  NULL,
    [Status] nvarchar(255)  NOT NULL
);
GO

-- Creating table 'Orderlines'
CREATE TABLE [dbo].[Orderlines] (
    [OrderlineNo] int IDENTITY(1,1) NOT NULL,
    [OrderHeadId] int  NOT NULL,
    [Article] nvarchar(500)  NOT NULL,
    [Text] nvarchar(500)  NOT NULL,
    [UnitText] nvarchar(100)  NOT NULL,
    [Quantity] float  NOT NULL,
    [UnitPrice] float  NOT NULL,
    [DiscountPercent] float  NOT NULL,
    [NetAmount] float  NOT NULL,
    [VATPercent] float  NOT NULL,
    [Amount] float  NOT NULL
);
GO

-- Creating table 'Perferences'
CREATE TABLE [dbo].[Perferences] (
    [UserId] int IDENTITY(1,1) NOT NULL,
    [Settings] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'BookingDetails'
CREATE TABLE [dbo].[BookingDetails] (
    [BookingID] int IDENTITY(1,1) NOT NULL,
    [BookingType] nvarchar(255)  NOT NULL,
    [ServiceID] int  NOT NULL,
    [UserID] int  NOT NULL,
    [BookingName] nvarchar(500)  NOT NULL,
    [NoOfPeople] int  NULL,
    [FromDate] datetime  NULL,
    [ToDate] datetime  NULL,
    [CreatedOn] datetime  NOT NULL,
    [IsFoodOrder] bit  NOT NULL,
    [Status] int  NULL,
    [BuildingID] int  NOT NULL,
    [ServiceType] int  NOT NULL,
    [SendMessageType] int  NOT NULL,
    [UnitText] nvarchar(200)  NULL,
    [Quantity] float  NULL,
    [UnitPrice] float  NULL,
    [DiscountPercent] float  NULL,
    [NetAmount] float  NULL,
    [VATPercent] float  NULL,
    [Amount] float  NULL,
    [MainBookingId] int  NULL,
    [FollowUpDate] datetime  NULL,
    [Ordering] int  NULL,
    [Customer] int  NULL
);
GO

-- Creating table 'FoodServices'
CREATE TABLE [dbo].[FoodServices] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [FoodID] int  NOT NULL,
    [Qty] int  NOT NULL,
    [Price] decimal(18,2)  NOT NULL,
    [BookingID] int  NOT NULL,
    [Status] bit  NOT NULL,
    [ArticleId] int  NULL,
    [Tekst] nvarchar(500)  NULL,
    [Sum] int  NULL,
    [IsKitchen] bit  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [MenuItemID] in table 'MenuItems'
ALTER TABLE [dbo].[MenuItems]
ADD CONSTRAINT [PK_MenuItems]
    PRIMARY KEY CLUSTERED ([MenuItemID] ASC);
GO

-- Creating primary key on [PersonID] in table 'People'
ALTER TABLE [dbo].[People]
ADD CONSTRAINT [PK_People]
    PRIMARY KEY CLUSTERED ([PersonID] ASC);
GO

-- Creating primary key on [OrganizationUnitID] in table 'OrganizationUnits'
ALTER TABLE [dbo].[OrganizationUnits]
ADD CONSTRAINT [PK_OrganizationUnits]
    PRIMARY KEY CLUSTERED ([OrganizationUnitID] ASC);
GO

-- Creating primary key on [ObjectID] in table 'OrganizationUnit_Person'
ALTER TABLE [dbo].[OrganizationUnit_Person]
ADD CONSTRAINT [PK_OrganizationUnit_Person]
    PRIMARY KEY CLUSTERED ([ObjectID] ASC);
GO

-- Creating primary key on [PicturePropertyID] in table 'PictureProperties'
ALTER TABLE [dbo].[PictureProperties]
ADD CONSTRAINT [PK_PictureProperties]
    PRIMARY KEY CLUSTERED ([PicturePropertyID] ASC);
GO

-- Creating primary key on [ObjectId] in table 'Article_Banner'
ALTER TABLE [dbo].[Article_Banner]
ADD CONSTRAINT [PK_Article_Banner]
    PRIMARY KEY CLUSTERED ([ObjectId] ASC);
GO

-- Creating primary key on [BannerId] in table 'Banners'
ALTER TABLE [dbo].[Banners]
ADD CONSTRAINT [PK_Banners]
    PRIMARY KEY CLUSTERED ([BannerId] ASC);
GO

-- Creating primary key on [Id] in table 'UserGroup_User'
ALTER TABLE [dbo].[UserGroup_User]
ADD CONSTRAINT [PK_UserGroup_User]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [UserGroupId] in table 'UserGroups'
ALTER TABLE [dbo].[UserGroups]
ADD CONSTRAINT [PK_UserGroups]
    PRIMARY KEY CLUSTERED ([UserGroupId] ASC);
GO

-- Creating primary key on [ArticleID] in table 'Articles'
ALTER TABLE [dbo].[Articles]
ADD CONSTRAINT [PK_Articles]
    PRIMARY KEY CLUSTERED ([ArticleID] ASC);
GO

-- Creating primary key on [ObjectID] in table 'Article_MenuItem'
ALTER TABLE [dbo].[Article_MenuItem]
ADD CONSTRAINT [PK_Article_MenuItem]
    PRIMARY KEY CLUSTERED ([ObjectID] ASC);
GO

-- Creating primary key on [ObjectID] in table 'Article_Article'
ALTER TABLE [dbo].[Article_Article]
ADD CONSTRAINT [PK_Article_Article]
    PRIMARY KEY CLUSTERED ([ObjectID] ASC);
GO

-- Creating primary key on [OrderNo] in table 'Orderheads'
ALTER TABLE [dbo].[Orderheads]
ADD CONSTRAINT [PK_Orderheads]
    PRIMARY KEY CLUSTERED ([OrderNo] ASC);
GO

-- Creating primary key on [OrderlineNo] in table 'Orderlines'
ALTER TABLE [dbo].[Orderlines]
ADD CONSTRAINT [PK_Orderlines]
    PRIMARY KEY CLUSTERED ([OrderlineNo] ASC);
GO

-- Creating primary key on [UserId] in table 'Perferences'
ALTER TABLE [dbo].[Perferences]
ADD CONSTRAINT [PK_Perferences]
    PRIMARY KEY CLUSTERED ([UserId] ASC);
GO

-- Creating primary key on [BookingID] in table 'BookingDetails'
ALTER TABLE [dbo].[BookingDetails]
ADD CONSTRAINT [PK_BookingDetails]
    PRIMARY KEY CLUSTERED ([BookingID] ASC);
GO

-- Creating primary key on [ID] in table 'FoodServices'
ALTER TABLE [dbo].[FoodServices]
ADD CONSTRAINT [PK_FoodServices]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Parentid] in table 'MenuItems'
ALTER TABLE [dbo].[MenuItems]
ADD CONSTRAINT [FK__MenuItem__Parent__15FC9BF3]
    FOREIGN KEY ([Parentid])
    REFERENCES [dbo].[MenuItems]
        ([MenuItemID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__MenuItem__Parent__15FC9BF3'
CREATE INDEX [IX_FK__MenuItem__Parent__15FC9BF3]
ON [dbo].[MenuItems]
    ([Parentid]);
GO

-- Creating foreign key on [OrganizationUnitID] in table 'OrganizationUnit_Person'
ALTER TABLE [dbo].[OrganizationUnit_Person]
ADD CONSTRAINT [FK__Organizat__Organ__2EC849BD]
    FOREIGN KEY ([OrganizationUnitID])
    REFERENCES [dbo].[OrganizationUnits]
        ([OrganizationUnitID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__Organizat__Organ__2EC849BD'
CREATE INDEX [IX_FK__Organizat__Organ__2EC849BD]
ON [dbo].[OrganizationUnit_Person]
    ([OrganizationUnitID]);
GO

-- Creating foreign key on [PersonID] in table 'OrganizationUnit_Person'
ALTER TABLE [dbo].[OrganizationUnit_Person]
ADD CONSTRAINT [FK__Organizat__Perso__2CE0014B]
    FOREIGN KEY ([PersonID])
    REFERENCES [dbo].[People]
        ([PersonID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__Organizat__Perso__2CE0014B'
CREATE INDEX [IX_FK__Organizat__Perso__2CE0014B]
ON [dbo].[OrganizationUnit_Person]
    ([PersonID]);
GO

-- Creating foreign key on [Pictureid] in table 'MenuItems'
ALTER TABLE [dbo].[MenuItems]
ADD CONSTRAINT [FK__MenuItem__Pictur__13202F48]
    FOREIGN KEY ([Pictureid])
    REFERENCES [dbo].[PictureProperties]
        ([PicturePropertyID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__MenuItem__Pictur__13202F48'
CREATE INDEX [IX_FK__MenuItem__Pictur__13202F48]
ON [dbo].[MenuItems]
    ([Pictureid]);
GO

-- Creating foreign key on [PictureID] in table 'OrganizationUnits'
ALTER TABLE [dbo].[OrganizationUnits]
ADD CONSTRAINT [FK__Organizat__Pictu__2A0394A0]
    FOREIGN KEY ([PictureID])
    REFERENCES [dbo].[PictureProperties]
        ([PicturePropertyID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__Organizat__Pictu__2A0394A0'
CREATE INDEX [IX_FK__Organizat__Pictu__2A0394A0]
ON [dbo].[OrganizationUnits]
    ([PictureID]);
GO

-- Creating foreign key on [PictureID] in table 'People'
ALTER TABLE [dbo].[People]
ADD CONSTRAINT [FK__Person__PictureI__31A4B668]
    FOREIGN KEY ([PictureID])
    REFERENCES [dbo].[PictureProperties]
        ([PicturePropertyID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__Person__PictureI__31A4B668'
CREATE INDEX [IX_FK__Person__PictureI__31A4B668]
ON [dbo].[People]
    ([PictureID]);
GO

-- Creating foreign key on [PersonID] in table 'PictureProperties'
ALTER TABLE [dbo].[PictureProperties]
ADD CONSTRAINT [FK__PicturePr__Perso__28C557A4]
    FOREIGN KEY ([PersonID])
    REFERENCES [dbo].[People]
        ([PersonID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__PicturePr__Perso__28C557A4'
CREATE INDEX [IX_FK__PicturePr__Perso__28C557A4]
ON [dbo].[PictureProperties]
    ([PersonID]);
GO

-- Creating foreign key on [BannerId] in table 'Article_Banner'
ALTER TABLE [dbo].[Article_Banner]
ADD CONSTRAINT [FK_Article_Banner_Banner]
    FOREIGN KEY ([BannerId])
    REFERENCES [dbo].[Banners]
        ([BannerId])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Article_Banner_Banner'
CREATE INDEX [IX_FK_Article_Banner_Banner]
ON [dbo].[Article_Banner]
    ([BannerId]);
GO

-- Creating foreign key on [PictureId] in table 'Banners'
ALTER TABLE [dbo].[Banners]
ADD CONSTRAINT [FK__Banner__Picturei__411C0422]
    FOREIGN KEY ([PictureId])
    REFERENCES [dbo].[PictureProperties]
        ([PicturePropertyID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__Banner__Picturei__411C0422'
CREATE INDEX [IX_FK__Banner__Picturei__411C0422]
ON [dbo].[Banners]
    ([PictureId]);
GO

-- Creating foreign key on [PersonId] in table 'Banners'
ALTER TABLE [dbo].[Banners]
ADD CONSTRAINT [FK_Banner_Person]
    FOREIGN KEY ([PersonId])
    REFERENCES [dbo].[People]
        ([PersonID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Banner_Person'
CREATE INDEX [IX_FK_Banner_Person]
ON [dbo].[Banners]
    ([PersonId]);
GO

-- Creating foreign key on [MainID] in table 'Articles'
ALTER TABLE [dbo].[Articles]
ADD CONSTRAINT [FK__Article__MainID__275C321F]
    FOREIGN KEY ([MainID])
    REFERENCES [dbo].[Articles]
        ([ArticleID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__Article__MainID__275C321F'
CREATE INDEX [IX_FK__Article__MainID__275C321F]
ON [dbo].[Articles]
    ([MainID]);
GO

-- Creating foreign key on [ArticleId] in table 'Article_Banner'
ALTER TABLE [dbo].[Article_Banner]
ADD CONSTRAINT [FK_Article_Banner_Article]
    FOREIGN KEY ([ArticleId])
    REFERENCES [dbo].[Articles]
        ([ArticleID])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Article_Banner_Article'
CREATE INDEX [IX_FK_Article_Banner_Article]
ON [dbo].[Article_Banner]
    ([ArticleId]);
GO

-- Creating foreign key on [PictureID] in table 'Articles'
ALTER TABLE [dbo].[Articles]
ADD CONSTRAINT [FK_Article_PictureProperty]
    FOREIGN KEY ([PictureID])
    REFERENCES [dbo].[PictureProperties]
        ([PicturePropertyID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Article_PictureProperty'
CREATE INDEX [IX_FK_Article_PictureProperty]
ON [dbo].[Articles]
    ([PictureID]);
GO

-- Creating foreign key on [ArticleID] in table 'Article_MenuItem'
ALTER TABLE [dbo].[Article_MenuItem]
ADD CONSTRAINT [FK_Article_MenuItem_Article]
    FOREIGN KEY ([ArticleID])
    REFERENCES [dbo].[Articles]
        ([ArticleID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Article_MenuItem_Article'
CREATE INDEX [IX_FK_Article_MenuItem_Article]
ON [dbo].[Article_MenuItem]
    ([ArticleID]);
GO

-- Creating foreign key on [MainID] in table 'Article_MenuItem'
ALTER TABLE [dbo].[Article_MenuItem]
ADD CONSTRAINT [FK_Article_MenuItem_Article1]
    FOREIGN KEY ([MainID])
    REFERENCES [dbo].[Articles]
        ([ArticleID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Article_MenuItem_Article1'
CREATE INDEX [IX_FK_Article_MenuItem_Article1]
ON [dbo].[Article_MenuItem]
    ([MainID]);
GO

-- Creating foreign key on [MenuItemID] in table 'Article_MenuItem'
ALTER TABLE [dbo].[Article_MenuItem]
ADD CONSTRAINT [FK_Article_MenuItem_MenuItem]
    FOREIGN KEY ([MenuItemID])
    REFERENCES [dbo].[MenuItems]
        ([MenuItemID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Article_MenuItem_MenuItem'
CREATE INDEX [IX_FK_Article_MenuItem_MenuItem]
ON [dbo].[Article_MenuItem]
    ([MenuItemID]);
GO

-- Creating foreign key on [OrderHeadId] in table 'Orderlines'
ALTER TABLE [dbo].[Orderlines]
ADD CONSTRAINT [FK_Orderlines_OrderHead]
    FOREIGN KEY ([OrderHeadId])
    REFERENCES [dbo].[Orderheads]
        ([OrderNo])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Orderlines_OrderHead'
CREATE INDEX [IX_FK_Orderlines_OrderHead]
ON [dbo].[Orderlines]
    ([OrderHeadId]);
GO

-- Creating foreign key on [BookingID] in table 'FoodServices'
ALTER TABLE [dbo].[FoodServices]
ADD CONSTRAINT [FK_FoodService_BookingDetail]
    FOREIGN KEY ([BookingID])
    REFERENCES [dbo].[BookingDetails]
        ([BookingID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FoodService_BookingDetail'
CREATE INDEX [IX_FK_FoodService_BookingDetail]
ON [dbo].[FoodServices]
    ([BookingID]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------