import {
  Component,
  Inject,
  AfterViewInit,
  ViewChild,
  ComponentRef,
  OnDestroy,
  Optional,
} from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { IObject } from '@allors/system/workspace/domain';
import {
  AllorsForm,
  angularForms,
} from '@allors/base/workspace/angular/foundation';
import { DynamicFormHostDirective } from '../form/form-host.directive';
import { CreateDialogData } from '../../create/create.dialog.data';
import { EditDialogData } from '../../edit/edit.dialog.data';
import { Class } from '@allors/system/workspace/meta';
import { Subscription, tap } from 'rxjs';

@Component({
  templateUrl: 'edit.component.html',
})
export class DynamicEditComponent implements AfterViewInit, OnDestroy {
  @ViewChild(DynamicFormHostDirective, { static: true })
  dynamicFormHost!: DynamicFormHostDirective;

  component: unknown;

  form: AllorsForm;

  private cancelledSubscription: Subscription;
  private savedSubscription: Subscription;

  constructor(
    @Optional()
    @Inject(MAT_DIALOG_DATA)
    private data: CreateDialogData | EditDialogData,
    private dialogRef: MatDialogRef<DynamicEditComponent>
  ) {}

  ngAfterViewInit(): void {
    const viewContainerRef = this.dynamicFormHost.viewContainerRef;
    viewContainerRef.clear();

    let componentRef: ComponentRef<AllorsForm>;

    switch (this.data.kind) {
      case 'EditDialogData':
        componentRef = viewContainerRef.createComponent<AllorsForm>(
          angularForms(this.data.objectType).edit
        );

        this.form = componentRef.instance;
        this.form.edit(this.data.object.id);
        break;

      case 'CreateDialogData':
        componentRef = viewContainerRef.createComponent<AllorsForm>(
          angularForms(this.data.objectType).create
        );

        this.form = componentRef.instance;
        this.form.create(this.data.objectType as Class);
        break;
    }

    this.cancelledSubscription = this.form.cancelled
      .pipe(tap(() => this.dialogRef.close))
      .subscribe();

    this.savedSubscription = this.form.saved
      .pipe(tap((object) => this.dialogRef.close(object)))
      .subscribe();
  }

  ngOnDestroy(): void {
    this.cancelledSubscription?.unsubscribe();
    this.savedSubscription?.unsubscribe();
  }
}
