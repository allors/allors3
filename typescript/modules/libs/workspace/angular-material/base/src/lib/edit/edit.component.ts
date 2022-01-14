import { combineLatest, Subscription, switchMap } from 'rxjs';
import { HostBinding, Directive, OnInit, OnDestroy } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { M } from '@allors/workspace/meta/default';
import { ContextService } from '@allors/workspace/angular/core';
import { IObject, IPullResult, Pull } from '@allors/workspace/domain/system';
import {
  AllorsComponent,
  ObjectData,
  RefreshService,
  SaveService,
} from '@allors/workspace/angular/base';

@Directive()
export abstract class AllorsEditComponent<
    T extends IObject,
    U extends AllorsEditComponent<T, U>
  >
  extends AllorsComponent
  implements OnInit, OnDestroy
{
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

  get canWrite() {
    return true;
  }

  get isCreate(): boolean {
    return this.data.id == null;
  }

  get isEdit(): boolean {
    return this.data.id != null && this.canWrite;
  }

  get isView(): boolean {
    return this.data.id != null && !this.canWrite;
  }

  m: M;

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

    this.onTitle();
  }

  public ngOnInit(): void {
    this.subscription = combineLatest([this.refreshService.refresh$])
      .pipe(
        switchMap(() => {
          const pulls = [];
          this.onPull(pulls);
          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.onPulled(loaded);
      });
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

  abstract onPull(pulls: Pull[]): void;

  abstract onPulled(loaded: IPullResult);

  protected onTitle() {
    // TODO: add to configure
    const name = this.object?.strategy.cls.singularName;
    if (this.isCreate) {
      this.title = `Add ${name}`;
    } else {
      if (this.object) {
        if (this.canWrite) {
          this.title = `Edit ${name}`;
        } else {
          this.title = `View ${name}`;
        }
      }
    }
  }
}
