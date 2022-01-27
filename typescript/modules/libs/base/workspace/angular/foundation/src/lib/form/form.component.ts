import { HostBinding, Directive, EventEmitter, Output } from '@angular/core';
import { NgForm } from '@angular/forms';
import { M } from '@allors/default/workspace/meta';
import { IObject, Pull } from '@allors/system/workspace/domain';
import { AllorsComponent } from '../component';
import { AllorsForm } from './form';
import { Context } from '../context/context';
import { ContextService } from '../context/context-service';
import { ErrorService } from '../error/error.service';
import { CreateRequest } from '../create/create-request';
import { EditRequest } from '../edit/edit-request';
import { Subscription } from 'rxjs';

@Directive()
export abstract class AllorsFormComponent<T extends IObject>
  extends AllorsComponent
  implements AllorsForm
{
  override dataAllorsKind = 'form';

  @HostBinding('attr.data-allors-id')
  get dataAllorsId() {
    return this.object?.strategy.id;
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

  private subscription: Subscription;

  get canSave() {
    return this.form.form.valid && this.context.hasChanges();
  }

  get canWrite(): any {
    return true;
  }

  create(request: CreateRequest): void {
    if (request.arguments?.length) {
      const pulls = request.arguments.map(v=>);
      for (const arg of request.arguments) {
      }

      p.Employment({
        objectId,
        include: {
          Employee: {},
          Employer: {},
        },
      });

      this.subscription?.unsubscribe();
      this.subscription = this.context.pull(pulls).subscribe((loaded) => {
        this.object = this.context.create<T>(request.objectType);
        this.object.FromDate = new Date();

        this.object.Employer = this.organisation;
        this.object.Employee = this.person;
      });
    } else {
      this.object = this.context.create<T>(request.objectType);
    }
  }

  edit(request: EditRequest): void {
    const m = this.m;
    const { pullBuilder: p } = m;

    const pull = p.Employment({
      objectId: request.object.id,
      include: {
        Employee: {},
        Employer: {},
      },
    });

    this.subscription?.unsubscribe();
    this.subscription = this.context.pull(pull).subscribe((loaded) => {
      this.object = loaded.objects.values().next()?.value;
    });
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
}
