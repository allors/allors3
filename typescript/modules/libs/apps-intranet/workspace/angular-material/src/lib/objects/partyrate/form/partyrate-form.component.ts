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
  PartyRate,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
  templateUrl: './partyrate-form.component.html',
  providers: [ContextService],
})
export class PartyRateFormComponent
  extends AllorsFormComponent<PartyRate>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  title: string;
  subTitle: string;

  readonly m: M;

  partyRate: PartyRate;
  timeFrequencies: TimeFrequency[];
  rateTypes: RateType[];

  private subscription: Subscription;
  party: Party;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm
  ) {
    super(allors, errorService, form);
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(this.refreshService.refresh$)
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id == null;

          const pulls = [
            pull.RateType({ sorting: [{ roleType: this.m.RateType.Name }] }),
            pull.TimeFrequency({
              sorting: [{ roleType: this.m.TimeFrequency.Name }],
            }),
          ];

          if (!isCreate) {
            pulls.push(
              pull.PartyRate({
                objectId: this.data.id,
                include: {
                  RateType: x,
                  Frequency: x,
                },
              })
            );
          }

          if (isCreate && this.data.associationId) {
            pulls.push(
              pull.Party({
                objectId: this.data.associationId,
                include: {
                  PartyRates: x,
                },
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

        this.party = loaded.object<Party>(m.Party);
        this.rateTypes = loaded.collection<RateType>(m.RateType);
        this.timeFrequencies = loaded.collection<TimeFrequency>(
          m.TimeFrequency
        );
        const hour = this.timeFrequencies?.find(
          (v) => v.UniqueId === 'db14e5d5-5eaf-4ec8-b149-c558a28d99f5'
        );

        if (isCreate) {
          this.title = 'Add Party Rate';
          this.partyRate = this.allors.context.create<PartyRate>(m.PartyRate);
          this.partyRate.Frequency = hour;
          this.party.addPartyRate(this.partyRate);
        } else {
          this.partyRate = loaded.object<PartyRate>(m.PartyRate);

          if (this.partyRate.canWriteRate) {
            this.title = 'Edit Party Rate';
          } else {
            this.title = 'View Party Rate';
          }
        }
      });
  }
}
