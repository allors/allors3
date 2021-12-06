import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { LegalForm } from '@allors/workspace/domain/default';
import { ObjectData, RefreshService, SaveService } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';

@Component({
  templateUrl: './legalform-edit.component.html',
  providers: [ContextService],
})
export class LegalFormEditComponent implements OnInit, OnDestroy {
  public title: string;
  public subTitle: string;

  public m: M;

  public legalForm: LegalForm;

  private subscription: Subscription;

  constructor(@Self() public allors: ContextService, @Inject(MAT_DIALOG_DATA) public data: ObjectData, public dialogRef: MatDialogRef<LegalFormEditComponent>, public refreshService: RefreshService, private saveService: SaveService) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    this.subscription = combineLatest(this.refreshService.refresh$)
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id == null;

          const pulls = [];

          if (!isCreate) {
            pulls.push(
              pull.LegalForm({
                objectId: this.data.id,
              })
            );
          }

          return this.allors.context.pull(pulls).pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();

        if (isCreate) {
          this.title = 'Add Position Type';
          this.legalForm = this.allors.context.create<LegalForm>(m.LegalForm);
        } else {
          this.legalForm = loaded.object<LegalForm>(m.LegalForm);

          if (this.legalForm.canWriteName) {
            this.title = 'Edit Legal Form';
          } else {
            this.title = 'View Legal Form';
          }
        }
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.legalForm);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
