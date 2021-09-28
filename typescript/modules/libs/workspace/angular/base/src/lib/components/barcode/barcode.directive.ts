import { Directive, HostListener } from '@angular/core';

import { AllorsBarcodeService } from '../../services/barcode/barcode.service';

@Directive({
  // tslint:disable-next-line:directive-selector
  selector: '[aBarcode]',
})
export class AllorsBarcodeDirective {
  constructor(private barcodeService: AllorsBarcodeService) {}

  @HostListener('document:keypress', ['$event'])
  onKeypress(event: KeyboardEvent) {
    this.barcodeService.onKeypress(event);
  }
}
