import { Component } from '@angular/core';
import {
  AllorsBarcodeService,
  AllorsComponent,
} from '@allors/base/workspace/angular/foundation';

@Component({
  selector: 'a-mat-scanner',
  templateUrl: './scanner.component.html',
})
export class AllorsMaterialScannerComponent extends AllorsComponent {
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
