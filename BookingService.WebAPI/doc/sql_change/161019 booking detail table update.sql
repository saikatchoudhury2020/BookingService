  ALTER TABLE [BraathenEiendom].[dbo].[BookingDetail]
  ADD IsMVA bit NOT NULL DEFAULT(0);

 ALTER TABLE [BraathenEiendom].[dbo].[BookingDetail]
 ADD InvoMessage nvarchar(500);