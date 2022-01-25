import { Subscription, combineLatest, BehaviorSubject } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { M } from '@allors/default/workspace/meta';
import { Organisation, Country } from '@allors/default/workspace/domain';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import {
  RefreshService,
  ErrorService,
} from '@allors/base/workspace/angular/foundation';
import { CreateData } from '@allors/base/workspace/angular/application';

@Component({
  templateUrl: './organisation-create.component.html',
  providers: [ContextService],
})
export class OrganisationCreateComponent implements OnInit, OnDestroy {
  m: M;

  title = 'Add Organisation';

  organisation: Organisation;

  countries: Country[];

  private refresh$: BehaviorSubject<Date>;
  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: CreateData,
    public dialogRef: MatDialogRef<OrganisationCreateComponent>,
    public refreshService: RefreshService,
    private errorService: ErrorService,
    private route: ActivatedRoute
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
    this.refresh$ = new BehaviorSubject<Date>(undefined);
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    this.subscription = combineLatest([this.route.url, this.refresh$])
      .pipe(
        switchMap(([,]) => {
          const id: string = this.route.snapshot.paramMap.get('id');

          const pulls = [
            pull.Locale({
              sorting: [{ roleType: m.Locale.Country }],
            }),
            pull.Country({}),
          ];

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.organisation = this.allors.context.create<Organisation>(
          m.Organisation
        );
        this.countries = loaded.collection<Country>(m.Country);
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.organisation);
      this.refreshService.refresh();
    }, this.errorService.errorHandler);
  }
}
