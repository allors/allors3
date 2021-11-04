import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';

import { ObjectType, Composite, Class, humanize } from '@allors/workspace/meta/system';
import { IObject } from '@allors/workspace/domain/system';

import { ObjectData } from '../../services/object/object.data';
import { ObjectService } from '../../services/object/object.service';
import { WorkspaceService } from '@allors/workspace/angular/core';


@Component({
  // tslint:disable-next-line:component-selector
  selector: 'a-mat-factory-fab',
  templateUrl: './factory-fab.component.html',
  styleUrls: ['./factory-fab.component.scss'],
})
export class FactoryFabComponent implements OnInit {
  @Input() public objectType: Composite;

  @Input() public createData: ObjectData;

  @Output() public created: EventEmitter<IObject> = new EventEmitter();

  classes: Class[];

  constructor(public readonly factoryService: ObjectService, private workspaceService: WorkspaceService) {}

  ngOnInit(): void {
    if (this.objectType.isInterface) {
      this.classes = [...this.objectType.classes];
    } else {
      this.classes = [this.objectType as Class];
    }

    const session = this.workspaceService.workspace.createSession();
    this.classes = this.classes.filter((v) => this.factoryService.hasCreateControl(v, this.createData, session));
  }

  get dataAllorsActions(): string {
    return this.classes ? this.classes.map((v) => v.singularName).join() : '';
  }

  create(objectType: ObjectType) {
    this.factoryService.create(objectType, this.createData).subscribe((v) => {
      if (v && this.created) {
        this.created.next(v);
      }
    });
  }

  displayName(cls: Class): string {
    return cls._.displayName ?? humanize(cls.singularName);
  }
}
