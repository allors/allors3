import { Component } from '@angular/core';

import { AllorsBarcodeService } from '../../../services/barcode/barcode.service';

@Component({
  // eslint-disable-next-line @angular-eslint/component-selector
  selector: 'a-mat-scanner',
  templateUrl: './scanner.component.html',
})
export class AllorsMaterialScannerComponent {
  showScanner = false;
  barcode: string | null;

  constructor(private barcodeService: AllorsBarcodeService) {}

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
