USE [BraathenEiendom]
GO

/****** Object:  Table [dbo].[Perferences]    Script Date: 16.10.2018 13:40:23 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Perferences](
	[UserId] [int] NOT NULL,
	[Settings] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_Perferences] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO