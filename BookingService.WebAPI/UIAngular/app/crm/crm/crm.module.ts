import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CrmComponent } from './crm.component';
import { FormsModule } from '@angular/forms';
import { BuildingComponent } from './building/building.component';
import { MenuComponent } from './menu/menu.component';
import { MessageListComponent } from './message-list/message-list.component';
import { Ng2DatetimePickerModule, Ng2Datetime } from 'ng2-datetime-picker';
import { OrderlistComponent } from './orderlist/orderlist.component';
import { OrganizationComponent } from './organization/organization.component';
import { ServiceComponent } from './service/service.component';
import { ServiceGroupComponent } from './service-group/service-group.component';
import { UserComponent } from './user/user.component';
import { UserEditComponent } from './user-edit/user-edit.component';
import { UsersgrouplistComponent } from './usersgrouplist/usersgrouplist.component';
import { UserslistComponent } from './userslist/userslist.component';
import { NgxLoadingModule, ngxLoadingAnimationTypes } from "ngx-loading";
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { SharedModule } from '../../Shared/shared.module';
import { SearchFilterPipe } from '../pipe/search-filter.pipe';
import { CacheManageComponent } from './cache-manage/cache-manage.component';
import { AgreementTypesComponent } from './agreement-types/agreement-types.component';



@NgModule({
  imports: [ReactiveFormsModule, CommonModule,
    Ng2DatetimePickerModule,
    NgxLoadingModule.forRoot({
      animationType: ngxLoadingAnimationTypes.circleSwish,
      backdropBackgroundColour: "rgba(0,0,0,0.1)",
      backdropBorderRadius: "4px",
      primaryColour: "#ffffff",
      secondaryColour: "#ccc",
      tertiaryColour: "#563d7c"
    })
    ,
    RouterModule.forChild([
      /*
             {
                 path: 'user/:id',
                 component: UserComponent
             },
             {
                 path: 'userDetail/:id',
                 component: UserEditComponent
             },
             {
                 path: 'building/:id',
                 component: BuildingComponent
             },
             {
                 path: 'service/:id',
                 component: ServiceComponent
             },
             {
                 path: 'serviceGroup/:id',
                 component: ServiceGroupComponent
             },
             {
                 path: 'organization/:id',
                 component: OrganizationComponent
             },
             {
                 path: 'userlist',
                 component: UserslistComponent
             },
             {
                 path: 'usersgrouplist/:id',
                 component: UsersgrouplistComponent
             },
             {
                 path: 'messagelist',
                 component: MessageListComponent
             },
             {
                 path: 'orderlist',
                 component: OrderlistComponent
             },
         */
    ])
    , SharedModule, FormsModule
  ],
  declarations: [CrmComponent, SearchFilterPipe, BuildingComponent, MenuComponent, MessageListComponent, OrderlistComponent, OrganizationComponent, ServiceComponent, ServiceGroupComponent, UserComponent, UserEditComponent, UsersgrouplistComponent, UserslistComponent, CacheManageComponent, AgreementTypesComponent]
})
export class CrmModule { }
