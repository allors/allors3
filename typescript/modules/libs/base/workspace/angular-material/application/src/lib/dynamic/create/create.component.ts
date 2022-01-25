import { Subscription, tap } from 'rxjs';
import { Class, Composite } from '@allors/system/workspace/meta';
import {
  Component,
  Inject,
  ViewChild,
  OnDestroy,
  Optional,
  OnInit,
} from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import {
  AllorsForm,
  angularForms,
} from '@allors/base/workspace/angular/foundation';
import { CreateData } from '@allors/base/workspace/angular/application';
import { DynamicFormHostDirective } from '../form/form-host.directive';

@Component({
  templateUrl: 'create.component.html',
})
export class DynamicCreateComponent implements OnInit, OnDestroy {
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
    private data: CreateData,
    private dialogRef: MatDialogRef<DynamicCreateComponent>
  ) {
    this.objectType = this.data.objectType;
  }

  ngOnInit(): void {
    const viewContainerRef = this.dynamicFormHost.viewContainerRef;
    viewContainerRef.clear();

    const componentRef = viewContainerRef.createComponent<AllorsForm>(
      angularForms(this.objectType).create
    );

    this.form = componentRef.instance;
    this.form.create(this.data.objectType as Class);

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
