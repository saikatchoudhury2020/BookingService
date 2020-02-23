import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import {AppService} from '../../app.service';
import { FormGroup, FormBuilder, Validators, AbstractControl, ValidatorFn, FormArray, FormControl } from '@angular/forms';

@Component({
  selector: 'app-message-list',
  templateUrl: './message-list.component.html',
  styleUrls: ['./message-list.component.css']
})
export class MessageListComponent implements OnInit {

   @Input() bookingId:number = 0;

   //output: when done, $event is the "messageList" 
   @Output() done = new EventEmitter<Array<any>>(); 
   
   messageList:Array<any> = [];
 
   templateList:Array<any> = [];  

   selectType:string = 'email';
   
   messageForm:any;
   
   //todo: with email and phone number as input.
   to:Array<string> = [];   
   
  constructor( private _service: AppService ) {

  }

  ngOnInit() {
	  this.initForm();
	  	  	  
	  //get existing list	  
	  this._service.send( 'Booking/GetMessageList', { bookingID: this.bookingId } ).subscribe( data =>{		
		   var list = [];
		   data.forEach( function(value){
				list.push( { id: value.ID, 
									  type: value.Type, 
									  to: value.To, 
									  subject: value.Subject,
									  body: value.Body, 
									  attachments: value.Attachments, 
									  sendTime: value.SendTime } );
		   } );
		   this.messageList = list;
	  });
	 
	 //get template list
	 this._service.send( 'Booking/GetMessageTemplateList' ).subscribe( data =>{				   
		   this.templateList = data;
	  });	 	 	 	  
  }

  private initForm()
  {
	  this.messageForm = { id:0, type: '', to:'', subject: '', body: '', attachments: '', sendTimeTime:'00:00' };
  }
  
  
  //choose a template
  chooseTemplate( currentTemplate ):void{
	  for( var i=0; i<this.templateList.length;i++)
	  {
		  var template = this.templateList[i];
		  if( template.id == currentTemplate )
		  {
			  if( this.selectType == 'email' )
			  {
				  this.messageForm.subject = template.subject;
			  }
			  this.messageForm.currentBody = template.body;			  
		  }
		  
	  };
  }
  
  //click add button
  addMessage( ):void {	  
	if( this.messageForm.id == 0 )
	{		
		this.messageList.push( this.updatedFormModel() );
	}
	else
	{
	  for( var i=0; i< this.messageList.length; i++ )
	  {
			var message = this.messageList[i];
			if( message.id == this.messageForm.id )
			{
				this.messageList[i] = this.updatedFormModel();				
				this.messageList[i]['updated'] = 1;
				break;
			}
	  }
	}
	this.initForm();
	this.done.emit( this.messageList );
  }
  
  //click edit button
  edit( i:number ){	 
	var message = this.messageList[i];
	message.currentBody = message.body;
	this.messageForm = JSON.parse(JSON.stringify( message ));
  }
  
  //click delete button
  delete( i:number ){
	   //todo: confirm dialog
	  var id = this.messageList[i].id;
	  if( id > 0 )
	  {
		this.messageList[i].id=-id;
	  }
	  else if( id == 0 )
	  {
		  this.messageList.splice( i, 1 );
	  }
	  this.done.emit( this.messageList );
  }
  
  //conver between input data and send data
  updatedFormModel():any{
	 if( this.messageForm.sendTimeDate )
	 {
		this.messageForm['sendTime'] = this.messageForm.sendTimeDate.toString() + ' ' + this.messageForm.sendTimeTime;
	 }
	  return this.messageForm;
  }
  
}
