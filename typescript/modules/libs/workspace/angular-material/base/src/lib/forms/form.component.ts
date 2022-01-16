import { Observable, tap } from 'rxjs';
import { HostBinding, Directive } from '@angular/core';
import { M } from '@allors/workspace/meta/default';
import { Context, ContextService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';
import { AllorsComponent, FormService } from '@allors/workspace/angular/base';
import { Class } from '@allors/workspace/meta/system';
import { NgForm } from '@angular/forms';

@Directive()
export abstract class AllorsFormComponent<
  T extends IObject
> extends AllorsComponent {
  dataAllorsKind = 'form';

  @HostBinding('attr.data-allors-id')
  get dataAllorsId() {
    return this.object?.strategy.id;
  }

  context: Context;
  m: M;

  isCreate: boolean;
  isEdit: boolean;

  object: T;

  edit$: Observable<number>;
  create$: Observable<Class>;
  save$: Observable<void>;
  saved$: Observable<IObject>;
  cancel$: Observable<void>;
  cancelled$: Observable<IObject>;

  constructor(
    allors: ContextService,
    public form: NgForm,
    public formService: FormService
  ) {
    super();

    this.context = allors.context;
    this.context.name = this.constructor.name;
    this.m = this.context.configuration.metaPopulation as M;

    this.create$ = this.formService.create$.pipe(
      tap(() => {
        this.isCreate = true;
        this.isEdit = false;
      })
    );

    this.edit$ = this.formService.edit$.pipe(
      tap(() => {
        this.isCreate = false;
        this.isEdit = true;
      })
    );

    this.save$ = this.formService.save$;
    this.saved$ = this.formService.saved$;
    this.cancel$ = this.formService.cancel$;
    this.cancelled$ = this.formService.cancelled$;

    this.formService.canSave = this.canSave;
  }

  canSave() {
    return this.form.form.valid && this.context.hasChanges;
  }
}
