import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { SessionService, MetaService, RefreshService, Saved } from '@allors/angular/services/core';
import { Party, PartyRate, TimeFrequency, RateType } from '@allors/domain/generated';
import { PullRequest } from '@allors/protocol/system';
import { Meta } from '@allors/meta/generated';
import { SaveService, ObjectData } from '@allors/angular/material/services/core';
import { IObject } from '@allors/domain/system';
import { Sort } from '@allors/data/system';
import { TestScope } from '@allors/angular/core';

@Component({
  templateUrl: './partyrate-edit.component.html',
  providers: [SessionService]
})
export class PartyRateEditComponent extends TestScope implements OnInit, OnDestroy {

  title: string;
  subTitle: string;

  readonly m: M;

  partyRate: PartyRate;
  timeFrequencies: TimeFrequency[];
  rateTypes: RateType[];

  private subscription: Subscription;
  party: Party;

  constructor(
    @Self() public allors: SessionService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<PartyRateEditComponent>,
    
    public refreshService: RefreshService,
    private saveService: SaveService,
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {

    const { pullBuilder: pull } = this.m; const x = {};

    this.subscription = combineLatest(this.refreshService.refresh$)
      .pipe(
        switchMap(() => {

          const isCreate = this.data.id === undefined;

          const pulls = [
            pull.RateType({ sorting: [{ roleType: this.m.RateType.Name }] }),
            pull.TimeFrequency({ sorting: [{ roleType: this.m.TimeFrequency.Name }] }),
          ];

          if (!isCreate) {
            pulls.push(
              pull.PartyRate({
                object: this.data.id,
                include: {
                  RateType: x,
                  Frequency: x
                }
              }),
            );
          }


          if (isCreate && this.data.associationId) {
            pulls.push(
              pull.Party({
                object: this.data.associationId,
                include: {
                  PartyRates: x,
                }
              }),
            );
          }

          return this.allors.context
            .load(new PullRequest({ pulls }))
            .pipe(
              map((loaded) => ({ loaded, isCreate }))
            );
        })
      )
      .subscribe(({ loaded, isCreate }) => {

        this.allors.context.reset();

        this.party = loaded.objects.Party as Party;
        this.rateTypes = loaded.collections.RateTypes as RateType[];
        this.timeFrequencies = loaded.collections.TimeFrequencies as TimeFrequency[];
        const hour = this.timeFrequencies.find((v) => v.UniqueId === 'db14e5d5-5eaf-4ec8-b149-c558a28d99f5');

        if (isCreate) {
          this.title = 'Add Party Rate';
          this.partyRate = this.allors.context.create('PartyRate') as PartyRate;
          this.partyRate.Frequency = hour;
          this.party.AddPartyRate(this.partyRate);
        } else {
          this.partyRate = loaded.objects.PartyRate as PartyRate;

          if (this.partyRate.CanWriteRate) {
            this.title = 'Edit Party Rate';
          } else {
            this.title = 'View Party Rate';
          }
        }
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public setDirty(): void {
    this.allors.context.session.hasChanges = true;
  }

  public save(): void {

    this.allors.context
      .save()
      .subscribe(() => {
        const data: IObject = {
          id: this.partyRate.id,
          objectType: this.partyRate.objectType,
        };

        this.dialogRef.close(data);
        this.refreshService.refresh();
      },
        this.saveService.errorHandler
      );
  }
}
