/******************************************************************************
**	Auth: Bjørn
**	Date: 17.02.04
*******************************************************************************
**	Change History
**
**	Date:			Author:		Description:
**	--------	--------	-------------------------------------------------------
**	25.02.04	Bjørn			Added AutoFieldName	
**	04.08.04	Bjørn			Added trimming of FieldName for leading/trailing spaces
**	17.08.04	Bjørn			Added error code 1 on MaxLength error
**	27.12.04	Arun			Added support for ntext datatype for the field
**	28.12.04	Bjørn			Unicode on parameters
**	13.01.05	Arun			Fixed Updating Procedures when u cahnage types
**	27.05.05	Arun Deep	Fixed to change the length of parameters for insert and update procdures on changing column lenght.
**	22.12.05	Sunil			Fixed, to drop and recreate column when data type of column changes in cases where implicit conversion is not allowed.
*******************************************************************************
**	NOTE: View this code with a tab size of 2 and monospace font
*******************************************************************************/

IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'upd_DataField')
	DROP  Procedure  dbo.upd_DataField
GO

CREATE Procedure dbo.upd_DataField
	@Type							 INT,
  @ObjectID          INT,
	@Modified					 DATETIME OUTPUT,
  @FieldName         NVARCHAR(50),
  @AutoFieldName		 VARCHAR(50),
	@DataObjectID			 INT OUTPUT,
	@PageID						 INT,
  @DataTypeID        INT,
  @MaxLength         INT,
  @scale             TINYINT,
  @Precision         TINYINT,
  @DefaultValue      NVARCHAR(50),
  @Required          BIT,        
	@SystemField			 TINYINT OUTPUT,
  @Priority          INT,
  @ControlID				 INT,
  @ErrorText         NVARCHAR(200),
  @HelpText          NVARCHAR(200),
  @Event             NVARCHAR(100),
  @Status						 TINYINT OUTPUT,
  @validationRule    NVARCHAR(50),
  @Pictureid         INT,
  @ColumnID					 INT OUTPUT,
  @ParentDisplayFieldID	INT OUTPUT,
  @ListOfValueID     INT,
  @ParentValueFieldID INT OUTPUT
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
		DECLARE @filehtmltypeid   INT,            @counter       INT,            @i              INT,
						@returnvalue      INT,            @dataviewid    INT,            
						@datafieldid      INT,            @sqlstring     VARCHAR(2000),  @datacolumn     VARCHAR(50),
						@updatesql        NVARCHAR(2000), @altertable    INT,            @oldMaxLength   INT,
						@tableid          INT,            @tablename     VARCHAR(50),    @olddatatypeid  INT,
						@olddatatype      VARCHAR(20),    @datatype      VARCHAR(20),    
						@oldListOfValueid INT,            @returnmsg     NVARCHAR(510),  @oldRequired    TINYINT,
						@updatetable      BIT,            @oldfieldname  VARCHAR(50)            
	          
		DECLARE @tempview_datafieldid table ( vid INT )  

		IF @ObjectID is NULL
		BEGIN
			RAISERROR('ObjectID can not be NULL.', 16, 1)
			GOTO OnError
		END

		SET @datafieldid = @ObjectID

		SELECT @DataObjectID = DataObjectID
		FROM   dbo.DataField
		WHERE  ObjectID = @datafieldid

		IF @DataObjectID is NULL
		BEGIN
			RAISERROR('DataObjectID can not be NULL.Datafield should belong to a dataobject.', 16, 1)
			GOTO OnError
		END

		IF EXISTS( SELECT ObjectID FROM dbo.DataObject  WHERE ObjectID = @DataObjectID AND ReadOnly = 1 )
		BEGIN
			RAISERROR('DataObject is readonly', 16, 1)
			GOTO OnError
		END

		IF @FieldName IS NULL
		BEGIN
			RAISERROR('FieldName can not be NULL.Datafield must have a name.', 16, 1)
			GOTO OnError
		END
		
		SET @FieldName = LTRIM(RTRIM(@FieldName))
	  

		IF NOT EXISTS(SELECT * FROM dbo.datatype WHERE typename='decimal' and ObjectID=@datatypeid)
			BEGIN
				SET @Precision=NULL
				SET @Scale =NULL
			END


		IF  @ControlID in (SELECT ObjectID FROM dbo.DataFieldControl WHERE  enableDataStorage = 0 )
		BEGIN
			SET @DataTypeID= NULL
			SET @Precision=NULL
			SET @Scale =NULL
		END

		IF @Required is NULL
			SET @Required=0
		IF @Precision is not NULL
		BEGIN
			IF not exists(SELECT * FROM dbo.datatype WHERE typename='decimal' and ObjectID=@datatypeid)
			BEGIN
				SET @Precision=NULL
				SET @Scale =NULL
				SET @MaxLength=@Precision+1
			END
		END

		IF exists(SELECT fieldname FROM dbo.datafield 
					WHERE dataObjectID=@dataObjectID and ObjectID <> @datafieldid and fieldname=@fieldname)
		BEGIN
			RAISERROR('Fieldname is already in use , please SELECT another name.',16,1)
			GOTO OnError
		END

	  
		SELECT @tableid=tableid ,@tablename=tablename FROM dbo.vie_digisystable WHERE DataObjectid=@DataObjectID
	  
		--PRINT @tableid
		--select the old values
		SELECT @olddatatypeid=datatypeid,  @oldMaxLength=MaxLength,  @oldrequired=required,
					@oldfieldname = fieldname
		FROM dbo.datafield WHERE ObjectID=@datafieldid

	--UPDATE the datafield
		IF @Priority is NULL
			SELECT @Priority=Priority FROM dbo.DataField WHERE ObjectID=@ObjectID

		SELECT @oldListOfValueid=ListOfValueID FROM dbo.datafield
		WHERE ObjectID=@ObjectID 

		IF @ListOfValueID is Not Null and @oldListOfValueid is Null
		BEGIN
				EXEC dbo.upd_DataObjectRelation @ChildFieldID=@ObjectID, @ParentDisplayFieldID=NULL, @ParentValueFieldID=NULL
				IF @@Error<> 0 GOTO OnError

				DELETE FROM dbo.fieldoption WHERE DataFieldid=@objectid
	      
				IF @@Error<> 0 GOTO OnError
		END
	      
		SET @Modified = GETDATE()

		UPDATE dbo.DataField SET
			Modified				 = @Modified,
			FieldName        = @FieldName,
			AutoFieldName		 = @AutoFieldName,
			PageID					 = @PageID,
			DataTypeID       = @DataTypeID,
			MaxLength        = @MaxLength,
			Scale            = @scale,
			[Precision]      = @Precision,
			DefaultValue     = @DefaultValue,
			Required         = @Required,
			Priority         = @Priority,
			ControlID        = @ControlID,
			ErrorText        = @ErrorText,
			HelpText         = @HelpText,
			Event            = @Event,
			ValidationRule   = @validationRule,
			Pictureid        = @Pictureid,
			ListOfValueID    = @ListOfValueID
		WHERE ObjectID=@ObjectID 

		IF @@Error<> 0 GOTO OnError
		
		SELECT 
			@DataObjectID					= DataObjectID,
			@SystemField					= SystemField,
			@Status								= Status,
			@ColumnID							= ColumnID,
			@ParentDisplayFieldID = ParentDisplayFieldID,
			@ParentValueFieldID   = ParentValueFieldID
		FROM dbo.DataField
		WHERE ObjectID = @ObjectID

		IF @tableid is not NULL  -- start to check if table exists and modify cahnges
		BEGIN
			SET @updatetable =0 --check if table is updated or not
			SET @datacolumn='DataColumn'+cast(@datafieldID as VARCHAR(10))  
			DECLARE @tempconstraINT table (tempid INT identity(1,1),cname nVARCHAR(128))

			IF @olddatatypeid is NULL and @DataTypeID is Not NULL  --changing column from type that can not store data to type that can
			BEGIN
				--PRINT 'here'
				SET @updatetable =1 
				SET  @sqlstring ='ALTER TABLE '+@tablename +' ADD '
	            
	      
				SELECT @datatype=TypeName FROM dbo.datatype  WHERE ObjectID=@DataTypeID
				IF @datatype is NULL --datatype can not be NULL give error message
				BEGIN
					RAISERROR('Datatype can not be NULL.',16,1)
					GOTO OnError
				END

				SELECT  @datatype=dbo.GetDataType(@datatype,@MaxLength,@precision,@scale)
	     
				SET @sqlstring=@sqlstring +' '  +@datacolumn + ' '+@datatype 
				IF @DefaultValue is Not  NULL
						SET @sqlstring=@sqlstring + ' DEFAULT('''+@DefaultValue+''')'
				--INSERT data INTo digisyscolumn
				INSERT INTO dbo.digisyscolumn(tableid,columnname) VALUES (@tableid,@datacolumn)

				SET @columnid = SCOPE_IDENTITY()

				IF @@Error<> 0 GOTO OnError

				UPDATE dbo.datafield SET columnid=@columnid WHERE ObjectID=@datafieldID

				IF @@Error<> 0 GOTO OnError

				--PRINT (@sqlstring)
				EXEC(@sqlstring)

				IF @@Error<> 0 GOTO OnError

				SET @updatesql=N'UPDATE '+@tablename +N' SET '+@datacolumn +N' = ' + ''''+@DefaultValue +''''
				--PRINT @updatesql
				EXEC sp_executesql @updatesql

				IF @@Error<> 0 GOTO OnError

				EXEC upd_datafield_backend @DataObjectID,@datafieldID,@returnvalue OUTPUT

				IF @returnvalue <> 0
				BEGIN
					RAISERROR('Internal procedure upd_datafield_backend failed.',16,1)
					GOTO OnError
				END
			END   -- end of changing column from type that can not store data to type that can
			--**********************************************
			--UPDATE table to delete the coloumn,non editable procedures,all views where fieldis used
			ELSE IF @datatypeid is NULL and @olddatatypeid is Not NULL 
			BEGIN
				--PRINT 'here'
				SET @updatetable =1 
	 
				DECLARE @min INT,@max INT,@constname nVARCHAR(128)
	      
				INSERT INTO @tempview_datafieldid (vid)
				SELECT dataviewid
				FROM   dbo.vie_dataview
				WHERE  editable   = 1
					AND datafieldid = @datafieldid
	        
				SELECT @datacolumn = ColumnName
				FROM   dbo.vie_digisyscolumn
				WHERE  DataFieldID = @datafieldid

				INSERT INTO @tempconstraint(cname)
				SELECT OBJECT_NAME(constid)
				FROM   sysconstraints 
				WHERE  OBJECT_NAME(id) = @tablename
					AND colid IN ( SELECT colid
												FROM syscolumns
												WHERE OBJECT_NAME(id) = @tablename
													AND name = @datacolumn )
	      
				SELECT @MIN = min(tempid),
							@MAX = max(tempid)
				FROM @tempconstraint
	      
				IF @min is Not NULL
				BEGIN
					WHILE @min <= @max
					BEGIN
						SELECT @constname =cname FROM @tempconstraINT WHERE tempid=@min
						SET @sqlstring=' ALTER  TABLE '+@tablename +' DROP CONSTRAINT '  +@constname
						exec(@sqlstring)
						SET @min=@min+1
					END
				END  

				SET  @sqlstring ='ALTER TABLE '+@tablename +' DROP COLUMN '  +@datacolumn
				exec(@sqlstring)

				IF @@Error<> 0 GOTO OnError

				UPDATE dbo.datafield SET columnid=Null WHERE ObjectID=@datafieldid

				IF @@Error<> 0 GOTO OnError

				DELETE FROM dbo.digisyscolumn WHERE ObjectID in (SELECT columnid FROM dbo.datafield WHERE ObjectID=@datafieldid)

				IF @@Error<> 0 GOTO OnError

				DELETE FROM dbo.viewbuilder WHERE datafieldid=@datafieldid

				IF @@Error<> 0 GOTO OnError

				--execute procedure to UPDATE all the noneditable views
				EXEC upd_datafield_backend @dataObjectID,NULL,@returnvalue OUTPUT 

				IF @returnvalue <> 0
				BEGIN
					RAISERROR('Internal call to the procedure upd_datafield_backend failed.',16,1)
					GOTO OnError
				END

				SELECT @counter=count(*) FROM @tempview_datafieldid 

				SET @i=0
				WHILE @i < @counter
				BEGIN
					SELECT TOP 1 @dataviewid=vid FROM @tempview_datafieldid 

					EXEC dbo.ins_DataView_Backend @DataViewID=@DataViewID

					IF @@Error <> 0
						GOTO OnError

					DELETE FROM @tempview_datafieldid  WHERE vid=@dataviewid
					SET @counter=@counter-1
				END        
			END      -- end of UPDATE table to delete the coloumn,non editable procedures,all views where fieldis used
			--*****************************************************
			-- when datatye is changed
			ELSE IF @olddatatypeid <> @DataTypeID
			BEGIN
				DECLARE @DropAndReCreateColumn BIT
				DECLARE @DataRestorePossible BIT
				DECLARE @DataRestoreSQL VARCHAR(200)
				DECLARE @NewColName VARCHAR(200)
				DECLARE @DataObjectHasData BIT 
				DECLARE @HasDataSQL NVARCHAR(400)
				DECLARE @OldFormField VARCHAR(200)
				DECLARE @FinalFormField VARCHAR(200)
				
				SET @DataObjectHasData = 0
				SET @NewColName = @datacolumn + 'Digimaker'
				SET @DropAndReCreateColumn = 0
				SET @DataRestorePossible = 0
				SET @DataRestoreSQL = ''
				
				Set @DataObjectHasData  = 0
				SET @HasDataSQL = N'IF EXISTS (Select * from dbo.' + @tablename + ') SET @DataObjectHasData = 1'
				exec sp_executesql @HasDataSQL , N'@DataObjectHasData BIT output', @DataObjectHasData out

				SET @updatetable =1 
				SELECT @olddatatype=typename, @OldFormField = FieldType FROM dbo.datatype WHERE ObjectID=@olddatatypeid
				SELECT @datatype=typename, @FinalFormField = FieldType FROM dbo.datatype WHERE ObjectID=@datatypeid
				
				/*				
				bit
				datetime
				decimal
				image*
				int
				ntext*
				nvarchar
				text*
				varchar
				*/
/*				
							WHEN 'bit'
							WHEN 'datetime'
							WHEN 'decimal'
							WHEN 'image'
							WHEN 'int'
							WHEN 'ntext'
							WHEN 'nvarchar'
							WHEN 'text'
							WHEN 'varchar'
*/											
				IF @olddatatype = 'bit'
						BEGIN
							IF  @datatype = 'bit' OR 
								@datatype = 'int' OR 
								@datatype = 'ntext' OR 
								@datatype = 'nvarchar' OR 
								@datatype = 'text' OR 
								@datatype = 'varchar'
								BEGIN
									SET @DropAndReCreateColumn = 0
								END

							ELSE IF @datatype = 'datetime' OR @datatype = 'decimal' OR @datatype = 'image'
								BEGIN
									SET @DropAndReCreateColumn = (1 & @DataObjectHasData)
									SET @DataRestorePossible = 0
								END
						END
						
				ELSE IF @olddatatype = 'datetime'
						BEGIN
							IF @datatype = 'bit' OR @datatype = 'decimal' OR @datatype = 'image' OR @datatype = 'int'
								BEGIN
									SET @DropAndReCreateColumn = (1 & @DataObjectHasData)
									SET @DataRestorePossible = 0
								END
							
							ELSE IF @datatype = 'datetime' OR @datatype = 'nvarchar' OR @datatype = 'varchar'
								BEGIN
									--If changed to Email then data should be removed.
									IF (@datatype = 'nvarchar' OR @datatype = 'varchar' ) AND 
									   (@FinalFormField = 'File' OR @FinalFormField = 'Epost' OR @FinalFormField = 'Url')
									   BEGIN
											SET @DropAndReCreateColumn = 1
											SET @DataRestorePossible = 0
									   END
									ELSE
										SET @DropAndReCreateColumn = 0
								END
							ELSE IF @datatype = 'text' OR @datatype = 'ntext' /* Operand type clash: datetime is incompatible with text */
								BEGIN
									SET @DropAndReCreateColumn = 1
									SET @DataRestorePossible = 1
									SET @DataRestoreSQL = 'CAST(' + @NewColName + ' AS VARCHAR)'
								END
						END
						
				ELSE IF @olddatatype = 'decimal' 
						BEGIN
							IF @datatype = 'bit' OR @datatype = 'int'
								BEGIN
									SET @DropAndReCreateColumn = 0
								END
							ELSE IF @datatype = 'datetime'
									BEGIN
										SET @DropAndReCreateColumn = (1 & @DataObjectHasData)
										SET @DataRestorePossible = 0
									END
							ELSE IF @datatype = 'image'
									BEGIN
										SET @DropAndReCreateColumn = 1
										SET @DataRestorePossible = 0
									END
							ELSE IF @datatype = 'varchar' OR @datatype = 'nvarchar'
									BEGIN
										SET @DropAndReCreateColumn = 1
										
										IF @FinalFormField = 'File' OR @FinalFormField = 'Epost' OR @FinalFormField = 'Url'
											BEGIN
												SET @DataRestorePossible = 0
											END
										ELSE
											BEGIN
												SET @DataRestorePossible = 1
												SET @DataRestoreSQL = 'SUBSTRING(CAST(' + @NewColName + ' AS VARCHAR), 1 ,' + CAST(@MaxLength AS VARCHAR) + ')'
											END
									END
							ELSE IF @datatype = 'ntext' OR @datatype = 'text'
									BEGIN
										SET @DropAndReCreateColumn = 1
										SET @DataRestorePossible = 1
										SET @DataRestoreSQL = 'CAST(' + @NewColName + ' AS VARCHAR)'
									END
						END
						
				ELSE IF @olddatatype = 'image'
						BEGIN
							SET @DropAndReCreateColumn = 1
							SET @DataRestorePossible = 0
						END
						
				ELSE IF @olddatatype = 'int'
						BEGIN
							IF @datatype = 'bit' OR @datatype = 'decimal' OR @datatype = 'int'
									SET @DropAndReCreateColumn = 0
							ELSE IF @datatype = 'datetime' OR @datatype = 'image'
									BEGIN
										SET @DropAndReCreateColumn = 1
										SET @DataRestorePossible = 0
									END
							ELSE IF @datatype = 'varchar' OR @datatype = 'nvarchar'
									BEGIN
										SET @DropAndReCreateColumn = 1
										
										IF @FinalFormField = 'File' OR @FinalFormField = 'Epost' OR @FinalFormField = 'Url'
											BEGIN
												SET @DataRestorePossible = 0
											END
										ELSE
											BEGIN
												SET @DataRestorePossible = 1
												SET @DataRestoreSQL = 'SUBSTRING(CAST(' + @NewColName + ' AS VARCHAR), 1 ,' + CAST(@MaxLength AS VARCHAR) + ')'
											END
									END
							ELSE IF @datatype = 'text' OR @datatype = 'ntext'
								BEGIN
										SET @DropAndReCreateColumn = 1
										SET @DataRestorePossible = 1
										SET @DataRestoreSQL = 'CAST(' + @NewColName + ' AS VARCHAR)'
								END
						END
						
				ELSE IF @olddatatype = 'ntext' OR @olddatatype = 'text'
						BEGIN
							IF @datatype = 'bit' OR @datatype = 'datetime' OR @datatype = 'decimal' OR @datatype = 'image' OR @datatype = 'int'
									BEGIN
										SET @DropAndReCreateColumn = 1
										SET @DataRestorePossible = 0
									END
							ELSE IF @datatype = 'ntext' OR @datatype = 'text' OR @datatype = 'varchar' OR @datatype = 'nvarchar'
									BEGIN
										IF (@datatype = 'varchar' OR @datatype = 'nvarchar')
											AND (@FinalFormField = 'Epost' OR @FinalFormField = 'Url' OR @FinalFormField = 'File' )
											BEGIN
												SET @DropAndReCreateColumn = 1
												SET @DataRestorePossible = 0
											END
										ELSE
											BEGIN
												SET @DropAndReCreateColumn = 1
												SET @DataRestorePossible = 1
												IF @datatype = 'varchar' OR @datatype = 'nvarchar'
													SET @DataRestoreSQL = 'CAST(' + @NewColName + ' AS VARCHAR)'
												ELSE	
													SET @DataRestoreSQL = @NewColName 
											END
									END
						END
				ELSE IF @olddatatype = 'varchar' OR @olddatatype = 'nvarchar'
						BEGIN
							IF @datatype = 'bit' OR @datatype = 'datetime' OR 
								@datatype = 'decimal' OR @datatype = 'image' OR @datatype = 'int'
									BEGIN
										SET @DropAndReCreateColumn = (1 & @DataObjectHasData)
										SET @DataRestorePossible = 0
									END
							ELSE IF @datatype = 'ntext' OR @datatype = 'text'
									SET @DropAndReCreateColumn = 0
									
							ELSE IF @datatype = 'nvarchar' OR @datatype = 'varchar'
									BEGIN
										IF (@OldFormField = 'Epost' AND @FinalFormField = 'Url') OR
											(@OldFormField = 'Epost' AND @FinalFormField = 'File') OR
											(@OldFormField = 'Url' AND @FinalFormField = 'Epost') OR
											(@OldFormField = 'Url' AND @FinalFormField = 'File') OR
											(@OldFormField = 'String' AND @FinalFormField = 'Epost') OR
											(@OldFormField = 'String' AND @FinalFormField = 'Url') OR
											(@OldFormField = 'String' AND @FinalFormField = 'File') OR
											(@OldFormField = 'Unicode String' AND @FinalFormField = 'Epost') OR
											(@OldFormField = 'Unicode String' AND @FinalFormField = 'Url') OR
											(@OldFormField = 'Unicode String' AND @FinalFormField = 'File') OR
											(@OldFormField = 'File' AND @FinalFormField = 'Epost') OR
											(@OldFormField = 'File' AND @FinalFormField = 'Url')
											BEGIN
												SET @DropAndReCreateColumn = 1
												SET @DataRestorePossible = 0
											END
										ELSE
											BEGIN
												IF @DataObjectHasData = 0
													SET @DropAndReCreateColumn = 0
												ELSE IF @MaxLength >= @oldMaxLength
													SET @DropAndReCreateColumn = 0
												ELSE
													--New Max Length is less than the old one, so truncate	
													BEGIN
														SET @DropAndReCreateColumn = 1
														SET @DataRestorePossible = 1
														SET @DataRestoreSQL = 'SUBSTRING(' + @NewColName + ', 1 ,' + CAST(@MaxLength AS VARCHAR) + ')'
													END
											END
									END
						END
				
				IF @olddatatype='decimal'
				BEGIN
					UPDATE dbo.datafield SET [Precision]=NULL,Scale=NULL WHERE ObjectID=@datafieldid

					IF @@Error <> 0
					BEGIN
						RAISERROR('can not convert from decimal to (Integer,decimal and bit)',16,1)
						GOTO OnError
					END
				END
				/*IF @MaxLength < @oldMaxLength
				BEGIN
					RAISERROR('1: Max Length can not be smaller than the old Max Length.',16,1)
					GOTO OnError
				END*/

				IF @DropAndReCreateColumn = 0
					BEGIN
						SET @sqlstring='ALTER TABLE dbo.'+@tablename+' ALTER COLUMN '+@datacolumn

						SELECT  @datatype=dbo.GetDataType(@datatype,@MaxLength,@precision,@scale)
			  
						SET @sqlstring=@sqlstring+ ' ' + @datatype
						
						EXEC(@sqlstring)
						IF @@Error<> 0 GOTO OnError
					END
				ELSE
					BEGIN
						--First Rename the column some other value
						SET @sqlstring = 'sp_rename ''dbo.' + @tablename + '.' + @datacolumn + ''', ''' + @NewColName + ''',''COLUMN'''
						EXEC(@sqlstring)
						IF @@Error<> 0 GOTO OnError
						
						SELECT  @datatype = dbo.GetDataType(@datatype,@MaxLength,@precision,@scale)
						
						SET @sqlstring = 'ALTER TABLE dbo.' + @tablename + ' ADD ' + @datacolumn + ' ' + @datatype
						EXEC(@sqlstring)
						IF @@Error<> 0 GOTO OnError
						
						--Set the values back to the new column if it can be preserved.
						IF @DataRestorePossible = 1
						BEGIN
							SET @sqlstring = 'UPDATE dbo.' + @tablename + ' SET ' +  @datacolumn + ' = ' + @DataRestoreSQL
							EXEC(@sqlstring)
						END
						
						SET @sqlstring = 'ALTER TABLE dbo.' + @tablename + ' DROP COLUMN ' + @NewColName
						EXEC(@sqlstring)
						IF @@Error<> 0 GOTO OnError
					END
				
				--PRINT  @sqlstring
	
				IF @datatype='text' or @datatype='ntext'
				BEGIN
					INSERT INTO @tempview_datafieldid(vid) SELECT dataviewid FROM dbo.vie_dataview WHERE DataFieldID=@datafieldid and type=0
					DELETE FROM dbo.viewbuilder WHERE   datafieldid=@datafieldid 

					IF @@Error <> 0
					BEGIN
						RAISERROR('Can not convert from decimal to (Integer, decimal and bit)',16,1)
						GOTO OnError
					END
					SELECT @counter=COUNT(*) FROM @tempview_datafieldid
					
					SET @i=0
					WHILE @i< @counter
					BEGIN
						SELECT TOP 1 @dataviewid=vid FROM @tempview_datafieldid
						
						EXEC dbo.ins_DataView_TextImageFields_Backend @DataViewID=@DataViewID, @DataFieldID=@DataFieldID, @ReturnValue=@ReturnValue OUTPUT
						
						IF @returnvalue <> 0
						BEGIN
							RAISERROR('Internal call to procedure ins_dataview_textimagefields_backend failed.',16,1)
							GOTO OnError
						END
						DELETE FROM @tempview_datafieldid WHERE vid=@dataviewid
						SET @counter=@counter-1
					END
				END

				--execute procedure to UPDATE all the noneditable views
				EXEC upd_datafield_backend @dataObjectID,NULL,@returnvalue OUTPUT 
				IF @returnvalue <> 0
				BEGIN
					RAISERROR('Internal call to the procedure upd_datafield_backend failed.',16,1)
					GOTO OnError
				END

				-- Update all the views where Datafield is used in paramter
				INSERT INTO @tempview_datafieldid (vid)
				SELECT dataviewid
				FROM   dbo.vie_dataview
				WHERE  editable   = 1
				AND Viewtype = 2
				AND type =1
				AND datafieldid = @datafieldid

				SELECT @counter = COUNT(*) FROM @tempview_datafieldid 

				SET @i=0
				WHILE @i < @counter
				BEGIN
					SELECT TOP 1 @dataviewid=vid FROM @tempview_datafieldid 
					EXEC dbo.ins_DataView_Backend @DataViewID=@DataViewID

					IF @@Error <> 0
						GOTO OnError

					DELETE FROM @tempview_datafieldid  WHERE vid=@dataviewid
					SET @counter=@counter-1
				END        


			END --  end of when datatye is changed
				--***********************************
			-- when the filed name is changed, update all the editable views
			ELSE IF   @oldfieldname <> @fieldname
			BEGIN
				-- insert all editable views where field exists      
				INSERT INTO @tempview_datafieldid (vid)
				SELECT dataviewid
				FROM   dbo.vie_dataview
				WHERE  editable   = 1
				AND datafieldid = @datafieldid

				INSERT INTO @tempview_datafieldid (vid)
				SELECT dataviewid
				FROM   dbo.vie_dataview
				WHERE editable     = 0
				AND viewtype     = 2
				AND datafieldid = @datafieldid

				--updates views
				SELECT @counter = COUNT(*) FROM @tempview_datafieldid 

				SET @i=0
				WHILE @i < @counter
				BEGIN
					SELECT TOP 1 @dataviewid=vid FROM @tempview_datafieldid 
					EXEC dbo.ins_DataView_Backend @DataViewID=@DataViewID

					IF @@Error <> 0
						GOTO OnError

					DELETE FROM @tempview_datafieldid  WHERE vid=@dataviewid
					SET @counter=@counter-1
				END        
			END -- end updating views on fieldname change
	  
			--***********************************
			-- modify table to not allow Null values 
			IF @oldrequired=1 and @required= 0 and @updatetable=0
			BEGIN
				SELECT @datatype=typename FROM dbo.datatype WHERE ObjectID=@datatypeid
				SET @sqlstring='ALTER TABLE dbo.'+@tablename+' ALTER COLUMN '+@datacolumn

				SELECT  @datatype=dbo.GetDataType(@datatype,@MaxLength,@precision,@scale)
	  
				SET @sqlstring=@sqlstring+ ' ' + @datatype
				--PRINT  @sqlstring
				EXEC(@sqlstring)

				IF @@Error<> 0 GOTO OnError
			END
		--****************************************** 
		-- modify table to change max length
			IF @oldmaxlength <> @maxlength and @updatetable=0
			BEGIN
			/*	IF @MaxLength < @oldMaxLength
				BEGIN
					RAISERROR('1: Max Length can not be smaller than the old Max Length',16,1)
					GOTO OnError
				END*/

				SELECT @datatype=typename FROM dbo.datatype WHERE ObjectID=@datatypeid
				SET @sqlstring='ALTER TABLE dbo.'+@tablename+' ALTER COLUMN '+@datacolumn

				SELECT @datatype=dbo.GetDataType(@datatype,@MaxLength,@precision,@scale)
	  
				SET @sqlstring=@sqlstring+ ' ' + @datatype
				--PRINT  @sqlstring
				EXEC(@sqlstring)

				IF @@Error<> 0 GOTO OnError

				-- change the insert and update procedures 
				--execute procedure to UPDATE all the noneditable views
				EXEC upd_datafield_backend @dataObjectID,NULL,@returnvalue OUTPUT 
				IF @returnvalue <> 0
				BEGIN
					RAISERROR('Internal call to the procedure upd_datafield_backend failed.',16,1)
					GOTO OnError
				END

				-- Update all the views where Datafield is used in paramter
				INSERT INTO @tempview_datafieldid (vid)
				SELECT dataviewid
				FROM   dbo.vie_dataview
				WHERE  editable   = 1
				AND Viewtype = 2
				AND type =1
				AND datafieldid = @datafieldid

				SELECT @counter = COUNT(*) FROM @tempview_datafieldid 

				SET @i=0
				WHILE @i < @counter
				BEGIN
					SELECT TOP 1 @dataviewid=vid FROM @tempview_datafieldid 
					EXEC dbo.ins_DataView_Backend @DataViewID=@DataViewID

					IF @@Error <> 0
						GOTO OnError

					DELETE FROM @tempview_datafieldid  WHERE vid=@dataviewid
					SET @counter=@counter-1
				END        
			END
		END -- end of check if table exists and modify changes
	END
	
	-- Code to check error after executing Insert, Update or Delete statement
	IF @@Error <> 0
	BEGIN
		RAISERROR ('Unable to update table.', 16, 1)
		GOTO OnError
	END

OnSuccess:
	COMMIT TRAN
	GOTO OnEnd
OnError:
	ROLLBACK TRAN
OnEnd:
	RETURN
GO


GRANT EXECUTE ON [dbo].[upd_DataField] TO [DigiAdminRole]--, [DigiWebRole]
GO

/**** EXTENDED PROPERTIES *****************************************************/

-- Purpose of the procedure
EXEC sp_addextendedproperty 
	N'Purpose', N'Purpose of Stored Procedure', 
	N'USER', N'dbo', 
	N'PROCEDURE', N'upd_DataField', 
	NULL, NULL
GO
-- Resturned result sets of the stored procedure
EXEC sp_addextendedproperty 
	N'Result', N'Recordset information.', 
	N'USER', N'dbo', 
	N'PROCEDURE', N'upd_DataField', 
	NULL,NULL
GO
-- Parameter description
EXEC sp_addextendedproperty 
	N'Description', N'Specifies the type of information to retrieve. Refer to the Result section for details.', 
	N'USER', N'dbo', 
	N'PROCEDURE', N'upd_DataField', 
	N'PARAMETER', N'@Type'
GO
