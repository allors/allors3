import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest, BehaviorSubject } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { InternalOrganisation, Locale, Organisation, Currency, CustomOrganisationClassification, IndustryClassification, LegalForm, CustomerRelationship, SupplierRelationship, OrganisationRole } from '@allors/workspace/domain/default';
import { ObjectData, RefreshService, SaveService, TestScope, SingletonId } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  templateUrl: './organisation-create.component.html',
  providers: [SessionService],
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
  private isActiveCustomer: boolean;
  private isActiveSupplier: boolean;

  private refresh$: BehaviorSubject<Date>;
  private subscription: Subscription;

  legalForms: LegalForm[];
  currencies: Currency[];

  constructor(
    @Self() public allors: SessionService,
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

    this.m = this.allors.workspace.configuration.metaPopulation as M;
    this.refresh$ = new BehaviorSubject<Date>(undefined);
  }

  public ngOnInit(): void {
    const m = this.allors.workspace.configuration.metaPopulation as M;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(this.route.url, this.refresh$, this.internalOrganisationId.observable$)
      .pipe(
        switchMap(([, , internalOrganisationId]) => {
          const id: string = this.route.snapshot.paramMap.get('id');

          const pulls = [
            this.fetcher.internalOrganisation,
            this.fetcher.locales,
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
            pull.Organisation({ objectId: id }),
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

          if (id != null) {
            pulls.push(
              pull.CustomerRelationship({
                predicate: new And({
                  operands: [
                    { kind: 'Equals',  propertyType: m.CustomerRelationship.Customer, objectId: id }),
                    { kind: 'Equals',  propertyType: m.CustomerRelationship.InternalOrganisation, object: internalOrganisationId }),
                    new Not({
                      operand: new Exists({ propertyType: m.CustomerRelationship.ThroughDate }),
                    }),
                  ],
                }),
              })
            );

            pulls.push(
              pull.SupplierRelationship({
                predicate: new And({
                  operands: [
                    { kind: 'Equals',  propertyType: m.SupplierRelationship.Supplier, objectId: id }),
                    { kind: 'Equals',  propertyType: m.SupplierRelationship.InternalOrganisation, object: internalOrganisationId }),
                    new Not({
                      operand: new Exists({ propertyType: m.SupplierRelationship.ThroughDate }),
                    }),
                  ],
                }),
              })
            );
          }

          return this.allors.client.pullReactive(this.allors.session, pulls);
        })
      )
      .subscribe((loaded) => {
        this.organisation = loaded.object<Organisation>(m.Organisation);
        this.internalOrganisation = loaded.object<Organisation>(m.InternalOrganisation);

        if (this.organisation) {
          this.customerRelationship = loaded.collections.CustomerRelationships[0] as CustomerRelationship;
          this.supplierRelationship = loaded.collections.SupplierRelationships[0] as SupplierRelationship;
        } else {
          this.organisation = this.allors.session.create<Organisation>(m.Organisation);
          this.organisation.IsManufacturer = false;
          this.organisation.IsInternalOrganisation = false;
          this.organisation.CollectiveWorkEffortInvoice = false;
          this.organisation.PreferredCurrency = this.internalOrganisation.PreferredCurrency;
        }

        this.currencies = loaded.collection<Currency>(m.Currency);
        this.locales = loaded.collection<Locale>(m.Locale) || [];
        this.classifications = loaded.collection<CustomOrganisationClassification>(m.CustomOrganisationClassification);
        this.industries = loaded.collection<IndustryClassification>(m.IndustryClassification);
        this.legalForms = loaded.collection<LegalForm>(m.LegalForm);
        this.roles = loaded.collection<OrganisationRole>(m.OrganisationRole);
        this.customerRole = this.roles.find((v: OrganisationRole) => v.UniqueId === '8b5e0cee-4c98-42f1-8f18-3638fba943a0');
        this.supplierRole = this.roles.find((v: OrganisationRole) => v.UniqueId === '8c6d629b-1e27-4520-aa8c-e8adf93a5095');
        this.manufacturerRole = this.roles.find((v: OrganisationRole) => v.UniqueId === '32e74bef-2d79-4427-8902-b093afa81661');
        this.selectableRoles.push(this.customerRole);
        this.selectableRoles.push(this.supplierRole);

        if (this.internalOrganisation.ActiveCustomers.includes(this.organisation)) {
          this.isActiveCustomer = true;
          this.activeRoles.push(this.customerRole);
        }

        if (this.internalOrganisation.ActiveSuppliers.includes(this.organisation)) {
          this.isActiveSupplier = true;
          this.activeRoles.push(this.supplierRole);
        }

        if (this.organisation.IsManufacturer) {
          this.activeRoles.push(this.manufacturerRole);
        }
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    if (this.activeRoles.indexOf(this.customerRole) > -1 && !this.isActiveCustomer) {
      const customerRelationship = this.allors.session.create<CustomerRelationship>(m.CustomerRelationship);
      customerRelationship.Customer = this.organisation;
      customerRelationship.InternalOrganisation = this.internalOrganisation;
    }

    if (this.activeRoles.indexOf(this.customerRole) > -1 && this.customerRelationship) {
      this.customerRelationship.ThroughDate = null;
    }

    if (this.activeRoles.indexOf(this.customerRole) === -1 && this.isActiveCustomer) {
      this.customerRelationship.ThroughDate = new Date();
    }

    if (this.activeRoles.indexOf(this.supplierRole) > -1 && !this.isActiveSupplier) {
      const supplierRelationship = this.allors.session.create<SupplierRelationship>(m.SupplierRelationship);
      supplierRelationship.Supplier = this.organisation;
      supplierRelationship.InternalOrganisation = this.internalOrganisation;
    }

    if (this.activeRoles.indexOf(this.supplierRole) > -1 && this.supplierRelationship) {
      this.supplierRelationship.ThroughDate = null;
    }

    if (this.activeRoles.indexOf(this.supplierRole) === -1 && this.isActiveSupplier) {
      this.supplierRelationship.ThroughDate = new Date();
    }

    this.allors.client.pushReactive(this.allors.session).subscribe(() => {
      this.dialogRef.close(this.organisation);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
