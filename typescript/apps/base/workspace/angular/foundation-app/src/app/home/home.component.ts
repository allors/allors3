import { Component, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';

@Component({
  templateUrl: './home.component.html',
})
export class HomeComponent implements OnInit {
  throttledCounter: number;

  constructor(private title: Title) {
    this.throttledCounter = 0;
  }

  public ngOnInit() {
    this.title.setTitle('Home');
  }

  throttleClick() {
    this.throttledCounter += 1;
  }
}
