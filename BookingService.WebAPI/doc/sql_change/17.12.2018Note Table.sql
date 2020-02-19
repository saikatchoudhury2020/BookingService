
ALTER TABLE [dbo].[Note] DROP CONSTRAINT [DF_Note_CreateDate]
GO

/****** Object:  Table [dbo].[Note]    Script Date: 17-12-2018 1.38.05 PM ******/
DROP TABLE [dbo].[Note]
GO

/****** Object:  Table [dbo].[Note]    Script Date: 17-12-2018 1.38.05 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Note](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CompanyId] [int] NULL,
	[UserId] [int] NULL,
	[NoteText] [nvarchar](max) NULL,
	[CreateDate] [datetime] NULL,
 CONSTRAINT [PK_Note] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Note] ADD  CONSTRAINT [DF_Note_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO


