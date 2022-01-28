import {
  HostBinding,
  Directive,
  EventEmitter,
  Output,
  OnDestroy,
} from '@angular/core';
import { NgForm } from '@angular/forms';
import { M } from '@allors/default/workspace/meta';
import {
  IObject,
  OnObjectCreate,
  OnObjectEdit,
  OnObjectPostCreate,
  OnObjectPostEdit,
  OnObjectPreCreate,
  OnObjectPreEdit,
  Pull,
} from '@allors/system/workspace/domain';
import { AllorsComponent } from '../component';
import { AllorsForm } from './form';
import { Context } from '../context/context';
import { ContextService } from '../context/context-service';
import { ErrorService } from '../error/error.service';
import { Subscription } from 'rxjs';
import { Class } from '@allors/system/workspace/meta';

const nameof = <T>(name: Extract<keyof T, string>): string => name;

@Directive()
export abstract class AllorsFormComponent<T extends IObject>
  extends AllorsComponent
  implements AllorsForm, OnDestroy
{
  override dataAllorsKind = 'form';

  @HostBinding('attr.data-allors-id')
  get dataAllorsId() {
    return this.object?.strategy.id;
  }

  isCreate: boolean;

  get isEdit() {
    return !this.isCreate;
  }

  context: Context;
  m: M;
  object: T;

  @Output()
  saved: EventEmitter<IObject> = new EventEmitter();

  @Output()
  cancelled: EventEmitter<void> = new EventEmitter();

  constructor(
    allors: ContextService,
    private errorService: ErrorService,
    public form: NgForm
  ) {
    super();

    this.context = allors.context;
    this.context.name = this.constructor.name;
    this.m = this.context.configuration.metaPopulation as M;
  }

  private createSubscription: Subscription;
  private editSubscription: Subscription;

  get canSave() {
    return this.form.form.valid && this.context.hasChanges();
  }

  get canWrite(): any {
    return true;
  }

  create(objectType: Class, handlers?: OnObjectCreate[]): void {
    this.isCreate = true;

    const hasPreCreate =
      this[nameof<OnObjectPreCreate>('onObjectPreCreate')] != null;
    const hasPostCreate =
      this[nameof<OnObjectPostCreate>('onObjectPostCreate')] != null;
    const hasHandlers = handlers?.length > 0;

    if (hasPreCreate || hasPostCreate || hasHandlers) {
      const pulls: Pull[] = [];

      if (hasPreCreate) {
        (this as unknown as OnObjectPreCreate).onObjectPreCreate(pulls);
      }
      handlers?.forEach((v) => v.onObjectPreCreate(pulls));

      this.createSubscription = this.context
        .pull(pulls)
        .subscribe((pullResult) => {
          this.onObjectCreate(objectType);

          handlers?.forEach((v) =>
            v.onObjectPostCreate(this.object, pullResult)
          );

          if (hasPostCreate) {
            (this as unknown as OnObjectPostCreate).onObjectPostCreate(
              this.object,
              pullResult
            );
          }
        });
    } else {
      this.onObjectCreate(objectType);
    }
  }

  onObjectCreate(objectType: Class) {
    this.object = this.context.create<T>(objectType);
  }

  edit(objectId: number, handlers?: OnObjectEdit[]): void {
    this.isCreate = false;

    const name = 'AllorsFormComponent';
    const pull: Pull = { objectId, results: [{ name }] };

    const hasPreEdit = this[nameof<OnObjectPreEdit>('onObjectPreEdit')] != null;
    const hasPostEdit =
      this[nameof<OnObjectPostEdit>('onObjectPostEdit')] != null;
    const hasHandlers = handlers?.length > 0;

    if (hasPreEdit || hasPostEdit || hasHandlers) {
      const pulls: Pull[] = [pull];

      if (hasPreEdit) {
        (this as unknown as OnObjectPreEdit).onObjectPreEdit(objectId, pulls);
      }
      handlers?.forEach((v) => v.onObjectPreEdit(objectId, pulls));

      this.editSubscription = this.context.pull(pulls).subscribe((result) => {
        this.object = result.objects.values().next()?.value;

        handlers?.forEach((v) => v.onObjectPostEdit(this.object, result));

        if (hasPostEdit) {
          (this as unknown as OnObjectPostEdit).onObjectPostEdit(
            this.object,
            result
          );
        }
      });
    } else {
      this.context.pull(pull).subscribe((result) => {
        this.object = result.object(name);
      });
    }
  }

  save(): void {
    this.context.push().subscribe({
      next: () => {
        this.saved.emit(this.object);
      },
      error: (error) => {
        this.errorService.errorHandler(error);
      },
    });
  }

  cancel(): void {
    this.cancelled.emit();
  }

  ngOnDestroy(): void {
    this.createSubscription?.unsubscribe();
    this.editSubscription?.unsubscribe();
  }
}
