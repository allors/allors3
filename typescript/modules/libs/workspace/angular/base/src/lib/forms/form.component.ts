import { Component, EventEmitter, Output } from '@angular/core';
import { NgForm } from '@angular/forms';

@Component({
  selector: 'a-form',
  template: `
    <form ngForm="form" novalidate submit="submit.emit(form)">
      <ng-content></ng-content>
    </form>
  `,
  providers: [
    {
      provide: NgForm,
    },
  ],
})
export class AllorsFormComponent {
  @Output()
  submit: EventEmitter<NgForm> = new EventEmitter<NgForm>();

  constructor(public form: NgForm) {}
}
