import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Class, humanize } from '@allors/system/workspace/meta';
import { IObject } from '@allors/system/workspace/domain';
import {
  AllorsComponent,
  angularDisplayName,
} from '@allors/base/workspace/angular/foundation';
import {
  CreateData,
  CreateService,
  OnCreate,
} from '@allors/base/workspace/angular/application';
import { angularIcon } from '../meta/angular-icon';

@Component({
  selector: 'a-mat-factory-fab',
  templateUrl: './factory-fab.component.html',
  styleUrls: ['./factory-fab.component.scss'],
})
export class FactoryFabComponent extends AllorsComponent implements OnInit {
  @Input() public createData: CreateData;

  @Input() public onCreate?: OnCreate;

  @Output() public created?: EventEmitter<IObject> = new EventEmitter();

  classes: Class[];

  constructor(private readonly createService: CreateService) {
    super();
  }

  ngOnInit(): void {
    if (this.createData.objectType.isInterface) {
      this.classes = [...this.createData.objectType.classes];
    } else {
      this.classes = [this.createData.objectType as Class];
    }

    this.classes = this.classes.filter((v) => this.createService.canCreate(v));
  }

  get dataAllorsActions(): string {
    return this.classes ? this.classes.map((v) => v.singularName).join() : '';
  }

  create(objectType: Class) {
    this.createService.create(objectType, this.onCreate).subscribe((v) => {
      if (v && this.created) {
        this.created.next(v);
      }
    });
  }

  icon(cls: Class): string {
    return angularIcon(cls);
  }

  displayName(cls: Class): string {
    return angularDisplayName(cls) ?? humanize(cls.singularName);
  }
}
