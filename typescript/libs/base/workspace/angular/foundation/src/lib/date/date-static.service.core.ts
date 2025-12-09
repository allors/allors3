import { Injectable } from '@angular/core';

import { DateConfig } from './date.config';
import { DateService } from './date.service';

@Injectable()
export class DateStaticService extends DateService {
  public locale: Locale;

  constructor(dateConfig: DateConfig) {
    super();
    this.locale = dateConfig.locale;
  }
}
