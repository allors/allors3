import {
  Component,
  Inject,
  ViewChild,
  OnDestroy,
  Optional,
  OnInit,
} from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { AllorsForm } from '@allors/base/workspace/angular/foundation';
import { angularForms } from '@allors/base/workspace/angular/application';
import { DynamicFormHostDirective } from '../form/form-host.directive';
import { EditDialogData } from '../../edit/edit.dialog.data';
import { Composite } from '@allors/system/workspace/meta';
import { Subscription, tap } from 'rxjs';

@Component({
  templateUrl: 'edit.component.html',
})
export class DynamicEditComponent implements OnInit, OnDestroy {
  @ViewChild(DynamicFormHostDirective, { static: true })
  dynamicFormHost!: DynamicFormHostDirective;

  objectType: Composite;
  title: string;

  component: unknown;

  form: AllorsForm;

  private cancelledSubscription: Subscription;
  private savedSubscription: Subscription;

  constructor(
    @Optional()
    @Inject(MAT_DIALOG_DATA)
    private data: EditDialogData,
    private dialogRef: MatDialogRef<DynamicEditComponent>
  ) {
    this.objectType = this.data.objectType ?? this.data.object.strategy.cls;
  }

  ngOnInit(): void {
    const viewContainerRef = this.dynamicFormHost.viewContainerRef;
    viewContainerRef.clear();

    const componentRef = viewContainerRef.createComponent<AllorsForm>(
      angularForms(this.objectType).edit
    );

    this.form = componentRef.instance;
    this.form.edit(this.data.object.id);

    this.cancelledSubscription = this.form.cancelled
      .pipe(tap(() => this.dialogRef.close()))
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
