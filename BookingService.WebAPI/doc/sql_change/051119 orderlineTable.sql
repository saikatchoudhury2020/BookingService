USE [BraathenEiendom]
GO

ALTER TABLE [dbo].[Orderlines] DROP CONSTRAINT [FK_Orderline_Orderhead]
GO

ALTER TABLE [dbo].[Orderlines] DROP CONSTRAINT [DF__Orderline__CostT__5A261260]
GO

ALTER TABLE [dbo].[Orderlines] DROP CONSTRAINT [DF__Orderline__CostP__5931EE27]
GO

/****** Object:  Table [dbo].[Orderlines]    Script Date: 04-11-2019 19:23:17 ******/
DROP TABLE [dbo].[Orderlines]
GO

/****** Object:  Table [dbo].[Orderlines]    Script Date: 04-11-2019 19:23:17 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Orderlines](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FoodID] [int] NOT NULL,
	[Qty] [decimal](18, 2) NOT NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[BookingID] [int] NOT NULL,
	[Status] [bit] NOT NULL,
	[ArticleId] [nvarchar](100) NULL,
	[Tekst] [nvarchar](500) NULL,
	[Sum] [decimal](18, 2) NULL,
	[IsKitchen] [bit] NULL,
	[DiscountPercent] [float] NULL,
	[NetAmount] [decimal](18, 2) NULL,
	[VATPercent] [float] NULL,
	[Amount] [decimal](18, 2) NULL,
	[UnitText] [nvarchar](100) NULL,
	[IsVat] [bit] NULL,
	[IsMainService] [bit] NULL,
	[OrderHeadId] [int] NOT NULL,
	[ServiceName] [nvarchar](500) NULL,
	[Time] [nvarchar](150) NULL,
	[CostPrice] [decimal](18, 2) NOT NULL,
	[CostTotal] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_Orderline] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Orderlines] ADD  DEFAULT ('0.00') FOR [CostPrice]
GO

ALTER TABLE [dbo].[Orderlines] ADD  DEFAULT ('0.00') FOR [CostTotal]
GO

ALTER TABLE [dbo].[Orderlines]  WITH CHECK ADD  CONSTRAINT [FK_Orderline_Orderhead] FOREIGN KEY([OrderHeadId])
REFERENCES [dbo].[Orderhead] ([OrderNo])
GO

ALTER TABLE [dbo].[Orderlines] CHECK CONSTRAINT [FK_Orderline_Orderhead]
GO

