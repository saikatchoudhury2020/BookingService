USE [BraathenEiendom]
GO

/***** Object:  Table [dbo].[Agreements]    Script Date: 23-10-2019 4.45.23 PM *****/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Agreements](
	[AgreementId] [int] IDENTITY(1,1) NOT NULL,
	[Customer] [int] NULL,
	[Contact] [int] NULL,
	[AgreementTypeId] [int] NULL,
	[FromDate] [datetime] NULL,
	[ToDate] [datetime] NULL,
	[Article] [varchar](max) NULL,
	[Text] [varchar](max) NULL,
	[NumberOfMonths] [int] NULL,
	[Price] [decimal](18, 2) NULL,
 CONSTRAINT [PK_Agreements] PRIMARY KEY CLUSTERED 
(
	[AgreementId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

USE [BraathenEiendom]
GO

/***** Object:  Table [dbo].[AgreementTypes]    Script Date: 23-10-2019 4.46.56 PM *****/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[AgreementTypes](
	[AgreementTypeId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](max) NULL,
	[Article] [varchar](max) NULL,
	[Text] [varchar](max) NULL,
	[Price] [decimal](18, 2) NULL,
	[NumberOfMonths] [int] NULL,
 CONSTRAINT [PK_AgreementTypes] PRIMARY KEY CLUSTERED 
(
	[AgreementTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
