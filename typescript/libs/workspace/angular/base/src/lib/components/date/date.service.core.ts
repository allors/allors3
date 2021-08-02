import { Injectable } from '@angular/core';

import { DateService } from '../../services/date/date.service';

import { DateConfig } from './date.config';

@Injectable()
export class DateServiceCore extends DateService {
  public locale: Locale;

  constructor(dateConfig: DateConfig) {
    super();
    this.locale = dateConfig.locale;
  }
}
