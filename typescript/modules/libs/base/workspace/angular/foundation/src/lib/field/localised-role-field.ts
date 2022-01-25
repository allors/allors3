import { Input, Directive } from '@angular/core';
import { assert, humanize } from '@allors/system/workspace/meta';
import { M } from '@allors/default/workspace/meta';
import { Locale, LocalisedText } from '@allors/default/workspace/domain';

import { RoleField } from './role-field';

@Directive()
// tslint:disable-next-line: directive-class-suffix
export abstract class LocalisedRoleField extends RoleField {
  @Input()
  public locale: Locale;

  get localisedObject(): LocalisedText | null {
    if (this.locale) {
      const all: LocalisedText[] = this.model;
      if (all) {
        const filtered: LocalisedText[] = all.filter(
          (v: LocalisedText) => v.Locale === this.locale
        );
        return filtered?.[0] ?? null;
      }
    }

    return null;
  }

  get localisedText(): string | null {
    if (this.locale) {
      return this.localisedObject?.Text ?? null;
    }

    return null;
  }

  set localisedText(value: string | null) {
    if (this.locale) {
      if (!this.localisedObject) {
        const m = this.roleType.relationType.metaPopulation as M;
        const localisedText: LocalisedText =
          this.object.strategy.session.create<LocalisedText>(m.LocalisedText);
        localisedText.Locale = this.locale;
        this.object.strategy.addCompositesRole(this.roleType, localisedText);
      }

      assert(this.localisedObject);
      this.localisedObject.Text = value;
    }
  }

  get localisedName(): string {
    if (this.locale) {
      return this.name + '_' + this.locale.Name;
    }

    return null;
  }

  get localisedLabel(): string {
    if (this.locale) {
      let name = this.roleType.name;
      const localised = 'Localised';
      if (name.indexOf(localised) === 0) {
        name = name.slice(localised.length);
        name = name.slice(0, name.length - 1);
      }

      const label = this.assignedLabel ? this.assignedLabel : humanize(name);
      return label + ' (' + this.locale.Language?.Name + ')';
    }

    return null;
  }
}
