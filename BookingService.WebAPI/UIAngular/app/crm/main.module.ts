import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { SelectModule } from 'angular2-select';
import { RouterModule } from "@angular/router";
import { HttpModule } from '@angular/http';
import { ReactiveFormsModule } from '@angular/forms';

import { MainComponent } from './main.component';

import { CrmModule } from './crm/crm.module';
import { CrmService } from './crm/crm.service';


@NgModule({
  declarations: [
    MainComponent
    
  ],
  imports: [BrowserModule, ReactiveFormsModule, HttpModule,  CrmModule, SelectModule,
     RouterModule.forChild([{
            path: '',
            component: MainComponent
        },])],
  providers: [ CrmService],
  bootstrap: [MainComponent]
})
export class MainModule { }
