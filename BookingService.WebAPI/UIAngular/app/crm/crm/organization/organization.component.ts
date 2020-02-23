import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from "@angular/router";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { CrmService } from '../crm.service';
import { confirmAlertService } from 'src/app/Shared/confirmationAlert/service/confirmAlert.service';
import { Subscription, Observable } from 'rxjs';
import { timer } from 'rxjs';

declare var jquery: any;
declare var $: any;
@Component({
  selector: 'app-organization',
  templateUrl: './organization.component.html',
  styleUrls: ['./organization.component.css']
})
export class OrganizationComponent implements OnInit {
  public showloader: boolean = false;
  private subscription: Subscription;
  private timer: Observable<any>;
  orgData: any;
  display = 'none';
  displayMessage = '';
  color = '';
  selectedUserId: number;
  selectedOrg: number;
  isMva: boolean;
  Code: boolean;
  selectedOrgId: any;
  orgForm: FormGroup;
  imagePath: string = '';
  constructor(private fb: FormBuilder, private _route: ActivatedRoute,
    private _router: Router,
    private _crmService: CrmService, private _confirmService: confirmAlertService) {
  }
  ngOnInit() {
    this._route.params.subscribe(params => {
      this._crmService.GetOrgDetail(params.id).subscribe(data => {
        this.orgData = data;
        this.Code = data.Code;
        console.log(this.orgData);
      });
    });

    //  this.openModal();
    this.onCloseHandled();
  }
  save(): void {
    console.log(this.orgForm.value);
  }
  openModal(userId: number, orgId: number) {
    this.selectedUserId = userId;
    this.selectedOrg = orgId;
    this.display = 'block';
    $('.bookModal').show();
  }
  onCloseHandled() {
    this.display = 'none';
  }
  refresh(message: string): void {
    this._crmService.GetOrgDetail(this.selectedOrg.toString()).subscribe(data => {
      this.orgData = data;
      console.log(this.orgData);
    });
  }
  onIsMvaChange = (event: boolean, id: any): void => {
    this.selectedOrgId = id;
    this.isMva = event;
    this._confirmService.toggle();
  }

  ConfirmCommand(value: any): void {
    if (value.val) {
      this._crmService.SetMVAToCustomer(this.isMva, this.selectedOrgId).subscribe(data => {
        this._confirmService.toggle();
        if (data === 1) {
          this.displayMessage = 'MVA updated';
          this.color = 'green';
          this.setTimer();
        }
        else {
          this.displayMessage = 'MVA not updated';
          this.color = 'red';
          this.setTimer();
        }
      });


    }
    else {
      this.Code = !this.Code;
    }

  }

  public setTimer() {

    // set showloader to true to show loading div on view
    this.showloader = true;
    this.timer = timer(3000);
    //savan this.timer = Observable.timer(3000); // 5000 millisecond means 5 seconds
    this.subscription = this.timer.subscribe(() => {
      // set showloader to false to hide loading div from view after 5 seconds
      this.showloader = false;
    });
  }
}
