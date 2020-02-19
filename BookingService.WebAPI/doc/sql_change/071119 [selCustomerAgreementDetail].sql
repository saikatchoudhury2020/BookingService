USE [BraathenEiendom]
GO

/****** Object:  StoredProcedure [dbo].[selCustomerAgreementDetail]    Script Date: 08-11-2019 14:20:26 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[selCustomerAgreementDetail]
	-- Add the parameters for the stored procedure here
	@FromDate varchar(Max),
	@ToDate varchar(Max),
	@InvoicedToDate varchar(Max),
	@Text varchar(Max),
	@OrganizationUnitID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
SET NOCOUNT ON;

SELECT A.AgreementId, P.DisplayName + ' ' + T.Name + ' ' + @Text + ' ' + CONVERT(varchar,CONVERT(decimal(18,2),(ROUND(2* CONVERT(decimal(18,2),DATEDIFF(Day, A.InvoicedToDate, ISNULL(A.ToDate,@ToDate)))/CONVERT(decimal(2,0),DAY(@ToDate))/2,1) * A.Price))) as Text
FROM Agreements A INNER JOIN OrganizationUnit O ON O.OrganizationUnitID=A.Customer
   			      INNER JOIN Person P ON P.PersonID = A.Contact
				  INNER JOIN AgreementTypes T ON T.AgreementTypeId = A.AgreementTypeId
WHERE O.OrganizationUnitID = @OrganizationUnitID 
AND (A.InvoicedToDate < @InvoicedToDate OR A.InvoicedToDate IS NULL)
AND ( A.FromDate < @FromDate OR A.FromDate IS NULL)
AND (A.ToDate > @ToDate OR A.ToDate IS NULL)

END


/****** Object:  StoredProcedure [dbo].[selCustomerAgreement]    Script Date: 05-11-2019 11.54.59 AM ******/
SET ANSI_NULLS ON


SET ANSI_NULLS ON
GO


