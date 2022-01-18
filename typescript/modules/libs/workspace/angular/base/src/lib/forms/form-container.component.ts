import { Component } from '@angular/core';
import { NgForm } from '@angular/forms';

@Component({
  selector: 'a-form-container',
  template: `
    <form ngForm="form" novalidate (submit)="$event.preventDefault()">
      <ng-content></ng-content>
    </form>
  `,
  providers: [
    {
      provide: NgForm,
    },
  ],
})
export class AllorsFormContainerComponent {}
