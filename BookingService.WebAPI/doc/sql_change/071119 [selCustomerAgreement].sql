USE [BraathenEiendom]
GO

/****** Object:  StoredProcedure [dbo].[selCustomerAgreement]    Script Date: 07-11-2019 12:01:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[selCustomerAgreement]
	-- Add the parameters for the stored procedure here
	@FromDate varchar(max),
	@ToDate varchar(max),
	@InvoicedToDate varchar(max),
	@OrganizationUnitID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
SET NOCOUNT ON;

SELECT A.Article, T.[Text]
    , SUM(ROUND(2* CONVERT(decimal(18,2),DATEDIFF(Day, A.InvoicedToDate, ISNULL(A.ToDate,@ToDate)))/CONVERT(decimal(2,0),DAY(@ToDate))/2,1) * A.Price) as Price
FROM Agreements A INNER JOIN OrganizationUnit O ON O.OrganizationUnitID=A.Customer
   			      INNER JOIN Person P ON P.PersonID = A.Contact
				  INNER JOIN AgreementTypes T ON T.AgreementTypeId = A.AgreementTypeId
WHERE O.OrganizationUnitID = @OrganizationUnitID 
AND (A.InvoicedToDate < @InvoicedToDate OR A.InvoicedToDate IS NULL)
AND ( A.FromDate < @FromDate OR A.FromDate IS NULL)
AND (A.ToDate > @ToDate OR A.ToDate IS NULL)
GROUP BY A.Article, T.[Text] 

END
GO


