import { Component, OnInit } from '@angular/core';
import { CrmService } from '../crm.service';
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { ActivatedRoute, Router } from "@angular/router";
@Component({
  selector: 'app-service-group',
  templateUrl: './service-group.component.html',
  styleUrls: ['./service-group.component.css']
})
export class ServiceGroupComponent implements OnInit {

  serviceGrpForm: any;
  imagePath: string = '';
  constructor(private fb: FormBuilder, private _route: ActivatedRoute,
      private _router: Router,
      private _crmService: CrmService) {
  }
  ngOnInit() {
      this._route.params.subscribe(params => {
          this._crmService.GetServiceGrpDetail(params.id).subscribe(data => {
              this.serviceGrpForm = data;
          });
      });


  }


}
