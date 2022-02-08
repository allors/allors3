import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import {
  EditIncludeHandler,
  Node,
  CreateOrEditPullHandler,
  Pull,
  IPullResult,
  PostCreatePullHandler,
} from '@allors/system/workspace/domain';
import {
  BasePrice,
  InternalOrganisation,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { FetcherService } from '../../../..';
import { Filters } from '../../../filters/filters';

@Component({
  selector: 'workrequirement-create',
  templateUrl: './workrequirement-create-form.component.html',
  providers: [OldPanelService, ContextService],
})
export class WorkRequirementCreateFormComponent implements OnInit, OnDestroy {
  readonly m: M;
  public title: string;

  internalOrganisation: InternalOrganisation;
  workTask: WorkRequirement;
  serialisedItems: SerialisedItem[];
  private subscription: Subscription;
  requirement: WorkRequirement;
  priorities: Priority[];
  priorityOptions: RadioGroupOption[];
  customer: Party;
  customersFilter: SearchFactory;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<WorkRequirementCreateFormComponent>,

    public refreshService: RefreshService,
    private errorService: ErrorService,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest([
      this.refreshService.refresh$,
      this.internalOrganisationId.observable$,
    ])
      .pipe(
        switchMap(([, internalOrganisationId]) => {
          const isCreate = this.data.id == null;

          const pulls = [
            this.fetcher.internalOrganisation,
            pull.Priority({
              predicate: {
                kind: 'Equals',
                propertyType: m.Priority.IsActive,
                value: true,
              },
              sorting: [{ roleType: m.Priority.DisplayOrder }],
            }),
          ];

          if (!isCreate) {
            pulls.push(
              pull.WorkRequirement({
                objectId: this.data.id,
                include: {
                  Originator: x,
                  Priority: x,
                  FixedAsset: x,
                  Pictures: x,
                },
              })
            );
          }

          if (isCreate && this.data.associationId) {
            pulls.push(
              pull.SerialisedItem({
                objectId: this.data.associationId,
                include: {
                  OwnedBy: x,
                  RentedBy: x,
                },
              })
            );
          }

          this.customersFilter = Filters.customersFilter(
            m,
            internalOrganisationId
          );

          return this.allors.context
            .pull(pulls)
            .pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();
        this.internalOrganisation =
          this.fetcher.getInternalOrganisation(loaded);
        this.priorities = loaded.collection<Priority>(m.Priority);
        this.priorityOptions = this.priorities.map((v) => {
          return {
            label: v.Name,
            value: v,
          };
        });

        if (isCreate) {
          this.title = 'Add Work Requirement';
          this.requirement = this.allors.context.create<WorkRequirement>(
            m.WorkRequirement
          );
          this.requirement.ServicedBy = this
            .internalOrganisation as Organisation;

          const serialisedItem = loaded.object<SerialisedItem>(
            m.SerialisedItem
          );
          if (serialisedItem !== undefined) {
            if (
              serialisedItem.OwnedBy != null &&
              !(<Organisation>serialisedItem.OwnedBy).IsInternalOrganisation
            ) {
              this.requirement.Originator = serialisedItem.OwnedBy;
              this.updateOriginator(this.requirement.Originator);
            } else if (
              serialisedItem.RentedBy != null &&
              !(<Organisation>serialisedItem.RentedBy).IsInternalOrganisation
            ) {
              this.requirement.Originator = serialisedItem.RentedBy;
              this.updateOriginator(this.requirement.Originator);
            }

            this.requirement.FixedAsset = serialisedItem;
          }
        } else {
          this.requirement = loaded.object<WorkRequirement>(m.WorkRequirement);
          this.originatorSelected(this.requirement.Originator);

          if (this.requirement.canWriteDescription) {
            this.title = 'Edit Work Requirement';
          } else {
            this.title = 'View Work Requirement';
          }
        }
      });
  }

  public originatorSelected(party: IObject) {
    if (party) {
      this.updateOriginator(party as Party);
    }
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.onSave();
    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.requirement);
      this.refreshService.refresh();
    }, this.errorService.errorHandler);
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
    this.requirement.Description = `Work Requirement for: ${this.requirement.FixedAsset?.DisplayName}`;
  }
}
