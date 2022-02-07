import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest, BehaviorSubject } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { M } from '@allors/default/workspace/meta';
import {
  InternalOrganisation,
  Locale,
  Person,
  Organisation,
  OrganisationContactRelationship,
  Currency,
  Enumeration,
  CustomerRelationship,
  Employment,
  PersonRole,
  OrganisationContactKind,
  GenderType,
  Salutation,
} from '@allors/default/workspace/domain';
import {
  NavigationService,
  ObjectData,
  RefreshService,
  ErrorService,
  SearchFactory,
  SingletonId,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { Filters } from '../../../filters/filters';

@Component({
  templateUrl: './person-create.component.html',
  providers: [ContextService],
})
export class PersonCreateComponent implements OnInit, OnDestroy {
  readonly m: M;

  public title = 'Add Person';

  internalOrganisation: InternalOrganisation;
  person: Person;
  organisation: Organisation;
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
  organisationsFilter: SearchFactory;

  private customerRole: PersonRole;
  private employeeRole: PersonRole;

  private subscription: Subscription;
  private readonly refresh$: BehaviorSubject<Date>;
  currencies: Currency[];

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<PersonCreateComponent>,
    public navigationService: NavigationService,
    public refreshService: RefreshService,
    private route: ActivatedRoute,
    private errorService: ErrorService,
    private fetcher: FetcherService,
    private singletonId: SingletonId,
    private internalOrganisationId: InternalOrganisationId
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
    this.refresh$ = new BehaviorSubject<Date>(undefined);
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(
      this.route.url,
      this.refresh$,
      this.internalOrganisationId.observable$
    )
      .pipe(
        switchMap(([,]) => {
          const pulls = [
            this.fetcher.internalOrganisation,
            pull.Singleton({
              objectId: this.singletonId.value,
              select: {
                Locales: {
                  include: {
                    Language: x,
                    Country: x,
                  },
                },
              },
            }),
            pull.Currency({
              predicate: {
                kind: 'Equals',
                propertyType: m.Currency.IsActive,
                value: true,
              },
              sorting: [{ roleType: m.Currency.Name }],
            }),
            pull.GenderType({
              predicate: {
                kind: 'Equals',
                propertyType: m.GenderType.IsActive,
                value: true,
              },
              sorting: [{ roleType: m.GenderType.Name }],
            }),
            pull.Salutation({
              predicate: {
                kind: 'Equals',
                propertyType: m.Salutation.IsActive,
                value: true,
              },
              sorting: [{ roleType: m.Salutation.Name }],
            }),
            pull.PersonRole({
              sorting: [{ roleType: m.PersonRole.Name }],
            }),
            pull.OrganisationContactKind({
              sorting: [{ roleType: m.OrganisationContactKind.Description }],
            }),
          ];

          this.organisationsFilter = Filters.organisationsFilter(m);

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.person = loaded.object<Person>(m.Person);
        this.internalOrganisation =
          this.fetcher.getInternalOrganisation(loaded);
        this.currencies = loaded.collection<Currency>(m.Currency);
        this.locales = loaded.collection<Locale>(m.Singleton.Locales) || [];
        this.genders = loaded.collection<GenderType>(m.GenderType);
        this.salutations = loaded.collection<Salutation>(m.Salutation);
        this.roles = loaded.collection<PersonRole>(m.PersonRole);
        this.organisationContactKinds =
          loaded.collection<OrganisationContactKind>(m.OrganisationContactKind);

        this.customerRole = this.roles?.find(
          (v: PersonRole) =>
            v.UniqueId === 'b29444ef-0950-4d6f-ab3e-9c6dc44c050f'
        );
        this.employeeRole = this.roles?.find(
          (v: PersonRole) =>
            v.UniqueId === 'db06a3e1-6146-4c18-a60d-dd10e19f7243'
        );

        this.person = this.allors.context.create<Person>(m.Person);
        this.person.CollectiveWorkEffortInvoice = false;
        this.person.PreferredCurrency =
          this.internalOrganisation.PreferredCurrency;
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    if (this.selectedRoles.indexOf(this.customerRole) > -1) {
      const customerRelationship =
        this.allors.context.create<CustomerRelationship>(
          this.m.CustomerRelationship
        );
      customerRelationship.Customer = this.person;
      customerRelationship.InternalOrganisation = this.internalOrganisation;
    }

    if (this.selectedRoles.indexOf(this.employeeRole) > -1) {
      const employment = this.allors.context.create<Employment>(
        this.m.Employment
      );
      employment.Employee = this.person;
      employment.Employer = this.internalOrganisation;
    }

    if (this.organisation != null) {
      const organisationContactRelationship =
        this.allors.context.create<OrganisationContactRelationship>(
          this.m.OrganisationContactRelationship
        );
      organisationContactRelationship.Contact = this.person;
      organisationContactRelationship.Organisation = this.organisation;
      organisationContactRelationship.ContactKinds = this.selectedContactKinds;
    }

    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.person);
      this.refreshService.refresh();
    }, this.errorService.errorHandler);
  }
}
