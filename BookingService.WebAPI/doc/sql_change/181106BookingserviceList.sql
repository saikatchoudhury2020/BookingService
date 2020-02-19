  


  ALTER TABLE [BraathenEiendom].[dbo].[BookingServiceList] ALTER COLUMN ArticleId nvarchar(100) NULL
 
  ALTER TABLE [BraathenEiendom].[dbo].[BookingServiceList] ADD [IsMainService] bit;
-- ALTER TABLE [BraathenEiendom].[dbo].[BookingServiceList]
--ADD IsVat bit;
  ALTER TABLE [BraathenEiendom].[dbo].[BookingServiceList] DROP COLUMN [VatType];
  

  ALTER TABLE [BraathenEiendom].[dbo].[BookingServiceList]
ADD OrderHeadId integer 
