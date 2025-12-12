import {AbstractControl, ValidationErrors, ValidatorFn} from '@angular/forms';

export function greaterThanZero(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;
    return value > 0 ? null : { greaterThanZero: true };
  };
}

export function greaterEqualZero(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;
    return value >= 0 ? null : { greaterThanZero: true };
  };
}

export function dateValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value?.trim();
    if (!value) {
      return null;  // aceita data vazia
    }

    // aceita dd/mm/aaaa ou dd/mm/aa
    const regex = /^(0?[1-9]|[12]\d|3[01])\/(0?[1-9]|1[0-2])\/(\d{2}|\d{4})$/;
    if (!regex.test(value)) {
      return {invalidDate: true};
    }

    // valida data real
    const [dia, mes, anoStr] = value.split('/');
    const ano = anoStr.length === 2 ? Number('20' + anoStr) : Number(anoStr);
    const data = new Date(ano, Number(mes) - 1, Number(dia));

    const dataValida =
      data.getFullYear() === ano &&
      data.getMonth() === Number(mes) - 1 &&
      data.getDate() === Number(dia);

    return dataValida ? null : { invalidDate: true };
  };
}

export function isHttps(httpsOnly:boolean): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;
    if (httpsOnly)
      return !value || /^https:\/\//.test(value) ? null : { isHttps: true };
    return !value || /^https?:\/\//.test(value) ? null : { isHttps: true };
  };
}

export function isGifJpeg(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;
    return !value || /\.(gif|jpg|jpeg)$/.test(value) ? null : { isGifJPeg: true };
  };
}

export function isDiretorio(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;
    return !value || /^[a-z0-9]+$/.test(value) ? null : { isDiretorio: true };
  };
}
