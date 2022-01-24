import { Subscription } from 'rxjs';
import { switchMap, filter } from 'rxjs/operators';
import { Component, OnInit, Self, OnDestroy } from '@angular/core';
import {
  Enumeration,
  Gender,
  Locale,
  Person,
} from '@allors/default/workspace/domain';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import {
  RefreshService,
  SaveService,
} from '@allors/base/workspace/angular/foundation';
import {
  AllorsPanelDetailComponent,
  NavigationService,
  PanelService,
} from '@allors/base/workspace/angular/application';
@Component({
  selector: 'person-detail',
  templateUrl: './person-detail.component.html',
  providers: [PanelService, ContextService],
})
export class PersonDetailComponent
  extends AllorsPanelDetailComponent<Person>
  implements OnInit, OnDestroy
{
  emailAddresses: string[] = [];
  locales: Locale[];
  genders: Enumeration[];
  salutations: Enumeration[];
  confirmPassword: string;

  private subscription: Subscription;

  constructor(
    @Self() allors: ContextService,
    @Self() panel: PanelService,
    public refreshService: RefreshService,
    public navigationService: NavigationService,
    private saveService: SaveService
  ) {
    super(allors, panel);

    // Collapsed
    const pullName = `${this.panel.name}_${this.m.Person.tag}`;

    panel.onPull = (pulls) => {
      this.object = undefined;

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
        this.object = loaded.object<Person>(pullName);
      }
    };
  }

  public ngOnInit(): void {
    // Maximized
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = this.panel.manager.on$
      .pipe(
        filter(() => {
          return this.panel.isExpanded;
        }),
        switchMap(() => {
          this.object = undefined;
          const id = this.panel.manager.id;

          const pulls = [
            pull.Person({
              objectId: id,
              include: {
                Gender: x,
                Locale: x,
              },
            }),
            pull.Locale({}),
            pull.Gender({}),
          ];

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.object = loaded.object<Person>(m.Person);
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
