USE [BraathenEiendom]
GO
/****** Object:  StoredProcedure [dbo].[selCustomerBookingOrder]    Script Date: 08.11.2019 11.46.38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[selCustomerBookingOrder]
	-- Add the parameters for the stored procedure here
	@BookingIDs varchar(max),
	@IsSplitting  int,
	@Customer int,
	@SplittPercent decimal(18,2)

AS
BEGIN
if (@IsSplitting=1 )
Begin
SELECT L.ArticleId
	 , SUM(L.Sum) As Amount
	 , CASE WHEN L.ArticleId = '1' THEN '101' ELSE CASE WHEN L.ArticleId='2' THEN  '102' ELSE '' END END As SplitLine1Article
	 , CASE WHEN L.ArticleId = '1' THEN '201' ELSE CASE WHEN L.ArticleId='2' THEN  '202' ELSE '' END END As SplitLine2Article
	 , CASE WHEN L.ArticleId = '1' THEN SUM(L.Sum) * @SplittPercent / 100 ELSE CASE WHEN L.ArticleId = '2' THEN ((SUM(L.Sum)/1.125) *@SplittPercent / 100)*1.25 ELSE 0 END END As SplitLine1Price
	 , CASE WHEN L.ArticleId = '1' THEN SUM(L.Sum) * (100 - @SplittPercent) / 100 ELSE CASE WHEN L.ArticleId = '2' THEN (SUM(L.Sum)/1.125) * (100 - @SplittPercent) / 100 ELSE 0 END END As SplitLine2Price
FROM BookingServiceList L
INNER JOIN BookingDetail B ON B.BookingID = L.BookingID
WHERE B.Customer = @Customer AND (B.Status = 0 or B.Status = 98 ) AND L.Status=0 AND  ISNULL(L.ArticleId,0) <> 0 
AND B.BookingId IN (
            SELECT CAST(Item AS INTEGER)
            FROM dbo.SplitString(@BookingIDs, ',')
      )
GROUP BY L.ArticleId
End
Else
Begin
SELECT L.ArticleId
	 , SUM(L.Sum) As Amount
FROM BookingServiceList L
INNER JOIN BookingDetail B ON B.BookingID = L.BookingID
WHERE B.Customer = @Customer AND B.Status = 0 AND L.Status=0 AND ISNULL(L.ArticleId,0) <> 0 
AND B.BookingId IN (
            SELECT CAST(Item AS INTEGER)
            FROM dbo.SplitString(@BookingIDs, ',')
      )
GROUP BY L.ArticleId
End

END
