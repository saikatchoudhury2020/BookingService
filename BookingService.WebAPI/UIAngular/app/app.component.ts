import { Component,OnInit,ViewChild } from '@angular/core';
import {AppSettings} from './appConfig';
import { FormGroup, FormBuilder, Validators, AbstractControl, ValidatorFn, FormArray, FormControl } from '@angular/forms';
import { Property } from './model/Property';
import { PropertyService } from './model/PropertyService';
import { MeetingRoom } from './model/MeetingRoom';
import { Services } from './model/Services';
import {AppService} from './app.service';
import{confirmAlertService}from './Shared/confirmationAlert/service/confirmAlert.service';
import { Subscription, Observable } from 'rxjs';
import {timer} from 'rxjs';
import * as moment from 'moment';

import * as _ from 'lodash';
// import * as $ from 'jquery';
declare var jquery: any;
declare var $: any;


@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {	
  
	ngOnInit():void{
	   moment.locale("nb"); 
	   moment().format('L');
	}
}
