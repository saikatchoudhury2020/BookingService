/******************************************************************************
**	Auth: Bjørn
**	Date: 16.02.04
*******************************************************************************
**	Change History
**
**	Date:			Author:		Description:
**	--------	--------	-------------------------------------------------------
**	25.02.04	Bjørn			Added AutoFieldName	
**	04.08.04	Bjørn			Added trimming of FieldName for leading/trailing spaces
**	27.12.04	Arun			Added support for Nvarchar datatype for the field
**	28.12.04	Bjørn			Unicode on parameters
*******************************************************************************
**	NOTE: View this code with a tab size of 2 and monospace font
*******************************************************************************/

IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'ins_DataField')
	DROP  Procedure  dbo.ins_DataField
GO

CREATE Procedure dbo.ins_DataField
	@Type										INT,
	@PersonID								INT,
	@ObjectID								INT OUTPUT,
	@Modified								DATETIME OUTPUT,
	@FieldName							NVARCHAR(50),
	@AutoFieldName					VARCHAR(50),
	@DataObjectID						INT,
	@PageID									INT,
	@DataTypeID							INT,
	@MaxLength							INT,
	@Scale									TINYINT,
	@Precision							TINYINT,
	@DefaultValue						NVARCHAR(50),
	@Required								BIT,
	@SystemField						TINYINT,
	@Priority								INT,
  @ControlID							INT,
	@ErrorText							NVARCHAR(200),
	@HelpText								NVARCHAR(200),
	@Event									NVARCHAR(100),
	@Status									TINYINT OUTPUT,
	@ValidationRule					NVARCHAR(50),
	@Pictureid							INT,
	@ColumnID								INT OUTPUT,
	@ParentDisplayFieldID		INT,
	@ListofvalueID					INT,
	@ParentValueFieldID			INT,
	@ColumnName							VARCHAR(50) OUTPUT
