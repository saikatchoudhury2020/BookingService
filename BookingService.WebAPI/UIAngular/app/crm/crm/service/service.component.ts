import { Component, OnInit } from '@angular/core';
import { CrmService } from '../crm.service';
import { ActivatedRoute, Router } from "@angular/router";
@Component({
  selector: 'app-service',
  templateUrl: './service.component.html',
  styleUrls: ['./service.component.css']
})
export class ServiceComponent implements OnInit {

  serviceForm: any;
  constructor(private _crmService: CrmService, private _route: ActivatedRoute,
      private _router: Router) {

  }

  ngOnInit(): void {
      this._route.params.subscribe(params => {

          this._crmService.GetArticleDetail(params.id).subscribe(data => {
              this.serviceForm = data;
          });
      });

  }

}
