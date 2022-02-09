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
  CreateRequest,
  FormService,
  TemplateHostDirective,
} from '@allors/base/workspace/angular/foundation';

@Component({
  templateUrl: 'dynamic-create.component.html',
})
export class AllorsMaterialDynamicCreateComponent implements OnInit, OnDestroy {
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
    private request: CreateRequest,
    private dialogRef: MatDialogRef<AllorsMaterialDynamicCreateComponent>,
    private formService: FormService
  ) {
    this.objectType = this.request.objectType;
  }

  ngOnInit(): void {
    const viewContainerRef = this.templateHost.viewContainerRef;
    viewContainerRef.clear();

    const componentRef = viewContainerRef.createComponent<AllorsForm>(
      this.formService.createForm(this.objectType)
    );

    this.form = componentRef.instance;
    this.form.create(
      this.request.objectType as Class,
      this.request.initializer
    );

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
