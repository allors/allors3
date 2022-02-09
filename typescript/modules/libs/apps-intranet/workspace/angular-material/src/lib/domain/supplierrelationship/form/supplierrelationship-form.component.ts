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
  SupplierRelationship,
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
  templateUrl: './supplierrelationship-form.component.html',
  providers: [ContextService],
})
export class SupplierRelationshipFormComponent
  extends AllorsFormComponent<SupplierRelationship>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  readonly m: M;

  partyRelationship: SupplierRelationship;
  internalOrganisation: InternalOrganisation;
  organisation: Organisation;
  title: string;
  canSave: boolean;

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private internalOrganisationId: InternalOrganisationId,
    private fetcher: FetcherService
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

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
              pull.SupplierRelationship({
                objectId: this.data.id,
                include: {
                  InternalOrganisation: x,
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

          this.title = 'Add Supplier Relationship';

          this.partyRelationship =
            this.allors.context.create<SupplierRelationship>(
              m.SupplierRelationship
            );
          this.partyRelationship.FromDate = new Date();
          this.partyRelationship.Supplier = this.organisation;
          this.partyRelationship.InternalOrganisation =
            this.internalOrganisation;
          this.partyRelationship.NeedsApproval = false;
        } else {
          this.partyRelationship = loaded.object<SupplierRelationship>(
            m.SupplierRelationship
          );

          if (this.partyRelationship.canWriteFromDate) {
            this.title = 'Edit Supplier Relationship';
          } else {
            this.title = 'View Supplier Relationship';
          }
        }
      });
  }
}
