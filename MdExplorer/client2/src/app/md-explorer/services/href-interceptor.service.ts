import { Injectable } from '@angular/core';

export interface IWorkWithElement {
  (element: any): void;
}

@Injectable({
  providedIn: 'root'
})
export class HrefInterceptorService {
  private _workWithElement: IWorkWithElement;
  private _name: string;

  constructor() {    
    document.onclick = this.interceptHref;    
    this._name = 'test';
  }

  setCallback(workWithElement: IWorkWithElement) {
    debugger;
    this._workWithElement = workWithElement;
    
  }

  interceptHref(_event) {
    const tEvent = _event || window.event;

    const element = tEvent.target || tEvent.srcElement;
    if (element.tagName === 'A'
     // && element.attributes['class'] != undefined
     // && element.attributes['class'].nodeValue === 'mdExplorerLink'
    ) {
      debugger;
      var variable = this._name;
      this._workWithElement(element);
      console.log("intercept!");

      return false; // prevent default action and stop event propagation
    }
  }
}
