import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  AllorsFormComponent,
  ErrorService,
} from '@allors/base/workspace/angular/foundation';
import { Locale, IrpfRegime } from '@allors/default/workspace/domain';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  selector: 'irpfregime-create',
  templateUrl: './irpfregime-create-form.component.html',
  providers: [ContextService],
})
export class IrpfRegimeCreateFormComponent extends AllorsFormComponent<IrpfRegime> {
  readonly m: M;
  locales: Locale[];

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(this.fetcher.locales);

    const initializer = this.createRequest?.initializer;
    if (initializer) {
      pulls.push();
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.context.create(this.createRequest.objectType);

    this.object.IsActive = true;

    this.locales = this.fetcher.getAdditionalLocales(pullResult);
  }

  public override save(): void {
    super.save();
  }
}
