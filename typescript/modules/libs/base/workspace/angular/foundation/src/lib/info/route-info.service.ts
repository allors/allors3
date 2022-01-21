import { Injectable } from '@angular/core';
import { Route, Router } from '@angular/router';

export interface RouteInfo {
  path?: string;
  pathMatch?: string;
  component?: string;
  redirectTo?: string;
  children: RouteInfo[];
}

@Injectable()
export class RouteInfoService {
  constructor(private router: Router) {}

  write(allors: { [key: string]: unknown }) {
    allors['route'] = this.route;
  }

  private get route(): string {
    const routeMapper = (v: Route) => {
      return {
        path: v.path,
        pathMatch: v.pathMatch,
        component: v.component?.name,
        redirectTo: v.redirectTo,
        children: v.children?.map(routeMapper),
      };
    };

    const routes = this.router.config;
    const routeInfos: RouteInfo[] = routes.map(routeMapper);

    return JSON.stringify(routeInfos);
  }
}
