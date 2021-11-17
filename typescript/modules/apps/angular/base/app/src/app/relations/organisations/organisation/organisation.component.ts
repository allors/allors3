import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, UrlSegment } from '@angular/router';
import { BehaviorSubject, Observable, Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { SearchFactory, TestScope } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { Organisation, Person } from '@allors/workspace/domain/default';
import { IObject, IPullResult } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';

@Component({
  templateUrl: './organisation.component.html',
  providers: [ContextService],
})
export class OrganisationComponent extends TestScope implements OnInit, OnDestroy {
  title: string;
  m: M;
  peopleFilter: SearchFactory;

  selected: IObject;
  people: Person[];
  organisation: Organisation;

  private subscription: Subscription;

  private refresh$: BehaviorSubject<Date>;

  constructor(@Self() public allors: ContextService, private titleService: Title, private route: ActivatedRoute) {
    super();

    this.allors.context.name = this.constructor.name;
    this.title = 'Organisation';
    this.titleService.setTitle(this.title);

    this.m = this.allors.context.configuration.metaPopulation as M;

    this.peopleFilter = new SearchFactory({ objectType: this.m.Person, roleTypes: [this.m.Person.UserName] });

    this.refresh$ = new BehaviorSubject<Date>(new Date());
  }

  public ngOnInit(): void {
    const route$: Observable<UrlSegment[]> = this.route.url;
    const combined$: Observable<[UrlSegment[], Date]> = combineLatest([route$, this.refresh$]);

    const { pullBuilder: p } = this.m;

    this.subscription = combined$
      .pipe(
        switchMap(([,]: [UrlSegment[], Date]) => {
          const id = this.route.snapshot.paramMap.get('id');

          const pulls = [
            p.Organisation({
              objectId: id ?? '',
            }),
            p.Person({}),
          ];

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded: IPullResult) => {
        this.allors.context.reset();

        this.organisation = loaded.object<Organisation>(this.m.Organisation) ?? this.allors.context.create(this.m.Organisation);
        this.people = loaded.collection<Person>(this.m.Person);
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public refresh(): void {
    this.refresh$.next(new Date());
  }

  public togglecanWrite() {
    this.allors.context.invoke(this.organisation.ToggleCanWrite).subscribe(() => {
      this.refresh();
    });
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.goBack();
    });
  }

  public goBack(): void {
    window.history.back();
  }

  public ownerSelected(selected: IObject): void {
    this.selected = selected;
  }
}
