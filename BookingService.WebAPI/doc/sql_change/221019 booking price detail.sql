USE [BraathenEiendom]
GO

/****** Object:  Table [dbo].[BookingPriceDetail]    Script Date: 22-10-2019 21:48:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BookingPriceDetail](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IsInternal] [bit] NOT NULL,
	[Article1] [varchar](50) NOT NULL,
	[Article2] [varchar](50) NOT NULL,
	[UnitId] [int] NOT NULL,
	[UnitDesc] [varchar](150) NOT NULL,
	[From] [int] NOT NULL,
	[To] [int] NOT NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[Status] [bit] NOT NULL,
	[CreatedDate] [datetime] NULL,
 CONSTRAINT [PK_BookingPriceDetail] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


