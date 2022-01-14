import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';
import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { M } from '@allors/workspace/meta/default';
import {
  Person,
  Organisation,
  Employment,
} from '@allors/workspace/domain/default';
import { ContextService } from '@allors/workspace/angular/core';
import {
  ObjectData,
  RefreshService,
  SaveService,
  SearchFactory,
} from '@allors/workspace/angular/base';

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

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<EmploymentEditComponent>,
    public refreshService: RefreshService,
    private saveService: SaveService
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
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest([this.refreshService.refresh$])
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id == null;

          const pulls = [];

          if (!isCreate) {
            pulls.push(
              pull.Employment({
                objectId: this.data.id,
                include: {
                  Employee: x,
                  Employer: x,
                },
              })
            );
          }

          if (isCreate && this.data.associationId) {
            pulls.push(
              pull.Organisation({
                objectId: this.data.associationId,
              }),
              pull.Person({
                objectId: this.data.associationId,
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

        this.organisation = loaded.object<Organisation>(m.Organisation);
        this.person = loaded.object<Person>(m.Person);

        if (isCreate) {
          this.title = 'Add Employment';

          this.employment = this.allors.context.create<Employment>(
            m.Employment
          );
          this.employment.FromDate = new Date();
          this.employment.Employer = this.organisation;
          this.employment.Employee = this.person;
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
    }, this.saveService.errorHandler);
  }
}
