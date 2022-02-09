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
  CreatePullHandler,
  EditPullHandler,
  PostCreateOrEditPullHandler,
  PreCreatePullHandler,
  PreCreateOrEditPullHandler,
  PreEditPullHandler,
  Pull,
  PostEditPullHandler,
  PostCreatePullHandler,
  EditIncludeHandler,
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
    return this[nameof<PreCreatePullHandler>('onPreCreatePull')] != null;
  }

  get hasPreEdit() {
    return this[nameof<PreEditPullHandler>('onPreEditPull')] != null;
  }

  get hasPreCreateOrEdit() {
    return (
      this[nameof<PreCreateOrEditPullHandler>('onPreCreateOrEditPull')] != null
    );
  }

  get hasEditInclude() {
    return this[nameof<EditIncludeHandler>('onEditInclude')] != null;
  }

  get hasPostCreate() {
    return this[nameof<PostCreatePullHandler>('onPostCreatePull')] != null;
  }

  get hasPostEdit() {
    return this[nameof<PostEditPullHandler>('onPostEditPull')] != null;
  }

  get hasPostCreateOrEdit() {
    return (
      this[nameof<PostCreateOrEditPullHandler>('onPostCreateOrEditPull')] !=
      null
    );
  }

  create(objectType: Class, handlers?: CreatePullHandler[]): void {
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
        (this as unknown as PreCreatePullHandler).onPreCreatePull(pulls);
      }

      if (this.hasPreCreateOrEdit) {
        (this as unknown as PreCreateOrEditPullHandler).onPreCreateOrEditPull(
          pulls
        );
      }

      handlers?.forEach((v) => v.onPreCreatePull(pulls));

      this.createSubscription = this.context
        .pull(pulls)
        .subscribe((pullResult) => {
          this.onCreate(objectType);

          handlers?.forEach((v) => v.onPostCreatePull(this.object, pullResult));

          if (this.hasPostCreate) {
            (this as unknown as PostCreatePullHandler).onPostCreatePull(
              this.object,
              pullResult
            );
          }

          if (this.hasPostCreateOrEdit) {
            (
              this as unknown as PostCreateOrEditPullHandler
            ).onPostCreateOrEditPull(this.object, pullResult);
          }
        });
    } else {
      this.onCreate(objectType);
    }
  }

  onCreate(objectType: Class) {
    this.object = this.context.create<T>(objectType);
  }

  edit(objectId: number, handlers?: EditPullHandler[]): void {
    this.isCreate = false;

    const name = 'AllorsFormComponent';

    const include = this.hasEditInclude
      ? (this as unknown as EditIncludeHandler).onEditInclude()
      : null;
    const pull: Pull = { objectId, results: [{ name, include }] };

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
        (this as unknown as PreEditPullHandler).onPreEditPull(objectId, pulls);
      }

      if (this.hasPreCreateOrEdit) {
        (this as unknown as PreCreateOrEditPullHandler).onPreCreateOrEditPull(
          pulls
        );
      }

      handlers?.forEach((v) => v.onPreEditPull(objectId, pulls));

      this.editSubscription = this.context.pull(pulls).subscribe((result) => {
        this.object = result.objects.values().next()?.value;

        handlers?.forEach((v) => v.onPostEditPull(this.object, result));

        if (this.hasPostEdit) {
          (this as unknown as PostEditPullHandler).onPostEditPull(
            this.object,
            result
          );
        }

        if (this.hasPostCreateOrEdit) {
          (
            this as unknown as PostCreateOrEditPullHandler
          ).onPostCreateOrEditPull(this.object, result);
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
