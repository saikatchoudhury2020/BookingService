import { Component, Input, OnInit, OnChanges, EventEmitter, Output } from "@angular/core";

import { FormBuilder, FormArray, FormGroup, FormControl, AbstractControl } from "@angular/forms";
import { Router } from "@angular/router";
import { OrganizationListService } from "./organization-list.service";

@Component({
  selector: 'organization-list',
  templateUrl: './organization-list.component.html',
  styleUrls: ['./organization-list.component.css']
})
export class OrganizationListComponent implements OnInit {

  @Input() parentId: number;
  organizationListData: any;
  constructor(private _organizationListService: OrganizationListService, private _router: Router) { }
  ngOnInit(): void {
      this._organizationListService.GetOrganizationList(this.parentId).subscribe(data => this.organizationListData = data);


  }

  refresh(message: string): void {

  }

}
