import {
  Directive,
  EventEmitter,
  HostListener,
  Input,
  OnDestroy,
  OnInit,
  Optional,
  Output,
} from '@angular/core';
import { Subject, Subscription } from 'rxjs';
import { throttleTime } from 'rxjs/operators';
import { ThrottledConfig } from './throttled-config';

const DEFAULT_THROTTLE_TIME = 1000;

@Directive({
  selector: '[throttled]',
})
export class ThrottledDirective implements OnInit, OnDestroy {
  @Input()
  throttleTime: number;

  @Output()
  throttleClick = new EventEmitter();

  private clicks = new Subject<MouseEvent>();
  private subscription: Subscription;

  constructor(@Optional() config: ThrottledConfig) {
    this.throttleTime ??= config?.time ?? DEFAULT_THROTTLE_TIME;
  }

  ngOnInit() {
    this.subscription = this.clicks
      .pipe(throttleTime(this.throttleTime))
      .subscribe((event) => this.emitThrottleClick(event));
  }

  emitThrottleClick(event: MouseEvent) {
    this.throttleClick.emit(event);
  }

  ngOnDestroy() {
    this.subscription.unsubscribe();
  }

  @HostListener('click', ['$event'])
  clickEvent(event: MouseEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.clicks.next(event);
  }
}
