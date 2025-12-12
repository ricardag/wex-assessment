import {Component, ElementRef, OnInit, ViewChild} from '@angular/core';
import {Purchase, PurchaseData, PurchaseFilter, PurchaseQueryResult} from '../../interfaces/purchases';
import {CurrencyPipe, DatePipe, DecimalPipe, JsonPipe, NgClass} from '@angular/common';
import {RestApiService} from '../../services/rest-api-service';
import {PaginationModule} from 'ngx-bootstrap/pagination';
import {FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import {CollapseModule} from 'ngx-bootstrap/collapse';
import {BsDatepickerModule} from 'ngx-bootstrap/datepicker';
import {NgxMaskDirective} from 'ngx-mask';
import {dateValidator, greaterThanZero} from '../../validators/validators';
import {CountryCurrency} from '../../interfaces/country-currency';
import {ExchangeRate} from '../../interfaces/exchange-rate';
import {AlertModule} from 'ngx-bootstrap/alert';

export interface AlertMessage {
  type: string;
  message: string;
}

@Component({
  selector: 'app-menu',
  imports: [
    DatePipe,
    CurrencyPipe,
    PaginationModule,
    FormsModule,
    ReactiveFormsModule,
    CollapseModule,
    BsDatepickerModule,
    NgxMaskDirective,
    DecimalPipe,
    NgClass,
    AlertModule,
  ],
  templateUrl: './menu.html',
  styleUrl: './menu.scss',
})
export class Menu implements OnInit {
  @ViewChild('modalDialog') dialogElement!: ElementRef<HTMLDialogElement>;
  @ViewChild('convertDialog') convertDialogElement!: ElementRef<HTMLDialogElement>;

  public data: PurchaseQueryResult | null = null;
  public filter: PurchaseFilter | null = null;
  public pageSize: number = 20;
  public currentPage: number = 1;

  public isFiltersCollapsed: boolean = true;
  public filterForm: FormGroup;

  public purchaseId: number = 0;
  public purchaseForm: FormGroup;

  public countriesCurrencies: CountryCurrency[] = [];
  public convertingItem: Purchase | null  = null;
  public idCountryCurrency: number = 0;
  public countryCurrencyToConvert: CountryCurrency | undefined = undefined;
  public exchangeRate: ExchangeRate | null = null;
  public daysSince: number | null = null;

  public alerts: AlertMessage[] = [];

  constructor(
    private restApiService: RestApiService,
    private fb: FormBuilder
  ) {
    this.filterForm = this.fb.group({
      description: [null],
      transactionStartDate: [null],
      transactionEndDate: [null],
      minAmount: [null],
      maxAmount: [null]
    });

    this.purchaseForm = this.fb.group({
      description: [null, Validators.required],
      purchaseAmount: [null, [Validators.required, greaterThanZero()]],
      transactionDateUtc: [null, Validators.required],
    });
  }

  public async ngOnInit() {

    this.filter = {
      description: null,
      transactionStartDate: null,
      transactionEndDate: null,
      minAmount: null,
      maxAmount: null,
      start: 0,
      pageSize: this.pageSize
    }

    // No await, do it asynchronously
    this.paginate(1);

    // No await, do it asynchronously
    this.restApiService.getCountryCurrencyList()
      .then(countryCurrencyList => {
        this.countriesCurrencies = countryCurrencyList;
      });
  }

  protected async paginate(page: number) {
    if (this.filter) {
      this.currentPage = page;
      this.filter.start = (page - 1) * this.pageSize;
      this.filter.pageSize = this.pageSize;

      try {
        this.data = await this.restApiService.paginate(this.filter);
      }
      catch (error:any) {
        console.error(error);
        this.addAlert('danger', `There was an error while processing the request: ${error.message}`);
      }
    }
  }

  protected async applyFilters() {
    if (this.filter) {
      const formValue = this.filterForm.value;

      this.filter.description = formValue.description;
      this.filter.transactionStartDate = formValue.transactionStartDate;
      this.filter.transactionEndDate = formValue.transactionEndDate;
      this.filter.minAmount = formValue.minAmount;
      this.filter.maxAmount = formValue.maxAmount;

      await this.paginate(1);
    }
  }

  protected async clearFilters() {
    this.filterForm.reset();
    await this.applyFilters();
  }

  protected async newTransaction() {
    if (this.isAnyDialogOpen()) {
      return;
    }

    this.purchaseForm.get('description')?.setValue(null);
    this.purchaseForm.get('purchaseAmount')?.setValue(null);
    this.purchaseForm.get('transactionDateUtc')?.setValue(new Date());

    this.purchaseId = 0;  // new purchase
    this.dialogElement.nativeElement.show();
  }

  protected closeDialog() {
    if (this.dialogElement && this.dialogElement.nativeElement) {
      this.dialogElement.nativeElement.close();
    }
  }

  protected async applyDialog() {
    if (this.dialogElement && this.dialogElement.nativeElement) {
      if (!this.purchaseForm.valid) {
        return;
      }

      const data: PurchaseData = {
        description: this.purchaseForm.get('description')?.value,
        purchaseAmount: this.purchaseForm.get('purchaseAmount')?.value,
        transactionDateUtc: this.purchaseForm.get('transactionDateUtc')?.value,
      }

      try {
        if (!this.purchaseId) {
          await this.restApiService.newPurchase(data);
        } else {
          await this.restApiService.editPurchase(this.purchaseId, data);
        }
        this.dialogElement.nativeElement.close();

        await this.paginate(this.currentPage);  // refresh table
      } catch (error: any) {
        console.error(error);
        this.addAlert('danger', `There was an error while processing the request: ${error.message}`);
      }
    }
  }

  protected editTransaction(item: Purchase) {
    if (this.isAnyDialogOpen()) {
      return;
    }

    this.purchaseId = item.id;
    this.purchaseForm.get('description')?.setValue(item.description);
    this.purchaseForm.get('purchaseAmount')?.setValue(item.purchaseAmount);
    this.purchaseForm.get('transactionDateUtc')?.setValue(new Date(item.transactionDatetimeUtc));
    this.dialogElement.nativeElement.show();
  }

  protected async deleteTransaction(id: number) {
    if (this.isAnyDialogOpen()) {
      return;
    }

    try {
      await this.restApiService.deletePurchase(id);
      await this.paginate(this.currentPage);  // refresh table
    } catch (error: any) {
      console.error(error);
      this.addAlert('danger', `There was an error while processing the request: ${error.message}`);
    }
  }

  protected async convertCurrency(item: Purchase) {
    if (this.isAnyDialogOpen()) {
      return;
    }

    this.convertingItem = item;
    this.idCountryCurrency = 0;
    this.countryCurrencyToConvert = undefined;
    this.daysSince = null;
    this.exchangeRate = null;

    this.convertDialogElement.nativeElement.show();
  }

  protected closeConvertDialog() {
    this.convertDialogElement.nativeElement.close();
  }

  protected async getExchangeRate() {
    this.countryCurrencyToConvert = this.countriesCurrencies.find(c => c.id == this.idCountryCurrency);
    this.daysSince = null;
    this.exchangeRate = null;

    if (this.countryCurrencyToConvert) {
      try {
        this.exchangeRate = await this.restApiService.getExchangeRate(this.countryCurrencyToConvert?.country, this.countryCurrencyToConvert?.currency, this.convertingItem?.transactionDatetimeUtc);
        this.daysSince = this.getDaysSince(this.exchangeRate.recordDate, this.convertingItem?.transactionDatetimeUtc ?? new Date());
        if (this.daysSince > 180) {
          this.addAlert('warning', 'The purchase cannot be converted to the target currency.');
        }
      }
      catch (error:any) {
        console.error(error);
        this.addAlert('danger', `There was an error while processing the request: ${error.message}`);
      }
    }
  }

  private getDaysSince(date: string | Date, purchaseDate: string | Date): number {
    const recordDate = new Date(date);
    const refDate = new Date(purchaseDate);
    const diffTime = refDate.getTime() - recordDate.getTime();
    return Math.floor(diffTime / (1000 * 60 * 60 * 24));
  }

  protected addAlert(type: string, message: string): void {
    const alert: AlertMessage = { type, message };
    this.alerts.push(alert);

    setTimeout(() => {
      this.closeAlert(alert);
    }, 5000);
  }

  protected closeAlert(alert: AlertMessage): void {
    const index = this.alerts.indexOf(alert);
    if (index !== -1) {
      this.alerts.splice(index, 1);
    }
  }

  private isAnyDialogOpen(): boolean {
    return this.dialogElement?.nativeElement?.open || this.convertDialogElement?.nativeElement?.open;
  }
}
