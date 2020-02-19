ALTER TABLE [BraathenEiendom].[dbo].[Orderlines]
ADD IsVat bit;
ALTER TABLE [BraathenEiendom].[dbo].[Orderlines]
ADD VatType int;
ALTER TABLE [BraathenEiendom].[dbo].[FoodService]
ADD IsVat bit;
ALTER TABLE [BraathenEiendom].[dbo].[FoodService]
ADD VatType int;


ALTER TABLE [BraathenEiendom].[dbo].[FoodService]
ADD DiscountPercent float;
ALTER TABLE [BraathenEiendom].[dbo].[FoodService]
ADD NetAmount float;
ALTER TABLE [BraathenEiendom].[dbo].[FoodService]
ADD VATPercent float;
ALTER TABLE [BraathenEiendom].[dbo].[FoodService]
ADD Amount float;
ALTER TABLE [BraathenEiendom].[dbo].[FoodService]
ADD UnitText nvarchar(100);
ALTER TABLE [BraathenEiendom].[dbo].[FoodService] 
ALTER COLUMN [Sum] float 

