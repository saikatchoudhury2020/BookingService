ALTER TABLE [BraathenEiendom].[dbo].[Orderhead]
ADD IsDeliver bit;
ALTER TABLE [BraathenEiendom].[dbo].[Orderhead]
ADD IsClean bit;
ALTER TABLE [BraathenEiendom].[dbo].[Orderhead]
ADD NoOFPeople int;
ALTER TABLE [BraathenEiendom].[dbo].[Orderhead]
ADD Todate datetime;
ALTER TABLE [BraathenEiendom].[dbo].[Orderhead]
ADD FromDate datetime;