import {
  HostBinding,
  Directive,
  EventEmitter,
  Output,
  OnDestroy,
} from '@angular/core';
import { NgForm } from '@angular/forms';
import { IObject, IPullResult, Pull } from '@allors/system/workspace/domain';
import { AllorsComponent } from '../component';
import { AllorsForm } from './form';
import { Context } from '../context/context';
import { ContextService } from '../context/context-service';
import { ErrorService } from '../error/error.service';
import { Subscription } from 'rxjs';
import { CreateRequest } from '../create/create-request';
import { EditRequest } from '../edit/edit-request';
import { AssociationType, RoleType } from '@allors/system/workspace/meta';

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

  @Output()
  saved: EventEmitter<IObject> = new EventEmitter();

  @Output()
  cancelled: EventEmitter<void> = new EventEmitter();

  createRequest: CreateRequest;

  editRequest: EditRequest;

  context: Context;
  object: T;

  constructor(
    allors: ContextService,
    private errorService: ErrorService,
    public form: NgForm
  ) {
    super();

    this.context = allors.context;
    this.context.name = this.constructor.name;
  }

  private formSubscription: Subscription;

  get canSave() {
    return this.canWrite && this.form.form.valid && this.context.hasChanges();
  }

  get canWrite() {
    return true;
  }

  create(createRequest: CreateRequest): void {
    this.createRequest = createRequest;
    this.onPull();
  }

  edit(editRequest: EditRequest): void {
    this.editRequest = editRequest;
    this.onPull();
  }

  onPull() {
    const pulls: Pull[] = [];
    this.onPrePull(pulls);
    this.formSubscription?.unsubscribe();
    this.formSubscription = this.context.pull(pulls).subscribe((pullResult) => {
      this.onPostPull(pullResult);
    });
  }

  abstract onPrePull(pulls: Pull[]): void;

  abstract onPostPull(pullResult: IPullResult): void;

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

  onPrePullInitialize(pulls: Pull[]): void {
    const initializer = this.createRequest?.initializer;
    if (initializer) {
      const pull: Pull = {
        objectId: initializer.id,
        results: [{ name: '_initializer' }],
      };

      pulls.push(pull);
    }
  }

  onPostPullInitialize(pullResult: IPullResult): void {
    const initializer = this.createRequest?.initializer;
    if (initializer) {
      const initializerObject = pullResult.object<IObject>('_initializer');
      const propertyType = initializer.propertyType;
      if (propertyType.isAssociationType) {
        const associationType = propertyType as AssociationType;
        initializerObject.strategy.setCompositeRole(
          associationType.roleType,
          this.object
        );
      } else {
        const roleType = propertyType as RoleType;
        this.object.strategy.setCompositeRole(roleType, initializerObject);
      }
    }
  }

  ngOnDestroy(): void {
    this.formSubscription?.unsubscribe();
  }
}
