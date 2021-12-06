import { Component, OnInit, Self, OnDestroy, Inject } from '@angular/core';

import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { Organisation, Party, Priority, WorkRequirement, SerialisedItem, InternalOrganisation } from '@allors/workspace/domain/default';
import { ObjectData, PanelService, RadioGroupOption, RefreshService, SaveService, SearchFactory } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { FetcherService } from '../../../..';
import { Filters } from '../../../filters/filters';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'workrequirement-create',
  templateUrl: './workrequirement-create.component.html',
  providers: [PanelService, ContextService],
})
export class WorkRequirementCreateComponent implements OnInit, OnDestroy {
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
    public dialogRef: MatDialogRef<WorkRequirementCreateComponent>,

    public refreshService: RefreshService,
    private saveService: SaveService,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId,
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest([this.refreshService.refresh$, this.internalOrganisationId.observable$])
      .pipe(
        switchMap(([, internalOrganisationId]) => {
          const isCreate = this.data.id == null;

          const pulls = [
            this.fetcher.internalOrganisation,
            pull.Priority({
              predicate: { kind: 'Equals', propertyType: m.Priority.IsActive, value: true },
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

          if (this.data.associationId) {
            pulls.push(
              pull.SerialisedItem({
                objectId: this.data.associationId,
              }),
            );
          }

          this.customersFilter = Filters.customersFilter(m, internalOrganisationId);

          return this.allors.context.pull(pulls).pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();
        this.internalOrganisation = this.fetcher.getInternalOrganisation(loaded);
        this.priorities = loaded.collection<Priority>(m.Priority);
        this.priorityOptions = this.priorities.map((v) => {
          return {
            label: v.Name,
            value: v,
          };
        });

        if (isCreate) {
          this.title = 'Add Work Requirement';
          this.requirement = this.allors.context.create<WorkRequirement>(m.WorkRequirement);
          this.requirement.ServicedBy = this.internalOrganisation as Organisation;
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
    this.onSave()
    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.requirement);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
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
                  { kind: 'Equals', propertyType: m.SerialisedItem.OwnedBy, object: party },
                  { kind: 'Equals', propertyType: m.SerialisedItem.RentedBy, object: party },
                ],
              },
            ],
          },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      this.serialisedItems = loaded.collection<SerialisedItem>(m.SerialisedItem);
    });
  }

  private onSave() {
    this.requirement.Description = `Work Requirement for: ${this.requirement.FixedAsset?.DisplayName}`;
  }
}
