import { Component, OnInit, Input, Output, EventEmitter, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { FormGroup } from '@angular/forms';

@Component({
  selector: 'dm-select',
  templateUrl: './auto-select.component.html',
  styleUrls: ['./auto-select.component.css']
})
export class AutoSelectComponent implements OnInit {

  @Input() parentForm: FormGroup;
  @Input() data: any=[];
  @Input() label:any;
  @Input() placeholder:any;
  @Input() disabled:any;
  @Input() value:any;
  @Input() control:any;
  @Output() search: EventEmitter<any> = new EventEmitter<any>();
  @Output() scroll: EventEmitter<any> = new EventEmitter<any>();
  @Output() select: EventEmitter<any> = new EventEmitter<any>();
  @Output() clear:  EventEmitter<any> = new EventEmitter<any>();
  @ViewChild('scrollMe') private myScrollContainer: ElementRef;
  display='none';
  
  constructor() { }

  ngOnInit() {


    

 
  }

  GetDetail=(data:any)=>{
    this.parentForm.get(this.control).setValue(data.navn);
    this.data=[];
    this.select.emit(data);
    
  }
  ClearSearch=()=>{

    this.data=[];
    this.clear.emit(true);
   }

  onCustomerChange=(val:any)=>{
    if(val){
      if(val.length>0){
          this.display='block';
      }
      else{
        this.data=[];
        this.display='none';
      }
        this.search.emit(val);
      }
      else{
        this.data=[];
        this.display='none';
      }
  }
  scrollHandler=(data:any)=>{
   
     if( this.myScrollContainer.nativeElement.scrollTop +   this.myScrollContainer.nativeElement.offsetHeight>=this.myScrollContainer.nativeElement.scrollHeight){
      let searchData ={allow:true,search:this.parentForm.get(this.control).value};
      this.scroll.emit(searchData);
     }
  
  }

}
