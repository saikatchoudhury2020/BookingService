import { Component, OnInit } from '@angular/core';
import { CrmService } from '../crm.service';


@Component({
  selector: 'app-agreement-types',
  templateUrl: './agreement-types.component.html',
  styleUrls: ['./agreement-types.component.css']
})
export class AgreementTypesComponent implements OnInit {
  agreementTypes: any;
  display = 'none';
  messageDisplay = 'none';
  AgreementTypeId:number;
  Name:string;
  Article:string;
  Text:string;
  Price:string;
  NumberOfMonths:number;

  constructor( private _crmService: CrmService) { }

  ngOnInit() {
    this.onGetAgreementTypesList();
  }
  openModal(data:any) {
    if (data) {
      this.AgreementTypeId=data.AgreementTypeId;
      this.Name=data.Name;
      this.Article=data.Article;
      this.Text=data.Text;
      this.Price=data.Price.toString().replace(".",",") ;
      this.NumberOfMonths=data.NumberOfMonths;
  }else{
    this.AgreementTypeId=0;
    this.Name="";
    this.Article="";
    this.Text="";
    this.Price=null;
    this.NumberOfMonths=null;
  }

  this.display = 'block';
}
  onGetAgreementTypesList() {
    this._crmService.GetAgreementTypesList().subscribe(data => {
        this.agreementTypes = data;
    });
}
save(): void {
  console.log(this.Name);
  this._crmService.AddUpdateAgreementTypes(this.AgreementTypeId,this.Name,this.Article,this.Text,this.Price,this.NumberOfMonths).subscribe(data => {
    this.onCloseHandled(); 
    this.onGetAgreementTypesList();
  });
}
onCloseMessage() {
  this.messageDisplay = 'none';
}
onCloseHandled() {
  this.display = 'none';
}
}
