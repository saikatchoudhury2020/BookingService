import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ConfirmAlertComponent } from './confirmationAlert/confirmAlert.component';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';

import { Ng2DatetimePickerModule, Ng2Datetime } from 'ng2-datetime-picker';

import { SelectModule } from 'angular2-select';

import { NgSelectModule } from '@ng-select/ng-select';

import { ServiceAlertService } from "./service-alert/service-alert.Service";
import { confirmAlertService } from './confirmationAlert/service/confirmAlert.service';
import { RouterModule } from "@angular/router";
import { MessageListComponent } from './message/message-list.component';
import { CRMMessageListComponent } from './message-list/message-list.component';
import { BookingDialogComponent } from './booking/booking-dialog.component';
import { BookingDialogService } from './booking/booking-dialog.service';
import { UserGroupComponent } from './user-group/user-group.component';
import { UserListComponent } from './user-list/user-list.component';
import { OrganizationListComponent } from './organization-list/organization-list.component';
import { UserGroupService } from './user-group/user-group.service';
import { UserListService } from './user-list/user-list.service';
import { OrganizationListService } from './organization-list/organization-list.service';
import { NoteComponent } from '../Shared/note/note.component';
import { NoteService } from './note/note.component.service';
import { CacheComponent } from './cache/cache.component';


import { AutoSelectComponent } from './auto-select/auto-select.component';
import { CustomerComponent } from './customer/customer.component';
import { UserComponent } from './user/user.component';;
import { BookingServiceComponent } from './booking-service/booking-service.component'
  ;
import { ServiceAlertComponent } from './service-alert/service-alert.component'
  ;
import { AgreementsComponent } from './agreements/agreements.component'
  ;
import { OrderComponent } from './order/order.component'
@NgModule({
  imports: [
    Ng2DatetimePickerModule, CommonModule, ReactiveFormsModule, FormsModule, SelectModule, RouterModule.forChild([])
  ],
  declarations: [
    ConfirmAlertComponent, CRMMessageListComponent, MessageListComponent, BookingDialogComponent, OrganizationListComponent, UserGroupComponent, UserListComponent
    , NoteComponent
    , CacheComponent
    , AutoSelectComponent
    , CustomerComponent
    , UserComponent, BookingServiceComponent, ServiceAlertComponent, AgreementsComponent, OrderComponent],
  providers: [UserGroupService, UserListService, OrganizationListService, confirmAlertService, ServiceAlertService, BookingDialogService, NoteService],
  exports: [

    ConfirmAlertComponent,
    CommonModule,
    OrderComponent,
    FormsModule,
    MessageListComponent,
    CRMMessageListComponent,
    BookingDialogComponent,
    BookingServiceComponent,
    UserGroupComponent,
    UserListComponent,
    CustomerComponent,
    UserComponent,
    OrganizationListComponent, NoteComponent, CacheComponent, AutoSelectComponent, ServiceAlertComponent, AgreementsComponent

  ]
})
export class SharedModule { }
