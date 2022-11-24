import { Subscription, combineLatest, BehaviorSubject } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { M } from '@allors/default/workspace/meta';
import { Locale, Person, Organisation } from '@allors/default/workspace/domain';
import {
  ContextService,
  CreateRequest,
} from '@allors/base/workspace/angular/foundation';
import {
  RefreshService,
  ErrorService,
} from '@allors/base/workspace/angular/foundation';
import { NavigationService } from '@allors/base/workspace/angular/application';

@Component({
  templateUrl: './person-create-dialog.component.html',
  providers: [ContextService],
})
export class PersonCreateDialogComponent implements OnInit, OnDestroy {
  readonly m: M;

  public title = 'Add Person';

  person: Person;
  organisation: Organisation;

  locales: Locale[];

  private subscription: Subscription;
  private readonly refresh$: BehaviorSubject<Date>;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: CreateRequest,
    public dialogRef: MatDialogRef<PersonCreateDialogComponent>,
    public navigation: NavigationService,
    public refreshService: RefreshService,
    private route: ActivatedRoute,
    private errorService: ErrorService
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
    this.refresh$ = new BehaviorSubject<Date>(undefined);
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest([this.route.url, this.refresh$])
      .pipe(
        switchMap(([url, refresh]) => {
          const pulls = [
            pull.Locale({
              select: {
                include: {
                  Language: x,
                  Country: x,
                },
              },
            }),
            pull.Organisation({
              objectId: this.data.initializer?.id,
            }),
          ];

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.organisation = loaded.object<Organisation>(m.Organisation);
        this.locales = loaded.collection<Locale>(m.Locale) || [];

        this.person = this.allors.context.create<Person>(m.Person);
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    if (this.organisation != null) {
      this.organisation.Owner = this.person;
    }

    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.person);
      this.refreshService.refresh();
    }, this.errorService.errorHandler);
  }
}
