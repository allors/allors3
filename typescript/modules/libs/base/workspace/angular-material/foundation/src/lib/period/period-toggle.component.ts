import { Component, EventEmitter, Input, Output } from '@angular/core';

export type PeriodToggle = 'Current' | 'Inactive' | 'All';

@Component({
  selector: 'a-mat-period-toggle',
  templateUrl: './period-toggle.component.html',
})
export class AllorsMaterialPeriodToggleComponent {
  @Input() selection: PeriodToggle;

  @Output() selectionChange = new EventEmitter<PeriodToggle>();
}
