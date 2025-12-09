import { BehaviorSubject, combineLatest, Observable, Subscription } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, UrlSegment } from '@angular/router';
import { M } from '@allors/default/workspace/meta';
import {
  Data,
  Organisation,
  Person,
  Locale,
} from '@allors/default/workspace/domain';
import { IPullResult } from '@allors/system/workspace/domain';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import {
  ErrorService,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { RadioGroupOption } from '@allors/base/workspace/angular-material/foundation';

@Component({
  templateUrl: './fields-form.component.html',
  providers: [ContextService],
})
export class FieldsComponent implements OnInit, OnDestroy {
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
    return this.organisations?.filter((v) => v.Employees.length > 0);
  }

  private refresh$: BehaviorSubject<Date>;
  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    private titleService: Title,
    private route: ActivatedRoute,
    private errorService: ErrorService
  ) {
    this.allors.context.name = this.constructor.name;
    this.title = 'Form';
    this.titleService.setTitle(this.title);

    this.m = this.allors.context.configuration.metaPopulation as M;

    this.organisationFilter = new SearchFactory({
      objectType: this.m.Organisation,
      roleTypes: [this.m.Organisation.Name],
    });
    this.peopleFilter = new SearchFactory({
      objectType: this.m.Person,
      roleTypes: [
        this.m.Person.FirstName,
        this.m.Person.LastName,
        this.m.Person.UserName,
      ],
    });

    this.refresh$ = new BehaviorSubject<Date>(new Date());
  }

  ngOnInit(): void {
    const route$: Observable<UrlSegment[]> = this.route.url;
    const combined$: Observable<[UrlSegment[], Date]> = combineLatest([
      route$,
      this.refresh$,
    ]);

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
                LocalisedTexts: {},
                LocalisedMarkdowns: {},
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

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded: IPullResult) => {
        this.allors.context.reset();

        this.organisations = loaded.collection<Organisation>(
          this.m.Organisation
        );
        this.people = loaded.collection<Person>(this.m.Person);
        const datas = loaded.collection<Data>(this.m.Data);

        this.locale = loaded
          .collection<Locale>(this.m.Locale)
          .find((v) => v.Name === 'nl');
        this.jane = this.people.find((v) => v.FirstName === 'Jane');

        if (datas && datas.length > 0) {
          this.data = datas[0];
        } else {
          this.data = this.allors.context.create(this.m.Data);
        }
      });
  }

  ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  reset() {
    this.allors.context.reset();
    this.data = this.allors.context.create(this.m.Data);
  }

  newDate() {
    if (this.data) {
      this.data.Date = new Date();
    }
  }

  newDateTime() {
    if (this.data) {
      this.data.DateTime = new Date();
    }
  }

  newDateTime2() {
    if (this.data) {
      this.data.DateTime2 = new Date();
    }
  }

  refresh(): void {
    this.refresh$.next(new Date());
  }

  save(): void {
    console.log('save');

    this.allors.context.push().subscribe(() => {
      console.log('+');
      this.data = null;
      this.refresh();
    }, this.errorService.errorHandler);
  }

  public goBack(): void {}
}
