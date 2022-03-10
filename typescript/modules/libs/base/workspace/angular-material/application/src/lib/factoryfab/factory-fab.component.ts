import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Class, Composite } from '@allors/system/workspace/meta';
import { IObject, Initializer } from '@allors/system/workspace/domain';
import {
  AllorsComponent,
  CreateRequest,
  CreateService,
  MetaService,
} from '@allors/base/workspace/angular/foundation';
import { IconService } from '../icon/icon.service';

@Component({
  selector: 'a-mat-factory-fab',
  templateUrl: './factory-fab.component.html',
  styleUrls: ['./factory-fab.component.scss'],
})
export class FactoryFabComponent extends AllorsComponent implements OnInit {
  @Input() public objectType: Composite;

  @Input() public initializer: Initializer;

  @Output() public created?: EventEmitter<IObject> = new EventEmitter();

  classes: Class[];

  constructor(
    private readonly createService: CreateService,
    private iconService: IconService,
    private metaService: MetaService
  ) {
    super();
  }

  ngOnInit(): void {
    if (this.objectType.isInterface) {
      this.classes = [...this.objectType.classes];
    } else {
      this.classes = [this.objectType as Class];
    }

    this.classes = this.classes.filter((v) => this.createService.canCreate(v));
  }

  get dataAllorsActions(): string {
    return this.classes ? this.classes.map((v) => v.singularName).join() : '';
  }

  create(objectType: Class) {
    const request: CreateRequest = {
      kind: 'CreateRequest',
      objectType,
      initializer: this.initializer,
    };

    this.createService.create(request).subscribe((v) => {
      if (v && this.created) {
        this.created.next(v);
      }
    });
  }

  icon(cls: Class): string {
    return this.iconService.icon(cls);
  }

  displayName(cls: Class): string {
    return this.metaService.singularName(cls);
  }
}
