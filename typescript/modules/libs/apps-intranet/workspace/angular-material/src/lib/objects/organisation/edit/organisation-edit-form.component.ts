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
  Organisation,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../../services/fetcher/fetcher-service';

@Component({
  selector: 'organisation-edit-form',
  templateUrl: './organisation-edit-form.component.html',
  providers: [ContextService, OldPanelService],
})
export class OrganisationEditFormComponent
  extends AllorsFormComponent<Organisation>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  readonly m: M;

  organisation: Organisation;
  locales: Locale[];
  classifications: CustomOrganisationClassification[];
  industries: IndustryClassification[];
  internalOrganisation: InternalOrganisation;

  private subscription: Subscription;
  legalForms: LegalForm[];
  currencies: Currency[];

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private singletonId: SingletonId,
    private fetcher: FetcherService
  ) {
    super(allors, errorService, form);

    panel.onPull = (pulls) => {
      if (this.panel.isCollapsed) {
        const m = this.m;
        const { pullBuilder: pull } = m;
        const id = this.panel.manager.id;

        pulls.push(
          pull.Organisation({
            name: pullName,
            objectId: id,
          })
        );
      }
    };

    panel.onPulled = (loaded) => {
      if (this.panel.isCollapsed) {
        this.organisation = loaded.object<Organisation>(pullName);
      }
    };
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    // Expanded
    this.subscription = this.panel.manager.on$
      .pipe(
        filter(() => {
          return this.panel.isExpanded;
        }),
        switchMap(() => {
          this.organisation = undefined;

          const id = this.panel.manager.id;

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
            pull.Organisation({
              objectId: id,
              include: {
                LegalForm: x,
                IndustryClassifications: x,
                CustomClassifications: x,
                PreferredCurrency: x,
                LogoImage: x,
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
        this.organisation = loaded.object<Organisation>(m.Organisation);
        this.internalOrganisation =
          this.fetcher.getInternalOrganisation(loaded);
        this.currencies = loaded.collection<Currency>(m.Currency);
        this.locales = loaded.collection<Locale>(m.Singleton.Locales) || [];
        this.classifications =
          loaded.collection<CustomOrganisationClassification>(
            m.CustomOrganisationClassification
          );
        this.industries = loaded.collection<IndustryClassification>(
          m.IndustryClassification
        );
        this.legalForms = loaded.collection<LegalForm>(m.LegalForm);
      });
  }
}
