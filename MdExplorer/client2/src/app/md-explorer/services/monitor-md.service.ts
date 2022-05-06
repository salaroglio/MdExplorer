import { Injectable } from '@angular/core';
import * as signalR from "@microsoft/signalr";

@Injectable({
  providedIn: 'root'
})
export class MonitorMDService {

  constructor() {
    this.startConnection();
  }

  private hubConnection: signalR.HubConnection
  private rule1IsRegistered :any;

  public startConnection = () => {

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('../signalr/monitormd')
      .build();
    this.hubConnection
      .start()
      .then(() => console.log('Connection started'))
      .catch(err => {        
        console.log('Error while starting connection: ' + err);
      }
      );    
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

  public addMdProcessedListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.hubConnection.on('markdownfileisprocessed', (data) => {
      callback(data, objectThis);
    });
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
