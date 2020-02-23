import { Component, OnInit } from '@angular/core';
import { CrmService } from '../crm.service';
import { Router } from '@angular/router';
import { DomSanitizer } from '@angular/platform-browser';
import { FormBuilder } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';

//import { OrganizationListComponent } from '.../../shared/organization-list/organization-list.component';

@Component({
  selector: 'app-building',
  templateUrl: './building.component.html',
  styleUrls: ['./building.component.css']
})
export class BuildingComponent implements OnInit {

  menuData: any; 
  display: string ='none';
  uploadMenuUrl: string= '';
  organizationId: '';
  menuDocumentId: '';


  constructor(private fb: FormBuilder, private _route: ActivatedRoute,
      private _router: Router,
      private _crmService: CrmService,
      public sanitizer: DomSanitizer
  ) {
  }
  ngOnInit() {        
      this._route.params.subscribe(params => {
          var array = params.id.split('-');
          this.organizationId = array[2];
          this.uploadMenuUrl = '/digimaker/braathen/UploadMenu.aspx?id=' + array[3];
          this.menuDocumentId = array[3];
          this._crmService.GetMenuDetail(array[0] +'-'+ array[1]).subscribe(data => {

              this.menuData = data;
          });
      });     
  }
  openModal() {
      this.display = 'block';
  }
  onCloseHandled() {
      this.display = 'none';
  }

}
