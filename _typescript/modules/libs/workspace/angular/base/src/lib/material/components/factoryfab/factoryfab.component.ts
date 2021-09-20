import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';

import { ObjectType, Composite, Class } from '@allors/workspace/meta/system';
import { IObject } from '@allors/workspace/domain/system';

import { ObjectData } from '../../services/object/object.data';
import { ObjectService } from '../../services/object/object.service';
import { WorkspaceService } from '@allors/workspace/angular/core';
import { IAngularMetaService } from '../../../meta/IAngularMetaService';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'a-mat-factory-fab',
  templateUrl: './factoryfab.component.html',
  styleUrls: ['./factoryfab.component.scss'],
})
export class FactoryFabComponent implements OnInit {
  @Input() private objectType: Composite;

  @Input() private createData: ObjectData;

  @Output() private created: EventEmitter<IObject> = new EventEmitter();

  classes: Class[];

  angularMeta: IAngularMetaService;

  constructor(public readonly factoryService: ObjectService, private workspaceService: WorkspaceService) {
    this.angularMeta = this.workspaceService.workspace.services.angularMetaService;
  }

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
}
