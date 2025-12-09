import { Component, EventEmitter, Input, Output } from '@angular/core';
import { AllorsComponent } from '@allors/base/workspace/angular/foundation';
import { PageEvent } from '@angular/material/paginator';

@Component({
  selector: 'a-mat-paginator',
  templateUrl: './paginator.component.html',
  styleUrls: ['./paginator.component.scss'],
})
export class AllorsMaterialPaginatorComponent extends AllorsComponent {
  @Input()
  pageIndex = 0;

  @Input()
  pageSize: number;

  @Input()
  pageSizeOptions: number[];

  @Input()
  pageFill: number;

  @Output()
  page: EventEmitter<PageEvent> = new EventEmitter();

  get isFirst() {
    return this.pageIndex === 0;
  }

  get isLast() {
    return this.pageFill < this.pageSize;
  }

  get of() {
    return this.isLast ? `${this.pageIndex + 1}` : `many`;
  }

  first() {
    this.pageIndex = 0;
    this.emitPage();
  }

  previous() {
    --this.pageIndex;
    this.emitPage();
  }

  next() {
    ++this.pageIndex;
    this.emitPage();
  }

  pageSizeChange() {
    this.pageIndex = 0;
    this.emitPage();
  }

  private emitPage() {
    const pageEvent = new PageEvent();
    pageEvent.pageIndex = this.pageIndex;
    pageEvent.pageSize = this.pageSize;
    this.page.emit(pageEvent);
  }
}
