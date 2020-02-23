import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { IpadComponent } from './ipad/ipad.component';
import { AppComponent } from './app.component';
import { BookingComponent } from './booking/booking.component';
import { KitchenComponent } from './kitchen/kitchen.component';
import { KitchenprintComponent } from './kitchen/kitchenprint.component';
import { MainComponent } from './crm/main.component';


import { BuildingComponent } from './crm/crm/building/building.component';
import { MenuComponent } from './crm/crm/menu/menu.component';
import { MessageListComponent } from './crm/crm/message-list/message-list.component';
import { OrderlistComponent } from './crm/crm/orderlist/orderlist.component';
import { OrganizationComponent } from './crm/crm/organization/organization.component';
import { ServiceComponent } from './crm/crm/service/service.component';
import { ServiceGroupComponent } from './crm/crm/service-group/service-group.component';
import { UserComponent } from './crm/crm/user/user.component';
import { UserEditComponent } from './crm/crm/user-edit/user-edit.component';
import { UsersgrouplistComponent } from './crm/crm/usersgrouplist/usersgrouplist.component';
import { UserslistComponent } from './crm/crm/userslist/userslist.component';
import { CacheManageComponent } from './crm/crm/cache-manage/cache-manage.component';
import { AgreementTypesComponent } from './crm/crm/agreement-types/agreement-types.component';
const routes: Routes = [
{
    path: 'ipad',
    pathMatch: 'full',
    component: IpadComponent
},

{
    path: 'kitchenprint/:bookingid',
    pathMatch: 'full',
    component: KitchenprintComponent
},

{
    path: 'kitchen',
    pathMatch: 'full',
    component: KitchenComponent
},

{
	path: 'crm',
	component: MainComponent,
	children:[
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
        },{
            path: 'cache',
            component: CacheManageComponent
        },{
            path: 'agreementtypes',
            component: AgreementTypesComponent
        }
		
	]
},

{
    path: '',
    pathMatch: 'full',
    component: BookingComponent
},

];

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
	RouterModule.forRoot(routes, {useHash: true })
  ],
  exports: [ RouterModule ]
})

export class AppRoutingModule { }
