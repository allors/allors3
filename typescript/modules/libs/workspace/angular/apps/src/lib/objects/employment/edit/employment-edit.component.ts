import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { Person, Organisation, Party, InternalOrganisation } from '@allors/workspace/domain/default';
import { ObjectData, RefreshService, SaveService, TestScope } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  templateUrl: './employment-edit.component.html',
  providers: [SessionService],
})
export class EmploymentEditComponent extends TestScope implements OnInit, OnDestroy {
  readonly m: M;

  partyRelationship: Employment;
  people: Person[];
  party: Party;
  person: Person;
  organisation: Organisation;
  internalOrganisation: Organisation;
  internalOrganisations: Organisation[];
  title: string;
  addEmployee = false;

  private subscription: Subscription;

  constructor(
    @Self() public allors: SessionService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<EmploymentEditComponent>,

    public refreshService: RefreshService,
    private saveService: SaveService,
    private internalOrganisationId: InternalOrganisationId,
    private fetcher: FetcherService
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  static canCreate(createData: ObjectData, context: Context) {
    const organisationId = ids.Organisation;
    if (createData.associationObjectType.id === organisationId) {
      const organisation = context.session.get(createData.associationId) as Organisation;
      return organisation.IsInternalOrganisation;
    }

    return true;
  }

  public ngOnInit(): void {
    const { pull, x, m } = this.metaService;

    this.subscription = combineLatest(this.refreshService.refresh$, this.internalOrganisationId.observable$)
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id === undefined;

          const pulls = [this.fetcher.internalOrganisation, pull.Person()];

          if (!isCreate) {
            pulls.push(
              pull.Employment({
                objectId: this.data.id,
                include: {
                  Employee: x,
                  Employer: x,
                  Parties: x,
                },
              })
            );
          }

          if (isCreate && this.data.associationId) {
            pulls.push(
              pull.Party({
                objectId: this.data.associationId,
              })
            );
          }

          return this.allors.client.pullReactive(this.allors.session, pulls).pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.session.reset();

        this.people = loaded.collection<Person>(m.Person);
        this.internalOrganisation = loaded.object<InternalOrganisation>(m.InternalOrganisation);

        if (isCreate) {
          this.title = 'Add Employment';

          this.partyRelationship = this.allors.session.create<Employment>(m.Employment);
          this.partyRelationship.FromDate = new Date();
          this.partyRelationship.Employer = this.internalOrganisation;

          this.party = loaded.object<Party>(m.Party);

          if (this.party.objectType.name === m.Person.name) {
            this.person = this.party as Person;
            this.partyRelationship.Employee = this.person;
          }

          if (this.party.objectType.name === m.Organisation.name) {
            this.organisation = this.party as Organisation;

            if (!this.organisation.IsInternalOrganisation) {
              this.dialogRef.close();
            }
          }
        } else {
          this.partyRelationship = loaded.object<Employment>(m.Employment);
          this.person = this.partyRelationship.Employee;
          this.organisation = this.partyRelationship.Employer as Organisation;

          if (this.partyRelationship.canWriteFromDate) {
            this.title = 'Edit Employment';
          } else {
            this.title = 'View Employment';
          }
        }
      });
  }

  public employeeAdded(employee: Person): void {
    this.partyRelationship.Employee = employee;
    this.people.push(employee);
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.client.pushReactive(this.allors.session).subscribe(() => {
      const data: IObject = {
        id: this.partyRelationship.id,
        objectType: this.partyRelationship.objectType,
      };

      this.dialogRef.close(data);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
