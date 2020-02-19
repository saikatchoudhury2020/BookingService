ALTER TABLE [BraathenEiendom].[dbo].[Orderhead]
ADD IsOrderConfirm bit;
ALTER TABLE [BraathenEiendom].[dbo].[Orderhead]
ADD VMOrderFailedErrorMessage NVarchar(Max);