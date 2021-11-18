import { TestScope } from '@allors/workspace/angular/base';
import { Component } from '@angular/core';

@Component({
  templateUrl: './error.component.html',
})
export class ErrorComponent extends TestScope {
  restart() {
    location.href = '/';
  }
}
