import { Injectable } from '@angular/core';
import { MenuItem } from './menu-item';

@Injectable()
export abstract class MenuService {
  abstract menu(): MenuItem[];
}