WITH ENCRYPTION
AS
	SET NOCOUNT ON
	BEGIN TRAN

	-- Code for checking input parameter 
	IF @Type IS NULL
	BEGIN
		RAISERROR ('Procedure expects parameter which was not supplied.', 16, 1)
		GOTO OnError
	END

	-- TYPE 0: (Description)
	IF @Type = 0
	BEGIN
		DECLARE 
			@tableid        INT,
			@tablename      VARCHAR(50),
			@filehtmltypeid INT

		IF @DataObjectID is NULL
		BEGIN
			RAISERROR('DataObjectID cannot be NULL',16,1)
			GOTO OnError
		END

		IF EXISTS(SELECT ObjectID FROM dbo.DataObject WHERE ObjectID = @DataObjectID AND ReadOnly = 1)
		BEGIN
			RAISERROR('DataObject is readonly',16,1)
			GOTO OnError
		END

		SELECT @filehtmltypeid = ObjectID
		FROM dbo.DataFieldControl WHERE ControlName = 'file'

		IF @filehtmltypeid is NULL
		BEGIN
			RAISERROR('There is no HTML tag of  file type exists in database.Critical fail.',16,1)
			GOTO OnError
		END

		IF @filehtmltypeid = @ControlID
		BEGIN
			DECLARE @counter INT
			SET @MaxLength =255

			SELECT @counter=COUNT(*) FROM dbo.vie_datafield WHERE DataObjectID=@DataObjectID AND HTMLTagid=@filehtmltypeid
				SET @counter = @counter+1
				SET @FieldName='FileName_'+cast(@counter as VARCHAR(10))
				
				SELECT @DataTypeID=objectid FROM dbo.datatype WHERE Fieldtype='File'
				
				WHILE 1> 0 --repeate untill we get corret unique id
				BEGIN
					IF EXISTS(SELECT * FROM dbo.datafield WHERE FieldName=@FieldName and DataObjectID=@DataObjectID )
					BEGIN
						SET @counter = @counter+1
						SET @FieldName = 'FileName_'+CAST(@counter AS VARCHAR(10))
					END
					ELSE 
						BREAK
				END
			END
		ELSE IF @FieldName LIKE 'FileName[_]%'
		BEGIN
			RAISERROR('FileName_ is a reserverd word , you can give a field name starting with FileName_. Please change the fieldname. ',16,1)
			GOTO OnError  
		END
		
		IF @FieldName IS NULL
		BEGIN
			RAISERROR('FieldName can not be NULL.Datafield must have a name.',16,1)
			GOTO OnError
		END
		
		SET @FieldName = LTRIM(RTRIM(@FieldName))

		IF EXISTS(SELECT * FROM dbo.DataField WHERE fieldname=@fieldname and DataObjectID=@DataObjectID)
		BEGIN
			RAISERROR('Another field with the same name already exists in database',16,1)
			GOTO OnError
		END
			
		IF @Precision IS NOT NULL
		BEGIN
			IF NOT EXISTS(SELECT * FROM dbo.datatype WHERE typename='decimal' and ObjectID=@datatypeid)
			BEGIN
				SET @Precision=NULL
				SET @Scale =NULL
			END
		END

		IF @Required IS NULL
			SET @Required=0

		IF @Priority IS NULL
		BEGIN
			SELECT @Priority = ISNULL(MAX(Priority) + 1, 1)
			FROM dbo.DataField
			WHERE DataObjectID = @DataObjectID
		END

		SET @Status = 0
		SET @ColumnID = NULL

		INSERT INTO dbo.DataField (
			FieldName, AutoFieldName, DataObjectID, PageID, DataTypeID, MaxLength, Scale, [Precision], 
			DefaultValue, Required, SystemField, Priority, ControlID, ErrorText, HelpText, Event, 
			Status, ValidationRule, Pictureid, ColumnID, ParentDisplayFieldID, ListofvalueID, 
			ParentValueFieldID
		) VALUES (
			@FieldName, @AutoFieldName, @DataObjectID, @PageID, @DataTypeID, @MaxLength, @Scale, @Precision, 
			@DefaultValue, @Required, @SystemField, @Priority, @ControlID, @ErrorText, @HelpText, @Event, 
			@Status, @ValidationRule, @Pictureid, @ColumnID, @ParentDisplayFieldID, @ListofvalueID, 
			@ParentValueFieldID
		)

		IF @@Error <> 0
		BEGIN
			RAISERROR ('Unable to update table.', 16, 1)
			GOTO OnError
		END

		SET @ObjectID = SCOPE_IDENTITY()

		SELECT
			@tableid   = tableid,
			@tablename = tablename
		FROM dbo.vie_digisystable
		WHERE dataObjectID = @DataObjectID

		--  modify table if the table is already created and new entered field has a datatypeid
		IF @tableid IS NOT NULL AND @DataTypeID IS NOT NULL
		BEGIN
			DECLARE 
				@sqlstring  VARCHAR(2000),
				@datacolumn VARCHAR(50),
				@datatype   VARCHAR(20),
				@updatesql  nVARCHAR(2000)

			SET @sqlstring ='ALTER TABLE '+@tablename +' ADD '
			          
			SET @datacolumn = 'DataColumn' + CAST(@ObjectID AS VARCHAR)
			SET @ColumnName = @datacolumn

			SELECT @datatype = TypeName FROM dbo.datatype WHERE ObjectID = @DataTypeID

			IF @datatype IS NULL
			BEGIN
				RAISERROR('The given DataTypeID is not vaild',16,1)
				GOTO OnError
			END    
			    
			IF @datatype = 'VARCHAR' OR @datatype = 'NVARCHAR'
			BEGIN
				IF @MaxLength IS NULL
				BEGIN  
					SET @MaxLength=50

					UPDATE dbo.datafield
					SET MaxLength = @MaxLength
					WHERE ObjectID = @ObjectID

					IF @@Error <> 0 GOTO OnError
				END

				SET @datatype = @datatype + '('+cast(@MaxLength as VARCHAR(4))+')'
			END

			IF @datatype='decimal'
			BEGIN
				IF @Precision IS NULL
					SET @Precision=18

				IF @scale IS NULL
					SET @scale=0
			  
				IF @scale > @Precision
					SET @scale=@Precision

				SET @MaxLength = @Precision+1

				UPDATE dbo.datafield
				SET MaxLength  = @MaxLength
				WHERE ObjectID = @ObjectID

				IF @@Error <> 0 GOTO OnError
			  
				SET @datatype = @datatype + '('+ cast(@Precision as VARCHAR(2)) + ',' + cast(@scale as VARCHAR(2)) +')'
			END  
			    
			SET @sqlstring = @sqlstring +' '+ @datacolumn +' '+ @datatype 

			IF @DefaultValue IS NOT NULL
				SET @sqlstring = @sqlstring + ' DEFAULT('''+@DefaultValue+''')'

			-- insert data into digisyscolumn
			
			INSERT INTO dbo.digisyscolumn( tableid, columnname )
			VALUES ( @tableid, @datacolumn )

			IF @@Error <> 0 GOTO OnError

			SELECT @columnid = SCOPE_IDENTITY()

			UPDATE dbo.datafield
			SET columnid = @columnid
			WHERE ObjectID = @ObjectID

			IF @@Error <> 0 GOTO OnError
			--PRINT @sqlstring
			EXEC(@sqlstring)

			IF @@Error <> 0 GOTO OnError

			SET @updatesql = N'UPDATE '+@tablename +N' SET '+@datacolumn +N' = ' + ''''+@DefaultValue +''''
			EXEC sp_executesql @updatesql

			IF @@Error <> 0 GOTO OnError

			DECLARE @ReturnValue INT
			EXEC upd_datafield_backend @DataObjectID,@ObjectID,@ReturnValue

			IF @@Error <> 0 GOTO OnError

			IF @ReturnValue <> 0
			BEGIN
				RAISERROR('Internal call to upd_datafield_backend procedure failed.',16,1)
				GOTO OnError
			END
		END
	END

OnSuccess:
	COMMIT TRAN
	GOTO OnEnd
OnError:
	ROLLBACK TRAN
OnEnd:
	RETURN
GO


GRANT EXECUTE ON [dbo].[ins_DataField] TO [DigiAdminRole]--, [DigiWebRole]
GO

/**** EXTENDED PROPERTIES *****************************************************/

-- Purpose of the procedure
EXEC sp_addextendedproperty 
	N'Purpose', N'Purpose of Stored Procedure', 
	N'USER', N'dbo', 
	N'PROCEDURE', N'ins_DataField', 
	NULL, NULL
GO
-- Resturned result sets of the stored procedure
EXEC sp_addextendedproperty 
	N'Result', N'Recordset information.', 
	N'USER', N'dbo', 
	N'PROCEDURE', N'ins_DataField', 
	NULL,NULL
GO
-- Parameter description
EXEC sp_addextendedproperty 
	N'Description', N'Specifies the type of information to retrieve. Refer to the Result section for details.', 
	N'USER', N'dbo', 
	N'PROCEDURE', N'ins_DataField', 
	N'PARAMETER', N'@Type'
GO
