ALTER TABLE [dbo].[Orderhead]
ADD bookingId integer CONSTRAINT fk FOREIGN KEY (bookingId) REFERENCES [dbo].[BookingDetail](BookingID)