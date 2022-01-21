import { Component } from '@angular/core';

@Component({
  templateUrl: './error.component.html',
})
export class ErrorComponent {
  restart() {
    location.href = '/';
  }
}
