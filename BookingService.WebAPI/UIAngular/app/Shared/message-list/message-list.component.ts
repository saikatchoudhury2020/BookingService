import { Component, Input, OnInit, OnChanges, EventEmitter, Output } from "@angular/core";
import { UserListService } from '../user-list/user-list.service';
import { Router } from "@angular/router";
@Component({
  selector: 'message-list',
  templateUrl: './message-list.component.html',
  styleUrls: ['./message-list.component.css']
})
export class CRMMessageListComponent implements OnInit {

  @Input() userId: number = 0;
  @Input() pageCount: number = 0;

  offset: number = 0;

  dataEmail: any;

  dataSMS: any;

  currentOffset: Array<number> = [0, 0];
  totalCount: Array<number> = [0, 0];
  constructor(private _userListService: UserListService, private _router: Router) { }
  ngOnInit(): void {                
      this.request(1, this.offset );
      this.request(2, this.offset );        
  }    

  request(type: number, offset: number ): void{
      if (this.userId == 0)
      {
          this._userListService.MessageList(type, offset, this.pageCount).subscribe(
              data => {
                  if (type == 1)
                  {
                      this.dataEmail = data;
                  }
                  else if (type == 2)
                  {
                      this.dataSMS = data;
                  }
                  this.totalCount[type - 1] = data.totalCount;
                  this.currentOffset[type-1] = data.offset;
              });   
      }
      else
      {
          this._userListService.MessageListByUser(type, this.userId).subscribe(
              data => {
                  if (type == 1) {
                      this.dataEmail = data;
                  }
                  else if (type == 2) {
                      this.dataSMS = data;
                  }
                  this.totalCount[type - 1] = data.totalCount;
                  this.currentOffset[type - 1] = data.offset;
              });  
      }
      
  }


  prev(type: number): void {
      var index = type - 1;
      var offset = this.currentOffset[index] - this.pageCount;
      if (offset >= 0  )
      {
          this.request(type, offset );
      }
  }

  next(type: number): void {
      var index = type - 1;
      var offset = this.currentOffset[index] + this.pageCount;
      if (offset < this.totalCount[index])
      {
          this.request(type, offset );
      }
  }

  ceil(value: number): number {
      return Math.ceil( value );
  }

}
