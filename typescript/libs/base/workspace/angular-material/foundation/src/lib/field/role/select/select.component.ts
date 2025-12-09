import {
  Component,
  EventEmitter,
  Input,
  Optional,
  Output,
} from '@angular/core';
import { NgForm } from '@angular/forms';
import { IObject } from '@allors/system/workspace/domain';
import { RoleField } from '@allors/base/workspace/angular/foundation';

@Component({
  selector: 'a-mat-select',
  templateUrl: './select.component.html',
})
export class AllorsMaterialSelectComponent extends RoleField {
  @Input()
  public display = 'display';

  @Input()
  public options: IObject[];

  @Output()
  public selected: EventEmitter<IObject> = new EventEmitter();

  public get multiple(): boolean {
    return this.roleType.isMany;
  }

  constructor(@Optional() form: NgForm) {
    super(form);
  }

  public onModelChange(option: IObject): void {
    this.selected.emit(option);
  }

  onRestore(event: Event) {
    event.stopPropagation();
    this.restore();
    this.selected.emit(this.model);
  }
}
