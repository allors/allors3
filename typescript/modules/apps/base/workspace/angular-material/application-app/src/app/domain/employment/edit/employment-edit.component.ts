import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { M } from '@allors/default/workspace/meta';
import {
  Person,
  Organisation,
  Employment,
} from '@allors/default/workspace/domain';
import {
  ContextService,
  CreateData,
  EditData,
} from '@allors/base/workspace/angular/foundation';
import {
  RefreshService,
  ErrorService,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';

@Component({
  templateUrl: './employment-edit.component.html',
  providers: [ContextService],
})
export class EmploymentEditComponent implements OnInit, OnDestroy {
  readonly m: M;

  title: string;

  employment: Employment;
  person: Person;
  organisation: Organisation;

  organisationsFilter: SearchFactory;

  peopleFilter: SearchFactory;

  addEmployee = false;
  canSave: boolean;

  createData: CreateData;
  editData: EditData;

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) data: CreateData | EditData,
    public dialogRef: MatDialogRef<EmploymentEditComponent>,
    public refreshService: RefreshService,
    private errorService: ErrorService
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;

    this.organisationsFilter = new SearchFactory({
      objectType: this.m.Organisation,
      roleTypes: [this.m.Organisation.Name],
    });

    this.peopleFilter = new SearchFactory({
      objectType: this.m.Person,
      roleTypes: [this.m.Person.FirstName, this.m.Person.LastName],
    });

    this.canSave = true;

    if (data.kind === 'CreateData') {
      this.createData = data;
    } else {
      this.editData = data;
    }
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest([this.refreshService.refresh$])
      .pipe(
        switchMap(() => {
          const pulls = [];

          if (this.editData) {
            pulls.push(
              pull.Employment({
                objectId: this.editData.object.id,
                include: {
                  Employee: x,
                  Employer: x,
                },
              })
            );
          }

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.organisation = loaded.object<Organisation>(m.Organisation);
        this.person = loaded.object<Person>(m.Person);

        if (this.createData) {
          this.title = 'Add Employment';

          this.employment = this.allors.context.create<Employment>(
            m.Employment
          );
          this.employment.FromDate = new Date();

          this.employment.Employer = this.organisation;
          this.employment.Employee = this.person;

          this.createData.onCreate(this.employment);
        } else {
          this.employment = loaded.object<Employment>(m.Employment);

          if (this.employment.canWriteFromDate) {
            this.title = 'Edit Employment';
          } else {
            this.title = 'View Employment';
          }
        }
      });
  }

  public employeeAdded(employee: Person): void {
    this.employment.Employee = employee;
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.employment);
      this.refreshService.refresh();
    }, this.errorService.errorHandler);
  }
}
