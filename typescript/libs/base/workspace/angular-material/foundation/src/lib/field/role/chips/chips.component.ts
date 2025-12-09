import { Observable, BehaviorSubject, combineLatest } from 'rxjs';
import {
  debounceTime,
  distinctUntilChanged,
  switchMap,
  map,
  filter,
} from 'rxjs/operators';
import {
  Component,
  EventEmitter,
  Input,
  OnInit,
  Optional,
  Output,
  ViewChild,
  ElementRef,
  DoCheck,
} from '@angular/core';
import { NgForm, FormControl } from '@angular/forms';
import {
  MatAutocompleteTrigger,
  MatAutocompleteSelectedEvent,
} from '@angular/material/autocomplete';
import { IObject, TypeForParameter } from '@allors/system/workspace/domain';
import { RoleField } from '@allors/base/workspace/angular/foundation';

@Component({
  selector: 'a-mat-chips',
  templateUrl: './chips.component.html',
})
export class AllorsMaterialChipsComponent
  extends RoleField
  implements OnInit, DoCheck
{
  @Input() display = 'display';

  @Input() debounceTime = 400;

  @Input() options: IObject[];

  @Input() filter: (
    search: string,
    parameters?: { [id: string]: TypeForParameter }
  ) => Observable<IObject[]>;

  @Input() filterParameters: { [id: string]: TypeForParameter };

  @Output() changed: EventEmitter<IObject> = new EventEmitter();

  filteredOptions: Observable<IObject[]>;

  searchControl: FormControl = new FormControl();

  @ViewChild('searchInput') searchInput: ElementRef;

  @ViewChild(MatAutocompleteTrigger) private trigger: MatAutocompleteTrigger;

  private focused = false;

  focus$: BehaviorSubject<Date>;

  constructor(@Optional() form: NgForm) {
    super(form);

    this.focus$ = new BehaviorSubject<Date>(new Date());
  }

  ngOnInit(): void {
    if (this.filter) {
      this.filteredOptions = combineLatest([
        this.searchControl.valueChanges,
        this.focus$,
      ]).pipe(
        filter(([search]) => search != null && search.trim),
        debounceTime(this.debounceTime),
        distinctUntilChanged(),
        switchMap(([search]) => {
          if (this.filterParameters) {
            return this.filter(search, this.filterParameters);
          } else {
            return this.filter(search);
          }
        })
      );
    } else {
      this.filteredOptions = this.searchControl.valueChanges.pipe(
        filter((v) => v != null && v.trim),
        debounceTime(this.debounceTime),
        distinctUntilChanged(),
        map((search: string) => {
          const lowerCaseSearch = search.trim().toLowerCase();
          return this.options
            .filter((v: IObject) => {
              const optionDisplay: string = (v as any)[this.display]
                ? (v as any)[this.display].toString().toLowerCase()
                : undefined;
              if (optionDisplay) {
                return optionDisplay.indexOf(lowerCaseSearch) !== -1;
              }

              return false;
            })
            .sort((a: IObject, b: IObject) =>
              (a as any)[this.display] !== (b as any)[this.display]
                ? (a as any)[this.display] < (b as any)[this.display]
                  ? -1
                  : 1
                : 0
            );
        })
      );
    }
  }

  ngDoCheck() {
    if (!this.focused && this.trigger && this.searchControl) {
      if (!this.trigger.panelOpen) {
        const value = this.searchControl.value as IObject[];
        const model = this.model as IObject[];

        let differ = value == null || value.length !== model.length;
        if (!differ) {
          for (let i = 0; i < value.length; i++) {
            if (value[i] !== model[i]) {
              differ = true;
              break;
            }
          }
        }

        if (differ) {
          this.searchControl.setValue(this.model);
        }
      }
    }
  }

  inputBlur() {
    this.focused = false;
  }

  inputFocus() {
    this.focused = true;
    this.trigger._onChange('');
    this.focus$.next(new Date());
  }

  displayFn(): (val: IObject) => string {
    return (val: IObject) => {
      if (val) {
        return val ? (val as any)[this.display] : '';
      }
    };
  }

  optionSelected(event: MatAutocompleteSelectedEvent): void {
    this.add(event.option.value);
    this.changed.emit(this.model);

    this.searchControl.reset();
    this.searchInput.nativeElement.value = '';
  }

  clear(event: Event) {
    event.stopPropagation();
    this.model = undefined;
    this.trigger.closePanel();
    this.changed.emit(this.model);
  }
}
