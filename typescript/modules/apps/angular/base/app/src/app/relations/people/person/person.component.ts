import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { TestScope } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { Person, Locale } from '@allors/workspace/domain/default';
import { IPullResult, Pull } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';

@Component({
  templateUrl: './person.component.html',
  providers: [ContextService],
})
export class PersonComponent extends TestScope implements OnInit, OnDestroy {
  public title: string;

  public m: M;
  public locales: Locale[];
  public person: Person;

  private subscription: Subscription;

  constructor(@Self() private allors: ContextService, private titleService: Title, private route: ActivatedRoute) {
    super();

    this.title = 'Person';
    this.titleService.setTitle(this.title);

    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const { pullBuilder: p } = this.m;

    this.subscription = this.route.url
      .pipe(
        switchMap(() => {
          const id = this.route.snapshot.paramMap.get('id');

          const pulls: Pull[] = [
            p.Person({
              objectId: id ?? '',
              include: {
                Photo: {},
                Pictures: {},
              },
            }),
            p.Locale({}),
          ];
          this.allors.context.reset();
          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded: IPullResult) => {
        this.person = loaded.object<Person>(this.m.Person) ?? this.allors.context.create<Person>(this.m.Person);
        this.locales = loaded.collection<Locale>(this.m.Locale);
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.goBack();
    });
  }

  public goBack(): void {
    window.history.back();
  }
}
