import { Injectable } from '@angular/core';
import * as signalR from "@microsoft/signalr";

interface linkSignalREvent_Component {
  key: string
  object: any;
  callback: (data: any, objectThis: any) => any
}

@Injectable({
  providedIn: 'root'
})
export class MonitorMDService {

  linkEventCompArray: linkSignalREvent_Component[];

  constructor() {
    this.startConnection();
    console.log('MonitorMDService constructor');
    this.linkEventCompArray = [];


  }

  private hubConnection: signalR.HubConnection
  private rule1IsRegistered :any;

  public startConnection = () => {
    debugger;
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('../signalr/monitormd')
      .build();
    
    if (this.hubConnection.state == "Disconnected" ) {
      this.hubConnection
        .start()
        .then(() => console.log('Connection started'))
        .catch(err => {
          console.log('Error while starting connection: ' + err);
        }
        );    
    }

    this.hubConnection.on('markdownfileisprocessed', (data) => {
      this.myCallBack(data, 'markdownfileisprocessed');
    });

    
  }

  public addOnCloseEvent(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.hubConnection.onclose((data) => {
      callback(data, objectThis);
    });
  } 

  public addRefactoringFileEvent(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.hubConnection.on('refactoringFileEvent', (data) => {
      callback(data, objectThis);
    });
  }

  public addMarkdownFileListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.hubConnection.on('markdownfileischanged', (data) => {
      callback(data, objectThis);
      console.log('markdownfileischanged');
    });
  }

  private myCallBack(data: any, signalREvent:string) {
    this.linkEventCompArray.forEach(_ => {
      if (_.key == signalREvent) {
        _.callback(data, _.object);
      }
    })
  }

  public addMdProcessedListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {    
    let check = this.linkEventCompArray.find(_ => _.key == 'markdownfileisprocessed' && _.object.constructor.name === objectThis.constructor.name);
    if (check == undefined) {
      this.linkEventCompArray.push({ key:'markdownfileisprocessed',object : objectThis, callback:callback });
    }    
  }

  public addMdRule1Listener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    // giusto per evitare di effettuare l'instanziazione un centinaio di volte l'evento
    console.log('addMdRule1Listener');
    if (this.rule1IsRegistered == undefined) {
      this.rule1IsRegistered = objectThis;
      this.hubConnection.on('markdownbreakrule1', (data) => {        
        callback(data, objectThis);
      });
    }
  }

  public addPdfIsReadyListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.hubConnection.on('pdfisready', (data) => {
      callback(data, objectThis);
    });
  }

  public addConnectionIdListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.hubConnection.on('getconnectionid', (data) => {
      callback(data, objectThis);
    });
  }

  public getConnectionId(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.hubConnection.invoke('GetConnectionId')
      .then(function (connectionId) {
        callback(connectionId, objectThis);
      });
  }



}
