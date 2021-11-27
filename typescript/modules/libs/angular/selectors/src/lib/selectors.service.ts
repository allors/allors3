import { Injectable } from '@angular/core';

export interface DebuggingAPI {
  getComponent(node: Node): any;
  getDirectives(node: Node): any[];
  getHostElement(cmp: any): HTMLElement;
}

declare let ng: DebuggingAPI;

@Injectable({
  providedIn: 'root',
})
export class SelectorsService {
  query(root: Element, selector: string) {
    const component = ng.getComponent(root);
    
    return root.querySelector(selector);
  }

  queryAll(root: Element, selector: string) {
    return Array.from(root.querySelectorAll(selector));
  }
}
