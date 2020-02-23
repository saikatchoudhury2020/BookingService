import { Component, OnInit } from '@angular/core';
import {AppService} from '../app.service';
import * as moment from 'moment';
import {AppSettings} from '../appConfig';
import {Router, ActivatedRoute, Params} from '@angular/router';

/*
created by xc on 2018-11-05
*/

@Component({
  selector: 'app-ipad',
  templateUrl: './ipad.component.html',
  styleUrls: ['./ipad.component.css']
})
export class IpadComponent implements OnInit {

	serviceId:number = 0;
	
	buildingId:number = 0; //property id(parent menuitem of a meeting room) //todo: remove this by improve booking api
	
	serviceParentId:number = 0;
	
	serviceName: string = '';	
	
	events: Array<any> = null;
	
	description:string = '';
	
	now:any = moment();

	isBooked:boolean = false;
	
	canBook:boolean = false;
	
	error:string = '';
	
	//New booking related
	startBook:boolean = false;
	selectedTimeGap:number = 30;
	bookingErrorMessage:string = '';
	
	constructor( private _bookingService: AppService, private activatedRoute: ActivatedRoute) {
		this.activatedRoute.queryParams.subscribe(params => {
			if( params['service'] )
			{
				this.serviceId = parseInt( params['service'] );
			}
			else
			{
				this.error = 'Invalid parameter.';
			}
		});
		
		moment.locale('nb');
   }
  
  ngOnInit() {	  
	  //todo: handle login/authentication.	 

      this._bookingService.send( 'Articles/GetMeetingroomInformation', { 'meetingroomId': this.serviceId } ).subscribe( data =>{
		  this.serviceName = data.name;
		  this.description = data.description;
		  this.serviceParentId = data.propertyId;
		  this.buildingId = data.buildingId;
		  this.canBook = !data.hasPrice;
		  
	  } );	  
	  
	  
	  this.requestService();	  
	  //auto refresh
	  setInterval( ()=>{ this.requestService();}, AppSettings.IpadRefreshTime*1000 );
  }

  
  requestService():void{	  
	  //get service
	  this._bookingService.GetBookingList( this.serviceId, moment().add( -2, 'days' ).format( 'YYYY-MM-DD' ), moment().add( 2,'days').format( 'YYYY-MM-DD' ) ).subscribe(data => {		
		  this.events = data;			
	      this.now = moment();

		  //update availability for now
   		  this.isBooked = false;
		  for( var i = 0; i<this.events.length;i++ )
		  {
			  var event = this.events[i];
			  if( moment( event.start ).isBefore( moment() ) && moment( event.end ).isAfter( moment() ) )
			  {
				  this.isBooked = true;
				  break;
			  }
		  }			
		});	  	  
  }
  
  
  bookNow(){	  
	  var timeGap = 15; //todo: get it from config file
	  var baseTime = moment( this.now.format( 'YYYY-MM-DD HH' ) + ':' + ( Math.floor( this.now.format( 'mm' )/timeGap ) * timeGap ) );
	  var fromDate = baseTime.format( 'YYYY-MM-DD' );
	  var fromTime = baseTime.format( 'HH:mm' );
	  var toDateTime = baseTime.add( this.selectedTimeGap, 'minutes' );
	  var bookingObject = { Name: 'Anonymous User', 
							NoOfPeople: 5,  //todo: confirm
							FromDate: fromDate, 
							FromTimer: fromTime, 
							ToDate: toDateTime.format( 'YYYY-MM-DD' ), 
							ToTimer: toDateTime.format( 'HH:mm' ),
							PropertyId: this.buildingId,
							PropertyServiceId: this.serviceParentId, 
							MeetingRoomId: this.serviceId,
							nameofbook: 'Anonymous',
							UserId: AppSettings.AnonymousBookingUserID,
							Note: 'Booked on ' + this.now.format( 'DD.MM.YYYY HH:mm:ss' ) + ' from iPad.',
							Foods: 
							[{
								ArticleId: "123456789",
								FoodId: this.serviceId,
								IsKitchenOrder: false,
								IsVatApply: true,
								MainServiceId: this.serviceId,
								OrderHeadId: 0,
								Price: 0,
								Qty: 1,
								Sum: 0,
								Tekst: this.serviceName}] //todo: change this
							};
	  this._bookingService.Book( bookingObject ).subscribe(data => {
			if( data.errorType == 0 )
			{
				this.requestService();
			}		
			else
			{
				this.bookingErrorMessage = data.data;
			}
		});	  
  }
  
  datetimeFormat( datetime:string, format: string = 'HH.mm' ):string{
		return moment( datetime ).format( format );
	}
	
  laterThanNow( datetime:string ):boolean{
	  return moment( datetime ).isAfter( moment() );
  }
  
  isInMeeting( start:string, end:string ):boolean{
	  return moment( start ).isBefore( this.now ) && moment( end ).isAfter( this.now );
  }

  
  
}
