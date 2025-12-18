import {
  Component,
  Output,
  EventEmitter,
  OnInit,
  OnDestroy,
} from '@angular/core';

import { M } from '@allors/default/workspace/meta';
import { Bank, Country } from '@allors/default/workspace/domain';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  selector: 'bank-inline',
  templateUrl: './bank-inline.component.html',
})
export class BankInlineComponent implements OnInit, OnDestroy {
  @Output() public saved: EventEmitter<Bank> = new EventEmitter<Bank>();

  @Output() public cancelled: EventEmitter<any> = new EventEmitter();

  public m: M;

  public bank: Bank;
  banks: Bank[];
  countries: Country[];

  constructor(
    private allors: ContextService,

    private fetcher: FetcherService
  ) {
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    const pulls = [
      pull.Bank({}),
      pull.Country({
        sorting: [{ roleType: m.Country.Name }],
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      this.banks = loaded.collection<Bank>(m.Bank);
      this.countries = loaded.collection<Country>(this.m.Country);

      this.bank = this.allors.context.create<Bank>(m.Bank);
    });
  }

  public ngOnDestroy(): void {
    if (this.bank) {
      this.bank.strategy.delete();
    }
  }

  public cancel(): void {
    this.cancelled.emit();
  }

  public save(): void {
    this.saved.emit(this.bank);
    this.bank = undefined;
  }
}
