import { Component, OnInit, Self, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { switchMap, filter } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { Enumeration, Gender, Locale, Person } from '@allors/workspace/domain/default';
import { NavigationService, PanelService, RefreshService, SaveService, SingletonId } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';

@Component({
  // eslint-disable-next-line @angular-eslint/component-selector
  selector: 'person-overview-detail',
  templateUrl: './person-overview-detail.component.html',
  providers: [PanelService, ContextService],
})
export class PersonOverviewDetailComponent implements OnInit, OnDestroy {
  readonly m: M;

  person: Person;
  emailAddresses: string[] = [];

  locales: Locale[];
  genders: Enumeration[];
  salutations: Enumeration[];
  public confirmPassword: string;

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    @Self() public panel: PanelService,
    public refreshService: RefreshService,
    public navigationService: NavigationService,
    private saveService: SaveService,
    private singletonId: SingletonId
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;

    panel.name = 'detail';
    panel.title = 'Personal Data';
    panel.icon = 'person';
    panel.expandable = true;

    // Minimized
    const pullName = `${this.panel.name}_${this.m.Person.tag}`;

    panel.onPull = (pulls) => {
      this.person = undefined;

      if (this.panel.isCollapsed) {
        const m = this.m;
        const { pullBuilder: pull } = m;
        const x = {};
        const id = this.panel.manager.id;

        pulls.push(
          pull.Person({
            name: pullName,
            objectId: id,
            include: {
              Locale: x,
              Gender: x,
            },
          })
        );
      }
    };

    panel.onPulled = (loaded) => {
      if (this.panel.isCollapsed) {
        this.person = loaded.object<Person>(pullName);
      }
    };
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    // Maximized
    this.subscription = this.panel.manager.on$
      .pipe(
        filter(() => {
          return this.panel.isExpanded;
        }),
        switchMap(() => {
          this.person = undefined;
          const id = this.panel.manager.id;

          const pulls = [
            pull.Person({
              objectId: id,
              include: {
                Gender: x,
                Locale: x,
              },
            }),
          ];

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.person = loaded.object<Person>(m.Person);
        this.genders = loaded.collection<Gender>(m.Gender);
        this.locales = loaded.collection<Locale>(m.Locale) || [];
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.refreshService.refresh();
      this.panel.toggle();
    }, this.saveService.errorHandler);
  }
}
