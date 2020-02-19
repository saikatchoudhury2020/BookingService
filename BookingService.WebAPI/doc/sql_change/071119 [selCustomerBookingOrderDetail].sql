USE [BraathenEiendom]
GO

/****** Object:  StoredProcedure [dbo].[selCustomerBookingOrderDetail]    Script Date: 07-11-2019 12:05:42 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

alter PROCEDURE [dbo].[selCustomerBookingOrderDetail]
	-- Add the parameters for the stored procedure here
	@BookingIDs varchar(max),
	@Customer int


AS
BEGIN

select L.Tekst +CASE WHEN B.UserID = 0 THEN '' ELSE '-'+(select DisplayName from Person where PersonID=B.UserID) END+', Kr. ' +CONVERT(varchar,L.Sum) as Text 
	   from BookingServiceList L
	  Inner Join BookingDetail B ON B.BookingID=L.BookingID
	  where B.Customer=@Customer AND L.Status = 0 AND B.BookingId IN (
             SELECT CAST(Item AS INTEGER)
            FROM dbo.SplitString(@BookingIDs, ',')
      )

End

GO


