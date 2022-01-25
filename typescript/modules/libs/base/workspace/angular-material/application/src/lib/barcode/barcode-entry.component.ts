import { Component } from '@angular/core';
import {
  AllorsBarcodeService,
  AllorsComponent,
} from '@allors/base/workspace/angular/foundation';

@Component({
  selector: 'a-mat-barcode-entry',
  templateUrl: './barcode-entry.component.html',
})
export class AllorsMaterialBarcodeEntryComponent extends AllorsComponent {
  showScanner = false;
  barcode: string | null;

  constructor(private barcodeService: AllorsBarcodeService) {
    super();
  }

  scan() {
    try {
      if (this.barcode) {
        this.barcodeService.scan(this.barcode);
      }
    } finally {
      this.barcode = null;
      this.showScanner = false;
    }
  }
}
