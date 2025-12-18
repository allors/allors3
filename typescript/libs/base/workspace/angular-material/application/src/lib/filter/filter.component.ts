import { Component, ViewChild, ElementRef, Input } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import {
  AllorsComponent,
  Filter,
  FilterField,
} from '@allors/base/workspace/angular/foundation';
import { AllorsMaterialFilterFieldDialogComponent } from './field/dialog.component';

@Component({
  selector: 'a-mat-filter',
  templateUrl: './filter.component.html',
})
export class AllorsMaterialFilterComponent extends AllorsComponent {
  @Input() filter: Filter;

  @ViewChild('toolBar', { static: true }) toolBar: ElementRef;

  constructor(private dialog: MatDialog) {
    super();
  }

  get hasFields() {
    return this.filter?.fields?.length > 0 ?? false;
  }

  clear() {
    this.filter.clearFields();
  }

  remove(field: FilterField) {
    this.filter.removeField(field);
  }

  add(event: MouseEvent) {
    const { offsetTop, offsetLeft, offsetWidth } = this.toolBar.nativeElement;

    const top = offsetTop;
    const twoThirds = offsetLeft + (offsetWidth * 2) / 3;

    let left =
      event.pageX < offsetLeft + 60 ? event.pageX - 20 : event.pageX - 40;
    left = left < twoThirds ? left : twoThirds;

    this.dialog.open(AllorsMaterialFilterFieldDialogComponent, {
      data: { filterBuilder: this.filter },
      autoFocus: true,
      backdropClass: 'nada',
      position: {
        top: `${top}px`,
        left: `${left}px`,
      },
      maxHeight: '90vh',
    });
  }
}
