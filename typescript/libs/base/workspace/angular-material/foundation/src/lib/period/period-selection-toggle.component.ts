import { Component, EventEmitter, Input, Output } from '@angular/core';

export enum PeriodSelection {
  Current = 'Current',
  Inactive = 'Inactive',
  All = 'All',
}

@Component({
  selector: 'a-mat-period-selection-toggle',
  templateUrl: './period-selection-toggle.component.html',
})
export class AllorsMaterialPeriodSelectionToggleComponent {
  @Input() periodSelection: PeriodSelection;

  @Output() periodSelectionChange = new EventEmitter<PeriodSelection>();

  readonly Current = PeriodSelection.Current;
  readonly Inactive = PeriodSelection.Inactive;
  readonly All = PeriodSelection.All;
}
