import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest, BehaviorSubject } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { InternalOrganisation, Locale, Organisation, Currency, CustomOrganisationClassification, IndustryClassification, LegalForm, CustomerRelationship, SupplierRelationship, OrganisationRole } from '@allors/workspace/domain/default';
import { ObjectData, RefreshService, SaveService, TestScope, SingletonId } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  templateUrl: './organisation-create.component.html',
  providers: [ContextService],
})
export class OrganisationCreateComponent extends TestScope implements OnInit, OnDestroy {
  public m: M;

  public title = 'Add Organisation';

  public organisation: Organisation;

  public locales: Locale[];
  public classifications: CustomOrganisationClassification[];
  public industries: IndustryClassification[];

  public customerRelationship: CustomerRelationship;
  public supplierRelationship: SupplierRelationship;
  public internalOrganisation: InternalOrganisation;
  public roles: OrganisationRole[];
  public selectableRoles: OrganisationRole[] = [];
  public activeRoles: OrganisationRole[] = [];
  private customerRole: OrganisationRole;
  private supplierRole: OrganisationRole;
  private manufacturerRole: OrganisationRole;

  private refresh$: BehaviorSubject<Date>;
  private subscription: Subscription;

  legalForms: LegalForm[];
  currencies: Currency[];

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<OrganisationCreateComponent>,

    public refreshService: RefreshService,
    private saveService: SaveService,
    private route: ActivatedRoute,
    private fetcher: FetcherService,
    private singletonId: SingletonId,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super();

    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
    this.refresh$ = new BehaviorSubject<Date>(undefined);
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(this.route.url, this.refresh$, this.internalOrganisationId.observable$)
      .pipe(
        switchMap(([, , internalOrganisationId]) => {
          const id: string = this.route.snapshot.paramMap.get('id');

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
            pull.OrganisationRole({}),
            pull.Currency({
              predicate: { kind: 'Equals', propertyType: m.Currency.IsActive, value: true },
              sorting: [{ roleType: m.Currency.Name }],
            }),
            pull.CustomOrganisationClassification({
              sorting: [{ roleType: m.CustomOrganisationClassification.Name }],
            }),
            pull.IndustryClassification({
              sorting: [{ roleType: m.IndustryClassification.Name }],
            }),
            pull.LegalForm({
              sorting: [{ roleType: m.LegalForm.Description }],
            }),
          ];

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.internalOrganisation = this.fetcher.getInternalOrganisation(loaded);

        this.organisation = this.allors.context.create<Organisation>(m.Organisation);
        this.organisation.IsManufacturer = false;
        this.organisation.IsInternalOrganisation = false;
        this.organisation.CollectiveWorkEffortInvoice = false;
        this.organisation.PreferredCurrency = this.internalOrganisation.PreferredCurrency;

        this.currencies = loaded.collection<Currency>(m.Currency);
        this.locales = loaded.collection<Locale>(m.Singleton.Locales) || [];
        this.classifications = loaded.collection<CustomOrganisationClassification>(m.CustomOrganisationClassification);
        this.industries = loaded.collection<IndustryClassification>(m.IndustryClassification);
        this.legalForms = loaded.collection<LegalForm>(m.LegalForm);
        this.roles = loaded.collection<OrganisationRole>(m.OrganisationRole);
        this.customerRole = this.roles?.find((v: OrganisationRole) => v.UniqueId === '8b5e0cee-4c98-42f1-8f18-3638fba943a0');
        this.supplierRole = this.roles?.find((v: OrganisationRole) => v.UniqueId === '8c6d629b-1e27-4520-aa8c-e8adf93a5095');
        this.manufacturerRole = this.roles?.find((v: OrganisationRole) => v.UniqueId === '32e74bef-2d79-4427-8902-b093afa81661');
        this.selectableRoles.push(this.customerRole);
        this.selectableRoles.push(this.supplierRole);
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    if (this.activeRoles.indexOf(this.customerRole) > -1) {
      const customerRelationship = this.allors.context.create<CustomerRelationship>(this.m.CustomerRelationship);
      customerRelationship.Customer = this.organisation;
      customerRelationship.InternalOrganisation = this.internalOrganisation;
    }

    if (this.activeRoles.indexOf(this.supplierRole) > -1) {
      const supplierRelationship = this.allors.context.create<SupplierRelationship>(this.m.SupplierRelationship);
      supplierRelationship.Supplier = this.organisation;
      supplierRelationship.InternalOrganisation = this.internalOrganisation;
    }

    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.organisation);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
