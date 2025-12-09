import { ActivatedRoute } from '@angular/router';
import { Composite } from '@allors/system/workspace/meta';

export class NavigationActivatedRoute {
  constructor(private activatedRoute: ActivatedRoute) {}

  id(): number | null {
    const id = this.activatedRoute.snapshot.paramMap.get('id');
    return id != null ? parseInt(id) : null;
  }

  composite(): string | null {
    const url = this.activatedRoute.snapshot.url;
    const path = url[url.length - 2].path;
    return path;
  }

  panel(): string | null {
    const queryParamMap = this.activatedRoute.snapshot.queryParamMap;
    return queryParamMap.get('panel');
  }

  queryParam(objectType: Composite): string | null {
    const queryParamMap = this.activatedRoute.snapshot.queryParamMap;
    // TODO: Optimize ...objectType.classes
    const match = [...objectType.classes].find((v) =>
      queryParamMap.has(v.singularName)
    );
    return match ? queryParamMap.get(match.singularName) : null;
  }
}
