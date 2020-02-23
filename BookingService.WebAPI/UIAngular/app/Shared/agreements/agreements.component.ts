import { Component,Input, OnInit, OnChanges } from '@angular/core';
import { CrmService } from "../../crm/crm/crm.service"
import { UserListService } from '../user-list/user-list.service';
import * as _ from 'lodash';
@Component({
  selector: 'app-agreements',
  templateUrl: './agreements.component.html',
  styleUrls: ['./agreements.component.css']
})
export class AgreementsComponent implements OnInit,OnChanges {

  @Input() userid: number;
  @Input() orgId: number;

  agreementTypes: any;
  agreements:any;
  userList:any;
  display = 'none';
  messageDisplay = 'none';

  AgreementId:number;
  FromDate:string;
  ToDate:string;
  AgreementTypeId:number;
  Name:string;
  Article:string;
  Text:string;
  Price:string;
  NumberOfMonths:number;
  InvoicedToDate:string;

  constructor( private _crmService: CrmService,private _userListService: UserListService) { }

  ngOnInit() {
    this.onGetAgreementsList();
    this.onGetAgreementTypesList();
    this.onGetuserListData();
  }
  ngOnChanges(): void {
    this.onGetAgreementsList();
    this.onGetAgreementTypesList();
    this.onGetuserListData();
  }
  openModal(data:any) {
    if (data) {
      this.AgreementId=data.AgreementId;
      this.userid=data.Contact;
      this.AgreementTypeId=data.AgreementTypeId;
      this.FromDate=data.FromDate;
      this.ToDate=data.ToDate;
      this.Article=data.Article;
      this.Text=data.Text;
      this.Price=data.Price.toString().replace(".",",") ;
      this.NumberOfMonths=data.NumberOfMonths;
      this.InvoicedToDate=data.InvoicedToDate;
  }else{
    this.AgreementId=0;
    this.AgreementTypeId=0;
    if(this.orgId>0){
      this.userid=0;
    }
    var d = new Date().getMonth()+1;
    this.FromDate="01."+d.toString()+"."+new Date().getFullYear().toString();
    this.ToDate="";
    this.Article="";
    this.Text="";
    this.Price=null;
    this.NumberOfMonths=null;
    this.InvoicedToDate="";
  }
  this.display = 'block';
}
  onGetAgreementsList() {
    this._crmService.GetAgreementsList(this.orgId,this.userid).subscribe(data => {
        this.agreements = data;
    });
}
onGetAgreementTypesList() {
  this._crmService.GetAgreementTypesList().subscribe(data => {
      this.agreementTypes = data;
  });
}
onGetuserListData() {
this._userListService.UserListByBuilding(0, this.orgId).subscribe(
  data => {
      this.userList = data;

  });
}
save(): void {
  console.log(this.Name);
  this._crmService.AddUpdateAgreements(this.AgreementId,this.orgId,this.userid,this.AgreementTypeId,this.FromDate,this.ToDate,this.Article,this.Text,this.Price,this.NumberOfMonths,this.InvoicedToDate).subscribe(data => {
      this.onGetAgreementTypesList(); 
      this.onGetAgreementsList();
      this.onCloseHandled();
  });
}
onCloseMessage() {
  this.messageDisplay = 'none';
}
onCloseHandled() {
  this.display = 'none';
}

filterList = (AgreementTypeId:any): void => {
  let agreement = _.find(this.agreementTypes, x => x.AgreementTypeId == AgreementTypeId);
  this.Article=agreement.Article;
  this.Text=agreement.Text;
  this.Price=agreement.Price;
  this.NumberOfMonths=agreement.NumberOfMonths;
}
}
