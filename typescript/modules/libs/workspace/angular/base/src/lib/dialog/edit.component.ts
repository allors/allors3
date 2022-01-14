import { Subscription } from 'rxjs';
import { HostBinding, Directive } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { M } from '@allors/workspace/meta/default';
import { ContextService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';
import { RefreshService } from '../refresh/refresh.service';
import { ObjectData } from '../object/object.data';
import { SaveService } from '../save/save.service';
import { AllorsComponent } from '../component';

@Directive()
export abstract class AllorsEditComponent<
  T extends IObject,
  U extends AllorsEditComponent<T, U>
> extends AllorsComponent {
  dataAllorsKind = 'edit';

  @HostBinding('attr.data-allors-id')
  get dataAllorsId() {
    return this.object?.strategy.id;
  }

  get object(): T {
    return this._object;
  }

  set object(value: T) {
    this._object = value;
    this.onTitle();
  }

  get canEdit() {
    return true;
  }

  m: M;

  isCreate: boolean;

  title: string;

  protected subscription: Subscription;

  private _object: T;

  constructor(
    public allors: ContextService,
    public data: ObjectData,
    public dialogRef: MatDialogRef<U>,
    public refreshService: RefreshService,
    public saveService: SaveService
  ) {
    super();

    this.m = this.allors.context.configuration.metaPopulation as M;
    this.allors.context.name = this.constructor.name;

    this.isCreate = this.data.id == null;
    this.onTitle();
  }

  ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  save(): void {
    this.allors.context.push().subscribe({
      complete: () => {
        this.dialogRef.close(this.object);
        this.refreshService.refresh();
      },
      error: this.saveService.errorHandler,
    });
  }

  protected onTitle() {
    // TODO: add to configure
    const name = this.object?.strategy.cls.singularName;
    if (this.isCreate) {
      this.title = `Add ${name}`;
    } else {
      if (this.object) {
        if (this.canEdit) {
          this.title = `Edit ${name}`;
        } else {
          this.title = `View ${name}`;
        }
      }
    }
  }
}
