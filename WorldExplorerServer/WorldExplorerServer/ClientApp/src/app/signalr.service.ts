
import { EventEmitter, Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@aspnet/signalr';
import { WorldExplorerClientLogMessage } from './models/worldexporerclientlogmessage.model';

@Injectable()
export class SignalrService {

  messageReceived = new EventEmitter<WorldExplorerClientLogMessage>();
  connectionEstablished = new EventEmitter<Boolean>();

  private connectionIsEstablished = false;
  private _hubConnection: HubConnection;

  constructor() {
    this.createConnection();
    this.registerOnServerEvents();
    this.startConnection();
  }

  sendChatMessage(message: WorldExplorerClientLogMessage) {
    this._hubConnection.send('WorldExplorerClientLogMessage', message);
  }

  private createConnection() {
    this._hubConnection = new HubConnectionBuilder()
      .withUrl('/WorldExplorerHub')
      .build();
  }

  private startConnection(): void {
    this._hubConnection
      .start()
      .then(() => {
        this.connectionIsEstablished = true;
        console.log('Hub connection started');
        this.connectionEstablished.emit(true);
      })
      .catch(err => {
        console.log('Error while establishing connection, retrying...');
        setTimeout(this.startConnection(), 5000);
      });
  }

  private registerOnServerEvents(): void {
    this._hubConnection.on('WorldExplorerClientLogMessage', (data: any) => {
      console.log('Data received:' + data);
      this.messageReceived.emit(data);
    });
  } 

}
