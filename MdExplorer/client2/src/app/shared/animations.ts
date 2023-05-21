import { animate, animateChild, group, query, style, transition, trigger } from "@angular/animations";
import { ActivatedRoute } from "@angular/router";
import { BehaviorSubject, Observable } from "rxjs";
import { IDataForRouting } from "../Models/IDataForRouting";

const fromProjectsToMain = (fromState: any, toState: any): boolean => {
  
  if (fromState==="void" || fromState==="") {
    return true;
  }  
  let fromStart = <ActivatedRoute> fromState;
  let toArrive = <ActivatedRoute>toState;
  let test1 = <BehaviorSubject<IDataForRouting>> fromStart.data;
  let test2 = <BehaviorSubject<IDataForRouting>>toArrive.data;
  if (test1.value.animation === "projects" && test2.value.animation === "main" ) {
    return true;
  }

  return false;
}

const fromMainToProjects = (fromState: any, toState: any): boolean => {

  if (fromState === "void" || fromState === "") {
    return true;
  }  
  let fromStart = <ActivatedRoute>fromState;
  let toArrive = <ActivatedRoute>toState;
  let test1 = <BehaviorSubject<IDataForRouting>>fromStart.data;
  let test2 = <BehaviorSubject<IDataForRouting>>toArrive.data;
  if (test1.value.animation === "main" && test2.value.animation === "projects") {
    return true;
  }

  return false;
}


export const slideInAnimation =
  trigger('routeAnimations', [
    transition(fromProjectsToMain, [
      style({ position: 'relative' }),
      query(':enter, :leave', [
        style({
          position: 'absolute',
          top: 0,
          left: 0,
          width: '100%'
        })
      ], { optional: true }),
      query(':enter', [
        style({ left: '-100%' })
      ], { optional: true }),
      query(':leave', animateChild(), { optional: true }),
      group([
        query(':leave', [
          animate('300ms ease-out', style({ left: '100%' }))
        ], { optional: true }),
        query(':enter', [
          animate('300ms ease-out', style({ left: '0%' }))
        ], { optional: true }),
      ]),
    ]),
    transition(fromMainToProjects, [
      style({ position: 'relative' }),
      query(':enter, :leave', [
        style({
          position: 'absolute',
          top: 0,
          left: 0,
          width: '100%'
        })
      ], { optional: true }),
      query(':enter', [
        style({ left: '100%' })
      ], { optional: true }),
      query(':leave', animateChild(), { optional: true }),
      group([
        query(':leave', [
          animate('300ms ease-out', style({ left: '-100%' }))
        ], { optional: true }),
        query(':enter', [
          animate('300ms ease-out', style({ left: '0%' }))
        ], { optional: true }),
      ]),
    ]), 
    //transition('* <=> *', [
    //  style({ position: 'relative' }),
    //  query(':enter, :leave', [
    //    style({
    //      position: 'absolute',
    //      top: 0,
    //      left: 0,
    //      width: '100%'
    //    })
    //  ], { optional: true }),
    //  query(':enter', [
    //    style({ left: '-100%' })
    //  ], { optional: true }),
    //  query(':leave', animateChild(), { optional: true }),
    //  group([
    //    query(':leave', [
    //      animate('300ms ease-out', style({ left: '100%', opacity: 0 }))
    //    ], { optional: true }),
    //    query(':enter', [
    //      animate('300ms ease-out', style({ left: '0%' }))
    //    ], { optional: true }),
    //    query('@*', animateChild(), { optional: true })
    //  ]),
    //])
  ]);
