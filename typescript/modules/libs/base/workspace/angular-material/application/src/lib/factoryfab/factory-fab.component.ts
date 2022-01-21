import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Composite, Class, humanize } from '@allors/workspace/meta/system';
import { IObject, ISession } from '@allors/workspace/domain/system';
import {
  AllorsComponent,
  angularDisplayName,
  angularIcon,
  CreateService,
  OnCreate,
} from '@allors/workspace/angular/base';

@Component({
  selector: 'a-mat-factory-fab',
  templateUrl: './factory-fab.component.html',
  styleUrls: ['./factory-fab.component.scss'],
})
export class FactoryFabComponent extends AllorsComponent implements OnInit {
  @Input() public session: ISession;

  @Input() public objectType: Composite;

  @Input() public onCreate?: OnCreate;

  @Output() public created?: EventEmitter<IObject> = new EventEmitter();

  classes: Class[];

  constructor(private readonly createService: CreateService) {
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
    this.createService
      .create(this.session, objectType, this.onCreate)
      .subscribe((v) => {
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
