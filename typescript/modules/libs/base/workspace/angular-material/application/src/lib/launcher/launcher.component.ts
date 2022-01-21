import { Component, OnDestroy, OnInit } from '@angular/core';
import {
  AllorsComponent,
  PanelManagerService,
} from '@allors/workspace/angular/base';

@Component({
  selector: 'a-mat-launcher',
  templateUrl: './launcher.component.html',
  styleUrls: ['./launcher.component.scss'],
})
export class AllorsMaterialLauncherComponent
  extends AllorsComponent
  implements OnInit, OnDestroy
{
  get panels() {
    return this.panelsService.panels.filter((v) => v.expandable);
  }

  constructor(public panelsService: PanelManagerService) {
    super();
  }

  public ngOnInit(): void {}

  public ngOnDestroy(): void {}
}
