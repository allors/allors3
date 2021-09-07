import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, UrlSegment } from '@angular/router';
import { DateAdapter } from '@angular/material/core';
import { BehaviorSubject, combineLatest, Observable, Subscription } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { SessionService } from '@allors/workspace/angular/core';
import { RadioGroupOption, SaveService, SearchFactory, TestScope } from '@allors/workspace/angular/base';
import { M } from '@allors/workspace/meta/default';
import { Data, Organisation, Person, Locale } from '@allors/workspace/domain/default';
import { IPullResult } from '@allors/workspace/domain/system';

@Component({
  templateUrl: './form.component.html',
  providers: [SessionService],
})
export class FormComponent extends TestScope implements OnInit, OnDestroy {
  title: string;
  m: M;

  organisations: Organisation[];
  people: Person[];
  locale: Locale;

  jane: Person | undefined;

  organisationFilter: SearchFactory;
  peopleFilter: SearchFactory;

  data: Data | null;

  radioGroupOptions: RadioGroupOption[] = [
    { label: 'One', value: 'one' },
    { label: 'Two', value: 'two' },
    { label: 'Three', value: 'three' },
  ];

  get organisationsWithManagers(): Organisation[] {
    return this.organisations?.filter((v) => v.Manager);
  }

  get organisationsWithEmployees(): Organisation[] {
    return this.organisations?.filter((v) => v.Employees?.length > 0);
  }

  private refresh$: BehaviorSubject<Date>;
  private subscription: Subscription;

  constructor(@Self() public allors: SessionService, private titleService: Title, private route: ActivatedRoute, private saveService: SaveService, private dateAdapter: DateAdapter<string>) {
    super();

    this.title = 'Form';
    this.titleService.setTitle(this.title);

    this.m = this.allors.workspace.configuration.metaPopulation as M;

    this.organisationFilter = new SearchFactory({
      objectType: this.m.Organisation,
      roleTypes: [this.m.Organisation.Name],
    });
    this.peopleFilter = new SearchFactory({
      objectType: this.m.Person,
      roleTypes: [this.m.Person.FirstName, this.m.Person.LastName, this.m.Person.UserName],
    });

    this.refresh$ = new BehaviorSubject<Date>(new Date());
  }

  ngOnInit(): void {
    const route$: Observable<UrlSegment[]> = this.route.url;
    const combined$: Observable<[UrlSegment[], Date]> = combineLatest(route$, this.refresh$);

    const { pullBuilder: p } = this.m;

    this.subscription = combined$
      .pipe(
        switchMap(([,]: [UrlSegment[], Date]) => {
          const pulls = [
            p.Data({
              include: {
                AutocompleteFilter: {},
                AutocompleteOptions: {},
                Chips: {},
                File: {},
                MultipleFiles: {},
              },
            }),
            p.Organisation({
              include: {
                OneData: {},
                ManyDatas: {},
                Owner: {},
                Employees: {},
              },
            }),
            p.Person({}),
            p.Locale({
              include: {
                Language: {},
                Country: {},
              },
            }),
          ];

          return this.allors.client.pullReactive(this.allors.session, pulls);
        })
      )
      .subscribe((loaded: IPullResult) => {
        this.allors.session.reset();

        this.organisations = loaded.collection<Organisation>(this.m.Organisation);
        this.people = loaded.collection<Person>(this.m.Person);
        const datas = loaded.collection<Data>(this.m.Data);

        this.locale = (loaded.collection<Locale>(this.m.Locale).find((v) => v.Name === 'nl-BE');
        this.jane = this.people.find((v) => v.FirstName === 'Jane');

        if (datas && datas.length > 0) {
          this.data = datas[0];
        } else {
          this.data = this.allors.session.create(this.m.Data);
        }
      });
  }

  ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  reset() {
    this.allors.session.reset();
    this.data = this.allors.session.create(this.m.Data);
  }

  newDate() {
    if (this.data) {
      const today = this.dateAdapter.today();
      this.data.Date = today;
    }
  }

  newDateTime() {
    if (this.data) {
      const today = this.dateAdapter.today();
      this.data.DateTime = today;
    }
  }

  newDateTime2() {
    if (this.data) {
      const today = this.dateAdapter.today();
      this.data.DateTime2 = today;
    }
  }

  refresh(): void {
    this.refresh$.next(new Date());
  }

  save(): void {
    console.log('save');

    this.allors.client.pushReactive(this.allors.session).subscribe(() => {
      this.data = null;
      this.refresh();
    }, this.saveService.errorHandler);
  }

  public goBack(): void {}
}
