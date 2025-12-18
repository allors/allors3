import {
  Component,
  Output,
  EventEmitter,
  OnInit,
  OnDestroy,
} from '@angular/core';

import { M } from '@allors/default/workspace/meta';
import { PostalAddress, Country } from '@allors/default/workspace/domain';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
  selector: 'postaladdress-inline',
  templateUrl: './postaladdress-inline.component.html',
})
export class PostalAddressInlineComponent implements OnInit, OnDestroy {
  @Output()
  public saved: EventEmitter<PostalAddress> = new EventEmitter<PostalAddress>();

  @Output()
  public cancelled: EventEmitter<any> = new EventEmitter();

  public countries: Country[];
  public postalAddress: PostalAddress;

  public m: M;

  constructor(private allors: ContextService) {
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    const pulls = [
      pull.Country({
        sorting: [{ roleType: this.m.Country.Name }],
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      this.countries = loaded.collection<Country>(m.Country);
      this.postalAddress = this.allors.context.create<PostalAddress>(
        m.PostalAddress
      );
    });
  }

  public ngOnDestroy(): void {
    if (this.postalAddress) {
      this.postalAddress.strategy.delete();
      this.postalAddress.strategy.delete();
    }
  }

  public cancel(): void {
    this.cancelled.emit();
  }

  public save(): void {
    this.saved.emit(this.postalAddress);

    this.postalAddress = undefined;
  }
}
