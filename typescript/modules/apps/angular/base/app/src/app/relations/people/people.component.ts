import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Location } from '@angular/common';
import { Title } from '@angular/platform-browser';
import { PageEvent } from '@angular/material/paginator';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Sort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { SelectionModel } from '@angular/cdk/collections';
import { BehaviorSubject, Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';

import { AllorsMaterialDialogService, Filter, MediaService, NavigationService, TableRow, TestScope } from '@allors/workspace/angular/base';
import { Person } from '@allors/workspace/domain/default';
import { SessionService } from '@allors/workspace/angular/core';
import { M } from '@allors/workspace/meta/default';

interface Row extends TableRow {
  person: Person;
  firstName: string;
  lastName: string;
  email: string;
}

@Component({
  templateUrl: './people.component.html',
  providers: [SessionService],
})
export class PeopleComponent extends TestScope implements OnInit, OnDestroy {
  public title = 'People';

  public displayedColumns = ['select', 'firstName', 'lastName', 'email', 'menu'];
  public selection = new SelectionModel<Row>(true, []);

  public total: number;
  public dataSource = new MatTableDataSource<Row>();

  public filter: Filter;

  private sort$: BehaviorSubject<Sort | null>;
  private refresh$: BehaviorSubject<Date>;
  private pager$: BehaviorSubject<PageEvent>;

  private subscription: Subscription;
  m: M;

  constructor(
    @Self() public allors: SessionService,
    public navigation: NavigationService,
    public mediaService: MediaService,
    private snackBar: MatSnackBar,
    private dialogService: AllorsMaterialDialogService,
    private location: Location,
    titleService: Title
  ) {
    super();

    titleService.setTitle(this.title);

    this.m = this.allors.workspace.configuration.metaPopulation as M;

    this.sort$ = new BehaviorSubject<Sort | null>(null);
    this.refresh$ = new BehaviorSubject<Date>(new Date());
    this.pager$ = new BehaviorSubject<PageEvent>(Object.assign(new PageEvent(), { pageIndex: 0, pageSize: 50 }));
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: p } = m;

    const angularMeta = this.allors.workspace.services.angularMetaService;
    const angularPerson = angularMeta.for(m.Person);
    this.filter = angularPerson.filter = angularPerson.filter ??= new Filter(angularPerson.filterDefinition);

    this.subscription = combineLatest([this.refresh$, this.filter.fields$, this.sort$, this.pager$])
      .pipe(
        scan(([previousRefresh, previousFilterFields], [refresh, filterFields, sort, pageEvent]) => {
          return [refresh, filterFields, sort, previousRefresh !== refresh || filterFields !== previousFilterFields ? Object.assign({ pageIndex: 0 }, pageEvent) : pageEvent];
        }),
        switchMap(([, filterFields, sort, pageEvent]) => {
          const pulls = [
            p.Person({
              predicate: this.filter.definition.predicate,
              sorting: sort ? angularPerson.sorter.create(sort) : null,
              include: {
                Pictures: {},
              },
              arguments: this.filter.parameters(filterFields),
              skip: pageEvent.pageIndex * pageEvent.pageSize,
              take: pageEvent.pageSize,
            }),
          ];

          return this.allors.client.pullReactive(this.allors.session, pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.session.reset();
        this.total = loaded.value('People_Total') as number;
        const people = loaded.collection<Person>(m.Person);

        this.dataSource.data = people.map((v) => {
          return {
            person: v,
            firstName: v.FirstName,
            lastName: v.LastName,
            email: v.UserEmail,
          } as Row;
        });
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public get hasSelection() {
    return !this.selection.isEmpty();
  }

  public get hasDeleteSelection() {
    return this.selectedPeople.filter((v) => v.canExecuteDelete).length > 0;
  }

  public get selectedPeople() {
    return this.selection.selected.map((v) => v.person);
  }

  public isAllSelected() {
    const numSelected = this.selection.selected.length;
    const numRows = this.dataSource.data.length;
    return numSelected === numRows;
  }

  public masterToggle() {
    this.isAllSelected() ? this.selection.clear() : this.dataSource.data.forEach((row) => this.selection.select(row));
  }

  public goBack(): void {
    this.location.back();
  }

  public refresh(): void {
    this.refresh$.next(new Date());
  }

  public sort(event: Sort): void {
    this.sort$.next(event);
  }

  public page(event: PageEvent): void {
    this.pager$.next(event);
  }

  public delete(person: Person | Person[]): void {
    const people = person instanceof Array ? person : person != null ? [person] : [];
    const methods = people.filter((v) => v.canExecuteDelete).map((v) => v.Delete);

    if (methods.length > 0) {
      this.dialogService.confirm(methods.length === 1 ? { message: 'Are you sure you want to delete this person?' } : { message: 'Are you sure you want to delete these people?' }).subscribe((confirm: boolean) => {
        if (confirm) {
          this.allors.client.invokeReactive(this.allors.session, methods).subscribe(() => {
            this.snackBar.open('Successfully deleted.', 'close', { duration: 5000 });
            this.refresh();
          });
        }
      });
    }
  }
}
