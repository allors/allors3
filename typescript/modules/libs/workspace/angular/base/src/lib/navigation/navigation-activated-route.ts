import { ActivatedRoute } from '@angular/router';
import { Composite } from '@allors/workspace/meta/system';

export class NavigationActivatedRoute {
  constructor(private activatedRoute: ActivatedRoute) {}

  id(): number | null {
    const id = this.activatedRoute.snapshot.paramMap.get('id');
    return id != null ? parseInt(id) : null;
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
