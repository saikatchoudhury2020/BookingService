ALTER TABLE [BraathenEiendom].[dbo].[BookingServiceList] ALTER COLUMN [Sum] decimal(18,2)
ALTER TABLE [BraathenEiendom].[dbo].[BookingServiceList] ALTER COLUMN NetAmount decimal(18,2)
ALTER TABLE [BraathenEiendom].[dbo].[BookingServiceList] ALTER COLUMN Amount decimal(18,2)
 ALTER TABLE [BraathenEiendom].[dbo].[BookingServiceList] ADD ServiceName nvarchar(500) NULL
  ALTER TABLE [BraathenEiendom].[dbo].[Orderhead] ADD ISVismaOrder BIT default 'FALSE';