import { Component } from '@angular/core';
import { OnInit } from '@angular/core';
import { CrmService } from './crm/crm.service';
import { AppSettings } from '../appConfig';

@Component({
  selector: 'app-crm',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.css']
})
export class MainComponent implements OnInit {
  BuildingData: Array<any> = [];

  constructor(private _crmService: CrmService) {


  }

  ngOnInit(): void {
    this._crmService.GetBuildingListDetail(AppSettings.RootPropertiesId).subscribe(data => this.BuildingData = data);

  }
}
