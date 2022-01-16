import { Component, Self } from '@angular/core';
import { Country } from '@allors/workspace/domain/default';
import { FormService, SaveService } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { AllorsFormComponent } from '@allors/workspace/angular-material/base';
import { switchMap, tap } from 'rxjs';
import { NgForm } from '@angular/forms';

@Component({
  selector: 'country-form',
  templateUrl: 'country-form.component.html',
  providers: [ContextService],
})
export class CountryFormComponent extends AllorsFormComponent<Country> {
  constructor(
    @Self() allors: ContextService,
    form: NgForm,
    formService: FormService,
    saveService: SaveService
  ) {
    super(allors, form, formService);

    this.create$
      .pipe(
        tap((objectType) => {
          this.object = this.context.create<Country>(objectType);
        })
      )
      .subscribe();

    this.edit$
      .pipe(
        switchMap((objectId) => {
          const { pullBuilder: p } = this.m;
          return this.context.pull(
            p.Country({
              objectId,
            })
          );
        })
      )
      .subscribe((loaded) => {
        this.object = loaded.object<Country>(this.m.Country);
      });

    this.save$
      .pipe(
        switchMap(() => {
          return this.context.session.push();
        })
      )
      .subscribe({
        next: () => {
          this.formService.saved(this.object);
        },
        error: (error) => {
          saveService.errorHandler(error);
        },
      });

    this.cancel$.subscribe(() => this.formService.cancelled(this.object));
  }
}
