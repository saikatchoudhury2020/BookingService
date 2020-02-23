import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import {Ng2DatetimePickerModule, Ng2Datetime } from 'ng2-datetime-picker';
import { AppComponent } from './app.component';
import {SelectModule} from 'angular2-select';
import {HttpModule} from '@angular/http';
import { RouterModule, Routes } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ToastrModule } from 'ng6-toastr-notifications';
import { AppService } from './app.service';
import {SharedModule} from './Shared/shared.module';
import { IpadComponent } from './ipad/ipad.component';
import { AppRoutingModule } from './app-routing.module';
import { BookingComponent } from './booking/booking.component';
import { KitchenComponent } from './kitchen/kitchen.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import {MainModule} from './crm/main.module';


import { confirmAlertService } from './Shared/confirmationAlert/service/confirmAlert.service';

import { KitchenprintComponent } from './kitchen/kitchenprint.component';
import { ToastService } from './toast.service';
//import { DlDateTimePickerDateModule } from 'angular-bootstrap-datetimepicker';

Ng2Datetime.firstDayOfWeek = 1;
Ng2Datetime.daysOfWeek = [
{ fullName: 'Søndag', shortName: 'sø', weekend: true },
{ fullName: 'mandag', shortName: 'ma' },
{ fullName: 'tirsdag', shortName: 'ti' },
{ fullName: 'onsdag', shortName: 'on' },
{ fullName: 'torsdag', shortName: 'to' },
{ fullName: 'fredag', shortName: 'fr' },
{ fullName: 'lørdag', shortName: 'lø', weekend: true }
];

Ng2Datetime.months = [{fullName: 'januar', shortName: 'Jan'},
    {fullName: 'februar', shortName: 'feb'},
    {fullName: 'mars', shortName: 'mar'},
    {fullName: 'april', shortName: 'apr'},
    {fullName: 'mai', shortName: 'mai'},
    {fullName: 'juni', shortName: 'jun'},
    {fullName: 'juli', shortName: 'jul'},
    {fullName: 'august', shortName: 'aug'},
    {fullName: 'september', shortName: 'sep'},
    {fullName: 'oktober', shortName: 'okt'},
    {fullName: 'november', shortName: 'nov'},
    {fullName: 'desember', shortName: 'dec'}
  ];
  
  
@NgModule({
  declarations: [
    AppComponent,
    IpadComponent,
    BookingComponent,
	KitchenComponent,
   
    KitchenprintComponent
  ],
  imports: [
    SharedModule,BrowserModule,BrowserAnimationsModule,ToastrModule.forRoot(),FormsModule, ReactiveFormsModule, HttpModule, Ng2DatetimePickerModule, SelectModule, AppRoutingModule, MainModule
  ],
  providers: [AppService,confirmAlertService,ToastService],
  bootstrap: [AppComponent]
})
export class AppModule { }
