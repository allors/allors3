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
  OnCreate,
  OnEdit,
  OnPostCreate,
  OnPostCreateOrEdit,
  OnPostEdit,
  OnPreCreate,
  OnPreCreateOrEdit,
  OnPreEdit,
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

  get canWrite() {
    return true;
  }

  get hasPreCreate() {
    return this[nameof<OnPreCreate>('onPreCreate')] != null;
  }

  get hasPreEdit() {
    return this[nameof<OnPreEdit>('onPreEdit')] != null;
  }

  get hasPreCreateOrEdit() {
    return this[nameof<OnPreCreateOrEdit>('onPreCreateOrEdit')] != null;
  }

  get hasPostCreate() {
    return this[nameof<OnPostCreate>('onPostCreate')] != null;
  }

  get hasPostEdit() {
    return this[nameof<OnPostEdit>('onPostEdit')] != null;
  }

  get hasPostCreateOrEdit() {
    return this[nameof<OnPostCreateOrEdit>('onPostCreateOrEdit')] != null;
  }

  create(objectType: Class, handlers?: OnCreate[]): void {
    this.isCreate = true;

    const hasHandlers = handlers?.length > 0;

    if (
      this.hasPreCreate ||
      this.hasPostCreate ||
      this.hasPreCreateOrEdit ||
      this.hasPostCreateOrEdit ||
      hasHandlers
    ) {
      const pulls: Pull[] = [];

      if (this.hasPreCreate) {
        (this as unknown as OnPreCreate).onPreCreate(pulls);
      }

      if (this.hasPreCreateOrEdit) {
        (this as unknown as OnPreCreateOrEdit).onPreCreateOrEdit(pulls);
      }

      handlers?.forEach((v) => v.onPreCreate(pulls));

      this.createSubscription = this.context
        .pull(pulls)
        .subscribe((pullResult) => {
          this.onCreate(objectType);

          handlers?.forEach((v) => v.onPostCreate(this.object, pullResult));

          if (this.hasPostCreate) {
            (this as unknown as OnPostCreate).onPostCreate(
              this.object,
              pullResult
            );
          }

          if (this.hasPostCreateOrEdit) {
            (this as unknown as OnPostCreateOrEdit).onPostCreateOrEdit(
              this.object,
              pullResult
            );
          }
        });
    } else {
      this.onCreate(objectType);
    }
  }

  onCreate(objectType: Class) {
    this.object = this.context.create<T>(objectType);
  }

  edit(objectId: number, handlers?: OnEdit[]): void {
    this.isCreate = false;

    const name = 'AllorsFormComponent';
    const pull: Pull = { objectId, results: [{ name }] };

    const hasHandlers = handlers?.length > 0;

    if (
      this.hasPreEdit ||
      this.hasPostEdit ||
      this.hasPreCreateOrEdit ||
      this.hasPostCreateOrEdit ||
      hasHandlers
    ) {
      const pulls: Pull[] = [pull];

      if (this.hasPreEdit) {
        (this as unknown as OnPreEdit).onPreEdit(objectId, pulls);
      }

      if (this.hasPreCreateOrEdit) {
        (this as unknown as OnPreCreateOrEdit).onPreCreateOrEdit(pulls);
      }

      handlers?.forEach((v) => v.onPreEdit(objectId, pulls));

      this.editSubscription = this.context.pull(pulls).subscribe((result) => {
        this.object = result.objects.values().next()?.value;

        handlers?.forEach((v) => v.onPostEdit(this.object, result));

        if (this.hasPostEdit) {
          (this as unknown as OnPostEdit).onPostEdit(this.object, result);
        }

        if (this.hasPostCreateOrEdit) {
          (this as unknown as OnPostCreateOrEdit).onPostCreateOrEdit(
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
