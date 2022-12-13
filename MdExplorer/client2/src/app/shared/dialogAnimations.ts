import { Injectable } from "@angular/core";
import { AnimationOption } from "./NgDialogAnimationService";

@Injectable({
  providedIn: 'root'
})
export class INCOMING_ROTATE_OPTION implements AnimationOption {
  keyframes?: Keyframe[];
  keyframeAnimationOptions: KeyframeAnimationOptions;
  constructor() {
    debugger;
    this.keyframes = [
      { transform: "rotate(360deg)" },
      { transform: "rotate(0)" }
    ];
    this.keyframeAnimationOptions = { easing: "ease-in-out", duration: 500 }
  }

}

@Injectable({
  providedIn: 'root'
})
export class OUTGOING_ROTATE_OPTION implements AnimationOption {
  keyframes?: Keyframe[];
  keyframeAnimationOptions: KeyframeAnimationOptions;
  constructor() {
    debugger;
    this.keyframes = [
      { transform: "rotate(0)" },
      { transform: "rotate(360deg)" }
    ];
    this.keyframeAnimationOptions = { easing: "ease-in-out", duration: 500 }
  }
}
