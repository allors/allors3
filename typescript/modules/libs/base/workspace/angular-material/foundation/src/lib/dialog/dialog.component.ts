import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Component, Inject } from '@angular/core';
import {
  AllorsComponent,
  PromptType,
} from '@allors/base/workspace/angular/foundation';
import { AllorsMaterialDialogData } from './dialog.data';

@Component({
  templateUrl: 'dialog.component.html',
})
export class AllorsMaterialDialogComponent extends AllorsComponent {
  public alert: boolean | undefined;
  public confirmation: boolean | undefined;
  public prompt: boolean | undefined;
  public promptType: PromptType;

  public title: string | undefined;
  public message: string | undefined;
  public label: string | undefined;
  public placeholder: string | undefined;
  public value: string;

  constructor(
    public dialogRef: MatDialogRef<AllorsMaterialDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: AllorsMaterialDialogData
  ) {
    super();

    this.alert = data.alert;
    this.confirmation = data.confirmation;
    this.prompt = data.prompt;

    const config = data.config;
    this.title = config.title;
    this.message = config.message;
    this.label = config.label;
    this.placeholder = config.placeholder;
    this.promptType = config.promptType || 'string';
  }
}
