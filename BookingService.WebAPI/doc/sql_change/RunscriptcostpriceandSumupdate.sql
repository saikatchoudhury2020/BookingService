


update BookingServiceList set CostPrice=Price , CostTotal=[Sum]  where ArticleId !=11
update BookingServiceList set CostPrice=34 ,CostTotal=(Qty*34)   where  ArticleID =11 

update DataObjectTable1019 set DataColumn3021=34, DataColumn3022=34  where DataColumn2007=11
update DataObjectTable1019 set DataColumn3021=DataColumn1999, DataColumn3022=DataColumn2000  where DataColumn2007 !=11



select * from BookingServiceList where  ArticleID =11 and IsMainService=0

