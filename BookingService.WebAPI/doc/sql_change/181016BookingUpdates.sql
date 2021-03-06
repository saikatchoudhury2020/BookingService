/****** Script for SelectTopNRows command from SSMS  ******/
SELECT *
  FROM [BraathenEiendom].[dbo].[BookingDetail]


ALTER TABLE [BraathenEiendom].[dbo].[BookingDetail]
ADD FollowUpDate datetime;

ALTER TABLE [BraathenEiendom].[dbo].[BookingDetail]
ADD Customer int;
ALTER TABLE [BraathenEiendom].[dbo].[BookingDetail]
ADD Ordering int;


ALTER TABLE [BraathenEiendom].[dbo].[FoodService]
ADD ArticleId int;
ALTER TABLE [BraathenEiendom].[dbo].[FoodService]
ADD Tekst nvarchar(500);
ALTER TABLE [BraathenEiendom].[dbo].[FoodService]
ADD [Sum] int;
ALTER TABLE [BraathenEiendom].[dbo].[FoodService]
ADD IsKitchen bit;





USE [BraathenEiendom]
GO

/****** Object:  Table [dbo].[Perferences]    Script Date: 16.10.2018 13:40:23 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Perferences](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[Settings] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_Perferences] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO