import { Injectable } from '@angular/core';
import * as signalR from "@microsoft/signalr";

@Injectable({
  providedIn: 'root'
})
export class MonitorMDService {

  private hubConnection: signalR.HubConnection
  public startConnection = () => {
    
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('../signalr/monitormd')
      .build();
    this.hubConnection
      .start()
      .then(() => console.log('Connection started'))
      .catch(err => console.log('Error while starting connection: ' + err))
  }

  public addMarkdownFileListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.hubConnection.on('markdownfileischanged', (data) => {      
      callback(data, objectThis);
    });
  }
}
