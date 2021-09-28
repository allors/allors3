import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest, BehaviorSubject } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { InternalOrganisation, Locale, Person, Organisation, OrganisationContactRelationship, Currency, Enumeration } from '@allors/workspace/domain/default';
import { NavigationService, ObjectData, RefreshService, SaveService, TestScope } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './person-create.component.html',
  providers: [SessionService]
})
export class PersonCreateComponent extends TestScope implements OnInit, OnDestroy {

  readonly m: M;

  public title = 'Add Person';

  internalOrganisation: InternalOrganisation;
  person: Person;
  organisation: Organisation;
  organisations: Organisation[];
  organisationContactRelationship: OrganisationContactRelationship;

  locales: Locale[];
  genders: Enumeration[];
  salutations: Enumeration[];
  organisationContactKinds: OrganisationContactKind[];
  selectedContactKinds: OrganisationContactKind[] = [];

  roles: PersonRole[];
  selectedRoles: PersonRole[] = [];
  customerRelationship: CustomerRelationship;
  employment: Employment;

  private customerRole: PersonRole;
  private employeeRole: PersonRole;

  private subscription: Subscription;
  private readonly refresh$: BehaviorSubject<Date>;
  currencies: Currency[];

  constructor(
    @Self() public allors: SessionService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<PersonCreateComponent>,
    
    public navigationService: NavigationService,
    public refreshService: RefreshService,
    private route: ActivatedRoute,
    private saveService: SaveService,
    private fetcher: FetcherService,
    private singletonId: SingletonId,
    private internalOrganisationId: InternalOrganisationId,
  ) {

    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;
    this.refresh$ = new BehaviorSubject<Date>(undefined);
  }

  public ngOnInit(): void {

    const m = this.allors.workspace.configuration.metaPopulation as M; const { pullBuilder: pull } = m; const x = {};

    this.subscription = combineLatest(this.route.url, this.refresh$, this.internalOrganisationId.observable$)
      .pipe(
        switchMap(([,]) => {

          const pulls = [
            this.fetcher.internalOrganisation,
            this.fetcher.locales,
            pull.Singleton({
              object: this.singletonId.value,
              select: {
                Locales: {
                  include: {
                    Language: x,
                    Country: x
                  }
                }
              }
            }),
            pull.Currency({
              predicate: { kind: 'Equals', propertyType: m.Currency.IsActive, value: true },
              sorting: [{ roleType: m.Currency.Name }],
            }),
            pull.GenderType({
              predicate: { kind: 'Equals', propertyType: m.GenderType.IsActive, value: true },
              sorting: [{ roleType: m.GenderType.Name }],
            }),
            pull.Salutation({
              predicate: { kind: 'Equals', propertyType: m.Salutation.IsActive, value: true },
              sorting: [{ roleType: m.Salutation.Name }],
            }),
            pull.PersonRole({
              sorting: [{ roleType: m.PersonRole.Name }]
            }),
            pull.OrganisationContactKind({
              sorting: [{ roleType: m.OrganisationContactKind.Description }],
            }),
            pull.Organisation({
              objectId: this.data.associationId,
            }),
            pull.Organisation({
              name: 'AllOrganisations',
              sorting: [{ roleType: m.Organisation.PartyName }]
            }),
          ];

          return this.allors.client.pullReactive(this.allors.session, pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.session.reset();

        this.person = loaded.object<Person>(m.Person);
        this.organisation = loaded.object<Organisation>(m.Organisation);
        this.organisations = loaded.collection<Organisation>(m.Organisation);
        this.internalOrganisation = loaded.object<InternalOrganisation>(m.InternalOrganisation);
        this.currencies = loaded.collection<Currency>(m.Currency);
        this.locales = loaded.collection<Locale>(m.Locale) || [];
        this.genders = loaded.collection<Enumeration>(m.Enumeration);
        this.salutations = loaded.collection<Enumeration>(m.Enumeration);
        this.roles = loaded.collection<PersonRole>(m.PersonRole);
        this.organisationContactKinds = loaded.collection<OrganisationContactKind>(m.OrganisationContactKind);

        this.customerRole = this.roles.find((v: PersonRole) => v.UniqueId === 'b29444ef-0950-4d6f-ab3e-9c6dc44c050f');
        this.employeeRole = this.roles.find((v: PersonRole) => v.UniqueId === 'db06a3e1-6146-4c18-a60d-dd10e19f7243');

        this.person = this.allors.session.create<Person>(m.Person);
        this.person.CollectiveWorkEffortInvoice = false;
        this.person.PreferredCurrency = this.internalOrganisation.PreferredCurrency;

      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {

    if (this.selectedRoles.indexOf(this.customerRole) > -1) {
      const customerRelationship = this.allors.session.create<CustomerRelationship>(m.CustomerRelationship);
      customerRelationship.Customer = this.person;
      customerRelationship.InternalOrganisation = this.internalOrganisation;
    }

    if (this.selectedRoles.indexOf(this.employeeRole) > -1) {
      const employment = this.allors.session.create<Employment>(m.Employment);
      employment.Employee = this.person;
      employment.Employer = this.internalOrganisation;
    }

    if (this.organisation !== undefined) {
      const organisationContactRelationship = this.allors.session.create<OrganisationContactRelationship>(m.OrganisationContactRelationship);
      organisationContactRelationship.Contact = this.person;
      organisationContactRelationship.Organisation = this.organisation;
      organisationContactRelationship.ContactKinds = this.selectedContactKinds;
    }

    this.allors.context
      .save()
      .subscribe(() => {
        const data: IObject = {
          id: this.person.id,
          objectType: this.person.objectType,
        };

        this.dialogRef.close(data);
        this.refreshService.refresh();
      },
        this.saveService.errorHandler
      );
  }
}
