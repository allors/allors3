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
import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  templateUrl: './subcontractorrelationship-form.component.html',
  providers: [ContextService],
})
export class SubContractorRelationshipFormComponent
  implements OnInit, OnDestroy
{
  readonly m: M;

  partyRelationship: SubContractorRelationship;
  internalOrganisation: InternalOrganisation;
  organisation: Organisation;
  title: string;

  private subscription: Subscription;
  canSave: boolean;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<SubContractorRelationshipFormComponent>,
    public refreshService: RefreshService,
    private errorService: ErrorService,
    private internalOrganisationId: InternalOrganisationId,
    private fetcher: FetcherService
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;

    this.canSave = true;
  }

  public canCreate(createData: ObjectData) {
    if (createData.associationObjectType === this.m.Person) {
      return false;
    }

    return true;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(
      this.refreshService.refresh$,
      this.internalOrganisationId.observable$
    )
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id == null;

          const pulls = [this.fetcher.internalOrganisation];

          if (!isCreate) {
            pulls.push(
              pull.SubContractorRelationship({
                objectId: this.data.id,
                include: {
                  Contractor: x,
                  SubContractor: x,
                  Parties: x,
                },
              })
            );
          }

          if (isCreate && this.data.associationId) {
            pulls.push(
              pull.Organisation({
                objectId: this.data.associationId,
              })
            );
          }

          return this.allors.context
            .pull(pulls)
            .pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();

        this.internalOrganisation =
          this.fetcher.getInternalOrganisation(loaded);
        this.organisation = loaded.object<Organisation>(m.Organisation);

        if (isCreate) {
          if (this.organisation == null) {
            this.canSave = false;
            // this.dialogRef.close();
          }

          this.title = 'Add SubContractor Relationship';

          this.partyRelationship =
            this.allors.context.create<SubContractorRelationship>(
              m.SubContractorRelationship
            );
          this.partyRelationship.FromDate = new Date();
          this.partyRelationship.SubContractor = this.organisation;
          this.partyRelationship.Contractor = this.internalOrganisation;
        } else {
          this.partyRelationship = loaded.object<SubContractorRelationship>(
            m.SubContractorRelationship
          );

          if (this.partyRelationship.canWriteFromDate) {
            this.title = 'Edit SubContractor Relationship';
          } else {
            this.title = 'View SubContractor Relationship';
          }
        }
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.partyRelationship);
      this.refreshService.refresh();
    }, this.errorService.errorHandler);
  }
}
