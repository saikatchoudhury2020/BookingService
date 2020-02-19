USE [BraathenEiendom]
GO
/****** Object:  StoredProcedure [dbo].[selCustomerUpdateBookingServiceList]    Script Date: 11-11-2019 12.23.24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

alter PROCEDURE [dbo].[selCustomerUpdateBookingServiceList]
	-- Add the parameters for the stored procedure here
	@BookingIDs varchar(max),
	@Customer int,
	@orderHeadID int

AS
BEGIN

Update BookingServiceList set OrderHeadId=@orderHeadID
where BookingID in (
select b.BookingID from BookingDetail b 
 inner join BookingServiceList l on b.BookingID=l.BookingID
 where l.Status=0 and (b.Status=0 or B.[Status] = 98 ) and b.BookingID in(
            SELECT CAST(Item AS INTEGER)
            FROM dbo.SplitString(@BookingIDs, ',')
      ) and b.Customer=@customer ) and status=0

END
