import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult, IObject } from '@allors/system/workspace/domain';
import {
  InternalOrganisation,
  Party,
  Priority,
  SerialisedItem,
  WorkRequirement,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { Filters } from '../../../filters/filters';
import { RadioGroupOption } from '@allors/base/workspace/angular-material/foundation';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'workrequirement-edit-form',
  templateUrl: './workrequirement-edit-form.component.html',
  providers: [ContextService],
})
export class WorkRequirementEditFormComponent extends AllorsFormComponent<WorkRequirement> {
  readonly m: M;
  public title: string;

  internalOrganisation: InternalOrganisation;
  serialisedItems: SerialisedItem[];
  priorities: Priority[];
  priorityOptions: RadioGroupOption[];
  customer: Party;
  customersFilter: SearchFactory;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

    this.customersFilter = Filters.customersFilter(
      this.m,
      this.internalOrganisationId.value
    );
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      this.fetcher.internalOrganisation,
      p.WorkRequirement({
        objectId: this.editRequest.objectId,
        include: {
          FixedAsset: {},
          Originator: {},
          Priority: {},
          Pictures: {},
          LastModifiedBy: {},
        },
      }),
      p.Priority({
        predicate: {
          kind: 'Equals',
          propertyType: m.Priority.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.Priority.DisplayOrder }],
      })
    );

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.onPostPullInitialize(pullResult);

    this.internalOrganisation =
      this.fetcher.getInternalOrganisation(pullResult);
    this.priorities = pullResult.collection<Priority>(this.m.Priority);
    this.priorityOptions = this.priorities.map((v) => {
      return {
        label: v.Name,
        value: v,
      };
    });

    this.originatorSelected(this.object.Originator);
  }

  public originatorSelected(party: IObject) {
    if (party) {
      this.updateOriginator(party as Party);
    }
  }

  public override save(): void {
    this.onSave();
    super.save();
  }

  private updateOriginator(party: Party) {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.SerialisedItem({
        predicate: {
          kind: 'And',
          operands: [
            {
              kind: 'Or',
              operands: [
                {
                  kind: 'Equals',
                  propertyType: m.SerialisedItem.OwnedBy,
                  object: party,
                },
                {
                  kind: 'Equals',
                  propertyType: m.SerialisedItem.RentedBy,
                  object: party,
                },
              ],
            },
          ],
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      this.serialisedItems = loaded.collection<SerialisedItem>(
        m.SerialisedItem
      );
    });
  }

  private onSave() {
    this.object.Description = `Work Requirement for: ${this.object.FixedAsset?.DisplayName}`;
  }
}
