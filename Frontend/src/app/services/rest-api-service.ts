import {Injectable} from '@angular/core';
import {environment} from '../../environments/environment';
import {HttpClient, HttpHeaders, HttpParams} from "@angular/common/http";
import {firstValueFrom} from 'rxjs';
import {Purchase, PurchaseData, PurchaseFilter, PurchaseQueryResult} from '../interfaces/purchases';
import {CountryCurrency} from '../interfaces/country-currency';
import {ExchangeRate} from '../interfaces/exchange-rate';


@Injectable({
  providedIn: 'root',
})
export class RestApiService {

  constructor(private http: HttpClient) {
  }

  public async login(username: string, password: string) {
    const url = `${environment.apiUrl}/auth`;
    return firstValueFrom(this.http.post<string>(url, {username, password}));
  }

  public async paginate(filter: PurchaseFilter) {
    const cleaned: any = {};
    for (const [k, v] of Object.entries(filter)) {
      if (v === null || v === undefined) continue;

      if (v instanceof Date) {
        cleaned[k] = v.toISOString();
      } else {
        cleaned[k] = v;
      }
    }

    const params = new HttpParams({fromObject: cleaned});
    const url = `${environment.apiUrl}/purchases`;
    return firstValueFrom(this.http.get<PurchaseQueryResult>(url, {params}));
  }

  public async newPurchase(purchase: PurchaseData) {
    const url = `${environment.apiUrl}/purchases`;
    return firstValueFrom(this.http.post<Purchase>(url, purchase));
  }

  public async editPurchase(purchaseId: number, purchase: PurchaseData) {
    const url = `${environment.apiUrl}/purchases/${purchaseId}`;
    return firstValueFrom(this.http.put<void>(url, purchase));
  }

  public async deletePurchase(purchaseId: number) {
    const url = `${environment.apiUrl}/purchases/${purchaseId}`;
    return firstValueFrom(this.http.delete<void>(url));
  }

  public async getCountryCurrencyList() {
    const url = `${environment.apiUrl}/country-currencies`;
    return firstValueFrom(this.http.get<CountryCurrency[]>(url));
  }

  public async getExchangeRate(country: string | undefined, currency: string | undefined, date: Date | undefined) {
    const formattedDate = date ? new Date(date).toISOString().split('T')[0] : '';
    const escapedCountry = encodeURIComponent(country ?? '');
    const escapedCurrency = encodeURIComponent(currency ?? '');

    const url = `${environment.apiUrl}/country-currencies/${escapedCountry}/${escapedCurrency}/${formattedDate}`;
    return firstValueFrom(this.http.get<ExchangeRate>(url));
  }
}
