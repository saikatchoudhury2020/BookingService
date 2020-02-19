
CREATE TABLE [dbo].[BookingMessage](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Type] [nvarchar](50) NULL,
	[BookingID] [int] NULL,
	[To] [nvarchar](255) NULL,
	[Body] [nvarchar](max) NULL,
	[Attachments] [nvarchar](255) NULL,
	[SendTime] [date] NULL,
	[Status] [int] NULL,
	[Subject] [nvarchar](255) NULL,
 CONSTRAINT [PK_BookingMessage] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[BookingMessage] ADD  CONSTRAINT [DF_BookingMessage_Status]  DEFAULT ((0)) FOR [Status]
GO

