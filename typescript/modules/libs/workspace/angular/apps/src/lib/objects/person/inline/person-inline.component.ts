import { Component, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';

import { M } from '@allors/workspace/meta/default';
import { Locale, Person, Enumeration } from '@allors/workspace/domain/default';
import { ContextService } from '@allors/workspace/angular/core';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'person-inline',
  templateUrl: './person-inline.component.html',
})
export class PersonInlineComponent implements OnInit, OnDestroy {
  @Output()
  public saved: EventEmitter<Person> = new EventEmitter<Person>();

  @Output()
  public cancelled: EventEmitter<any> = new EventEmitter();

  public person: Person;

  public m: M;

  public locales: Locale[];
  public genders: Enumeration[];
  public salutations: Enumeration[];

  constructor(private allors: ContextService) {
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m; const { pullBuilder: pull } = m;

    const pulls = [
      pull.Locale({
        sorting: [{ roleType: this.m.Locale.Name }],
      }),
      pull.GenderType({
        predicate: { kind: 'Equals', propertyType: this.m.GenderType.IsActive, value: true },
        sorting: [{ roleType: this.m.GenderType.Name }],
      }),
      pull.Salutation({
        predicate: { kind: 'Equals', propertyType: this.m.Salutation.IsActive, value: true },
        sorting: [{ roleType: this.m.Salutation.Name }],
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      this.locales = loaded.collection<Locale>(m.Locale);
      this.genders = loaded.collection<Enumeration>(m.Enumeration);
      this.salutations = loaded.collection<Enumeration>(m.Enumeration);

      this.person = this.allors.context.create<Person>(m.Person);
    });
  }

  public ngOnDestroy(): void {
    if (this.person) {
      this.person.strategy.delete();
    }
  }

  public cancel(): void {
    this.cancelled.emit();
  }

  public save(): void {
    this.saved.emit(this.person);
    this.person = undefined;
  }
}