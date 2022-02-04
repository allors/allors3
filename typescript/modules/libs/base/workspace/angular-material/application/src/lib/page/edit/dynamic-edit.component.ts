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
  EditRequest,
  FormService,
  TemplateHostDirective,
} from '@allors/base/workspace/angular/foundation';
import { Composite } from '@allors/system/workspace/meta';
import { Subscription, tap } from 'rxjs';

@Component({
  templateUrl: 'dynamic-edit.component.html',
})
export class AllorsMaterialDynamicEditComponent implements OnInit, OnDestroy {
  @ViewChild(TemplateHostDirective, { static: true })
  templateHost!: TemplateHostDirective;

  objectType: Composite;
  title: string;

  component: unknown;

  form: AllorsForm;

  private cancelledSubscription: Subscription;
  private savedSubscription: Subscription;

  constructor(
    @Optional()
    @Inject(MAT_DIALOG_DATA)
    private request: EditRequest,
    private dialogRef: MatDialogRef<AllorsMaterialDynamicEditComponent>,
    private formService: FormService
  ) {
    this.objectType =
      this.request.objectType ?? this.request.object.strategy.cls;
  }

  ngOnInit(): void {
    const viewContainerRef = this.templateHost.viewContainerRef;
    viewContainerRef.clear();

    const componentRef = viewContainerRef.createComponent<AllorsForm>(
      this.formService.editForm(this.objectType)
    );

    this.form = componentRef.instance;
    this.form.edit(this.request.object.id);

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
