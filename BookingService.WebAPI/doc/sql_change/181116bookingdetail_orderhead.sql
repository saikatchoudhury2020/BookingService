ALTER TABLE [BraathenEiendom].[dbo].[BookingDetail]
ADD IsDeliver bit;
ALTER TABLE [BraathenEiendom].[dbo].[BookingDetail]
ADD IsClean bit;

ALTER TABLE [BraathenEiendom].[dbo].[Orderhead]
ADD VismaOrderNo int;
ALTER TABLE [BraathenEiendom].[dbo].[Orderhead]
ADD VismaOrderDate datetime;





