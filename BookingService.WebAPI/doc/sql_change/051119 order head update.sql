



 ALTER TABLE Orderhead
DROP CONSTRAINT fk;


exec sp_updatestats
dbcc freeproccache