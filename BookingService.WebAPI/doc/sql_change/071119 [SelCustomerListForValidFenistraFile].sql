USE [BraathenEiendom]
GO

/****** Object:  StoredProcedure [dbo].[SelCustomerListForValidFenistraFile]    Script Date: 07-11-2019 12:03:15 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SelCustomerListForValidFenistraFile]
	-- Add the parameters for the stored procedure here
	@BookingIDs varchar(max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;


SELECT O.OrganizationUnitID, O.Custom1, O.OrgNumber 
FROM BookingDetail B INNER Join OrganizationUnit O ON O.OrganizationUnitID=B.Customer
		     INNER JOIN BookingServiceList L ON L.BookingID=B.BookingID AND L.OrderHeadId = 0
WHERE  B.BookingID IN (
            SELECT CAST(Item AS INTEGER)
            FROM dbo.SplitString(@BookingIDs, ',')
      )
GROUP BY O.OrganizationUnitID, O.Custom1, O.OrgNumber
UNION SELECT O.OrganizationUnitID, O.Custom1, O.OrgNumber
FROM Agreements A INNER JOIN OrganizationUnit O ON O.OrganizationUnitID=A.Customer
		  INNER JOIN Person P ON P.PersonID = A.Contact
GROUP BY O.OrganizationUnitID, O.Custom1, O.OrgNumber

END



/****** Object:  StoredProcedure [dbo].[selCustomerBookingOrder]    Script Date: 05-11-2019 11.41.26 AM ******/
SET ANSI_NULLS ON
GO


