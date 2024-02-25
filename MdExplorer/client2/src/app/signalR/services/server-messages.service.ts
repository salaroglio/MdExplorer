import { Injectable } from '@angular/core';
import * as signalR from "@microsoft/signalr";
import { GITService } from '../../git/services/gitservice.service';
import { ConnectionLostProvider } from '../../signalR/dialogs/connection-lost/connection-lost.provider';
import { ParsingProjectProvider } from '../../signalR/dialogs/parsing-project/parsing-project.provider';
import { PlantumlWorkingProvider } from '../../signalR/dialogs/plantuml-working/plantuml-working.provider';
import { connect } from 'net';
import { OpeningApplicationProvider } from '../dialogs/opening-application/opening-application.provider';

interface linkSignalREvent_Component {
  key: string
  object: any;
  callback: (data: any, objectThis: any) => any
}

@Injectable({
  providedIn: 'root'
})
export class MdServerMessagesService {

  linkEventCompArray: linkSignalREvent_Component[];
  public connectionId: string;

  constructor(
    private parsingProjectProvider: ParsingProjectProvider,
    private plantumlWorkingProvider: PlantumlWorkingProvider,
    private connectionLostProvider: ConnectionLostProvider,
    private openingApplicationProvider: OpeningApplicationProvider,
    private gitService: GITService) {
    this.startConnection();
    console.log('MonitorMDService constructor');
    this.linkEventCompArray = [];
    
  }

  private hubConnection: signalR.HubConnection
  private rule1IsRegistered: any;
  private connectionIsLost: boolean = false;
  private consoleIsClosed: boolean = false;
  

  public startConnection = () => {
    if (this.hubConnection == null) {
      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl('../signalr/monitormd')
        .build();
      this.hubConnection.on('markdownfileisprocessed', (data) => {
        this.processCallBack(data, 'markdownfileisprocessed');
      });
      this.hubConnection.on('parsingProjectStart', (data) => {
        this.parsingProjectProvider.show(data);
      });
      this.hubConnection.on('openingApplication', (data) => {
        this.openingApplicationProvider.show(data);
      });
      this.hubConnection.on('parsingProjectStop', (data) => {
        this.parsingProjectProvider.hide(data);
      });
      this.hubConnection.on('plantumlWorkStart', (data) => {
        this.plantumlWorkingProvider.show(data);
      });
      this.hubConnection.on('plantumlWorkStop', (data) => {
        this.plantumlWorkingProvider.hide(data);
      });
      this.hubConnection.on('indexingFolder', (folder) => {
        this.parsingProjectProvider.folder$.next(folder);
      });

      this.hubConnection.on('consoleClosed', (data) => {
        console.log('consoleClosed');
        this.consoleIsClosed = true;
        this.connectionLostProvider.showConsoleClosed();
      });
      this.hubConnection.onclose((data) => {
        if (!this.consoleIsClosed) {
          this.connectionLostProvider.show(this);
          this.connectionIsLost = true;
        }
      });

    }

    if (this.hubConnection.state == "Disconnected") {
      this.hubConnection
        .start()
        .then(() => {
          console.log('Connection started');
          this.connectionIsLost = false;
          this.getCurrentConnectionId(this);
        }

        )
        .catch(err => {
          console.log('Error while starting connection: ' + err);
        }
        );
    }

    

  }




  public addRefactoringFileEvent(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.hubConnection.on('refactoringFileEvent', (data) => {
      callback(data, objectThis);
    });
  }


  public addMarkdownFileListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    this.hubConnection.on('markdownfileischanged', (data) => {
      this.gitService.getCurrentBranch();
      callback(data, objectThis);
      console.log('markdownfileischanged');
    });
  }

  private processCallBack(data: any, signalREvent: string) {
    this.linkEventCompArray.forEach(_ => {
      if (_.key == signalREvent) {
        _.callback(data, _.object);
      }
    })
  }



  public addMdProcessedListener(callback: (data: any, objectThis: any) => any, objectThis: any): void {
    let check = this.linkEventCompArray.find(_ =>
      _.key == 'markdownfileisprocessed' && _.object.constructor.name === objectThis.constructor.name);
    if (check == undefined) {
      this.linkEventCompArray.push({ key: 'markdownfileisprocessed', object: objectThis, callback: callback });
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
        objectThis.connectionId = connectionId;
        callback(connectionId, objectThis);
      });
  }

  public getCurrentConnectionId(objectThis: MdServerMessagesService): void {
    this.hubConnection.invoke('GetConnectionId')
      .then(function (connectionId) {        
        objectThis.connectionId = connectionId;        
      });
  }

}
