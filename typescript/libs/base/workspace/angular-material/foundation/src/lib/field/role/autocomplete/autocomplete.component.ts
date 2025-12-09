import { Observable } from 'rxjs';
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
  DoCheck,
} from '@angular/core';
import { FormControl, NgForm } from '@angular/forms';
import {
  MatAutocompleteTrigger,
  MatAutocompleteSelectedEvent,
} from '@angular/material/autocomplete';
import { IObject, TypeForParameter } from '@allors/system/workspace/domain';
import { RoleField } from '@allors/base/workspace/angular/foundation';

@Component({
  selector: 'a-mat-autocomplete',
  templateUrl: './autocomplete.component.html',
})
export class AllorsMaterialAutocompleteComponent
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

  @ViewChild(MatAutocompleteTrigger) private trigger: MatAutocompleteTrigger;

  private focused = false;

  get displayProperty(): string {
    return this.model ? this.model[this.display] : '';
  }

  constructor(@Optional() form: NgForm) {
    super(form);
  }

  public ngOnInit(): void {
    if (this.filter) {
      this.filteredOptions = this.searchControl.valueChanges.pipe(
        filter((v) => v != null && v.trim),
        debounceTime(this.debounceTime),
        distinctUntilChanged(),
        switchMap((search: string) => {
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
                : null;
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
      if (!this.trigger.panelOpen && this.searchControl.value !== this.model) {
        this.searchControl.setValue(this.model);
      }
    }
  }

  inputBlur() {
    this.focused = false;
  }

  inputFocus() {
    this.focused = true;
    if (!this.model) {
      this.trigger._onChange('');
    }
  }

  displayFn(): (val: IObject) => string {
    return (val: IObject) => (val ? (val as any)[this.display] : '');
  }

  optionSelected(event: MatAutocompleteSelectedEvent): void {
    this.model = event.option.value;
    this.changed.emit(this.model);
  }

  onRestore(event: Event) {
    event.stopPropagation();
    this.restore();
    this.trigger.closePanel();
    this.changed.emit(this.model);
  }

  onClear(event: Event) {
    event.stopPropagation();
    this.model = undefined;
    this.trigger.closePanel();
    this.changed.emit(this.model);
  }
}
