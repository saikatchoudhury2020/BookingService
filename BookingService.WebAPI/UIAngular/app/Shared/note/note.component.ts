import { Component, Input, OnInit, OnChanges, EventEmitter, Output } from "@angular/core";
import { FormBuilder, FormArray, FormGroup, FormControl, AbstractControl } from "@angular/forms";
import { Router } from "@angular/router";
import { NoteService } from './note.service';
import { confirmAlertService } from '../confirmationAlert/service/confirmAlert.service';
declare var jquery: any;
declare var $: any;

@Component({
  selector: 'app-note',
  templateUrl: './note.component.html',
  styleUrls: ['./note.component.css']
})
export class NoteComponent implements OnInit {
  @Input() userid: number;
  @Input() orgId: number;
  noteListData: any;
  messageSubject: any;
  Id: string;
  messageDisplay = 'none';
  messageBody: string;

  sending: boolean = false;
  errorMessage: string;

  constructor(private _noteListService: NoteService, private _router: Router, private _confirmService: confirmAlertService) { }

  ngOnInit(): void {
    this.onGetUserGroup();
  }
  ngOnChanges(): void {
    this.onGetUserGroup();
  }
  onGetUserGroup() {
    if (!this.userid) {
      this.userid = 0;
    }
    if (!this.orgId) {
      this.orgId = 0;
    }
    this._noteListService.GetNoteDisplay(this.orgId, this.userid).subscribe(data => {
      this.noteListData = data;
      console.log(this.noteListData);
    });
  }

  DeleteNote(Id: string): void {
    this.Id = Id;
    this._confirmService.toggle();
  }
  ConfirmCommand(value: any): void {
    if (value.val) {
      this._noteListService.DeleteNote(this.Id).subscribe(data => {

        this._confirmService.toggle();
        this.onGetUserGroup();
      });

    }

  }


  sendMessageDialog(type: string) {
    this.messageBody = '';
    this.messageDisplay = 'block';
  }
  onCloseMessage() {
    this.messageDisplay = 'none';
  }

  onSendMessage() {
    this.sending = true;

    this._noteListService.AddNote(this.orgId, this.userid, this.messageBody).subscribe(data => {
      if (data == "1") {
        this.sending = false;
        this.onCloseMessage();
        this.onGetUserGroup();
      }
    }, error => {
      this.sending = false;
      this.errorMessage = error;
    });

  }

}
