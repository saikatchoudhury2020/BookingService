import { Component, OnInit, Output, Input, EventEmitter, OnChanges } from '@angular/core';
import { FormGroup, FormBuilder, Validators, AbstractControl, ValidatorFn, FormArray, FormControl } from '@angular/forms';
import { OrderMode } from 'src/app/model/orderMode';
import { AppSettings } from 'src/app/appConfig';
import * as _ from 'lodash';
import { AppService } from 'src/app/app.service';
@Component({
  selector: 'app-order',
  templateUrl: './order.component.html',
  styleUrls: ['./order.component.css']
})
export class OrderComponent implements OnInit, OnChanges {
  @Input() parameters: any;
  @Input() isOpen: boolean = false;
  @Output() init = new EventEmitter<Array<any>>();
  @Output() saved = new EventEmitter<Array<any>>();
  @Output() closed = new EventEmitter<any>();
  @Output() submit = new EventEmitter<Array<any>>();
  @Output() deleted = new EventEmitter<Array<any>>();
  display = 'none';
  orderForm: FormGroup;
  mode: string = 'orderEdit';
  dialogTitle = '';
  buttonTitle = '';
  displayWarning = '';
  addonTotal: number = 0;
  addonCostTotal: number = 0;
  modeConfig: OrderMode = AppSettings.OrdersMode;
  orderlineArray: Array<any> = [];
  get OrderLines(): FormArray {
    return <FormArray>this.orderForm.get('OrderLines');
  }
  constructor(private fb: FormBuilder, private _orderService: AppService) { }

  ngOnInit() {

    this.orderForm = this.fb.group({
      OrderHeadId: 0,
      OrderLines: this.fb.array([this.buildOrderLine()]),
      SumTotal: '0',
      CostSumTotal: '0'
    });
    this.OnPriceQtyChange();
  }

  ngOnChanges(changes) {
    if (changes['isOpen']) {
      if (this.isOpen) {
        this.open(this.parameters);
      }
      else {
        this.close();
      }
    }

  }

  addOrderLine(): void {
    this.OrderLines.push(this.buildOrderLine());

  }
  buildOrderLine(): FormGroup {
    return this.fb.group({
      FoodId: 0,
      Qty: 1,
      ArticleId: '',

      MainServiceId: '',
      IsKitchenOrder: true,
      OrderHeadId: 0,
      OrderListId: 0,
      IsVatApply: false,
      Price: this.setTwoNumberDecimal(0),
      Sum: this.setTwoNumberDecimal(0),
      CostPrice: this.setTwoNumberDecimal(0),
      CostTotal: this.setTwoNumberDecimal(0),
      Tekst: '',
      Time: '',
      ServiceText: '',

    });
  }

  open(parameters: any) {


    this.display = 'block';
    var mode = parameters && parameters['mode'] ? parameters['mode'] : 'add';
    this.updateMode(mode);
    if (parameters) {


      this.orderlineArray = this.parameters.orderlines;



      const control = <FormArray>this.orderForm.controls['OrderLines'];
      for (let i = control.length - 1; i >= 0; i--) {
        control.removeAt(i);

      }
      if (this.orderlineArray) {

        for (var i = 0; i < this.orderlineArray.length; i++) {
          this.addOrderLine();
        }

        if (this.parameters.orderlines) {
          _.forEach(this.parameters.orderlines, (orderline): void => {
            let a = this.setTwoNumberDecimal(orderline.Price);
            let b = this.setTwoNumberDecimal(orderline.CostPrice);
            orderline.Price = this.setTwoNumberDecimal(orderline.Price);
            orderline.CostPrice = this.setTwoNumberDecimal(orderline.CostPrice);

          })
          this.orderForm.get('OrderHeadId').setValue(this.parameters.OrderHeadId);
          this.orderForm.get('OrderLines').setValue(this.parameters.orderlines);
        }

      }
      else {
        this.addOrderLine();
      }

    }
  }
  setTwoNumberDecimal(value: any): any {
    let a = parseFloat(value).toFixed(2);
    return (a);
  }
  close() {
    this.display = 'none';


    if (this.isOpen) {
      this.isOpen = false;

      // this.ResetForm(null);
      this.closed.emit();
    }

  }

  updateMode(mode: string) {
    this.mode = mode;

    if (this.mode == 'OrdersMode') {
      this.dialogTitle = "Oppdater order (" + this.parameters.OrderHeadId + ")";
      this.buttonTitle = 'Oppdater';
      this.modeConfig = AppSettings.OrdersMode;
    }
    else if (this.mode == "OrdersViewMode") {
      this.dialogTitle = "order (" + this.parameters.OrderHeadId + ")";
      this.buttonTitle = 'Oppdater';
      this.modeConfig = AppSettings.OrdersViewMode;
    }

  }
  totalValue() {
    this.addonTotal = 0;
    this.addonCostTotal = 0;
    let totalval = 0;
    let totalCostval = 0;
    const control = <FormArray>this.orderForm.controls['OrderLines'];
    const subcontrol = control.controls;

    for (let i = 0; i < subcontrol.length; i++) {

      var oh = <FormGroup>subcontrol[i];
      totalval += oh.value["Price"] * oh.value["Qty"];
      totalCostval += oh.value["CostPrice"] * oh.value["Qty"]
      this.addonTotal = this.setTwoNumberDecimal(totalval);
      this.addonCostTotal = this.setTwoNumberDecimal(totalCostval);
    }
  }

  OnPriceQtyChange(): void {
    this.orderForm.controls['OrderLines'].valueChanges.subscribe(val => {
      if (val.length > 0) {

        for (let i = 0; i < val.length; i++) {
          if (val[i].ArticleId) {


            val[i].Sum = this.setTwoNumberDecimal(val[i].Price * val[i].Qty);
            val[i].CostTotal = this.setTwoNumberDecimal(val[i].CostPrice * val[i].Qty);
            this.orderForm.controls['OrderLines'].patchValue(val, { onlySelf: true, emitEvent: false });
            this.totalValue();


          }



        }


      }

    });
  }

  save = (): void => {

    this._orderService.OrderBook(this.orderForm.value).subscribe(data => {
      if (data.errorType != 0) {
        this.displayWarning = data.data;
      }
      else {
        this.displayWarning = '';

        this.saved.emit(data);
      }

    }, error => { });


  }
}
