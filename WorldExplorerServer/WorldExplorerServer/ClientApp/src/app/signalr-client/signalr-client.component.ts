import { Component, OnInit, NgZone   } from '@angular/core';
import { SignalrService } from '../signalr.service';
import { WorldExplorerClientLogMessage } from '../models/worldexporerclientlogmessage.model';
import { Tab } from '../models/tab.model';

@Component({
  selector: 'app-signalr-client',
  templateUrl: './signalr-client.component.html',
  styleUrls: ['./signalr-client.component.css']
})
export class SignalrClientComponent  {
  logMessage: WorldExplorerClientLogMessage;
  canSendMessage: boolean;
  tabs: Tab[];
  currentTab: string;

  constructor(
    private signalrService: SignalrService,
    private _ngZone: NgZone
  ) {
    this.subscribeToEvents();
    //this.logMessage = new WorldExplorerClientLogMessage();
    this.tabs = [];
    this.tabs.push(new Tab('Presence', 'Presence messages'));
    this.tabs.push(new Tab('SignalR', 'Welcome to SignalR Room'));
    this.currentTab = 'Presence';
  }

  sendMessage() {
    //if (this.canSendMessage) {
    //  this.logMessage.room = this.currentTopic;
    //  this.signalrService.sendChatMessage(this.logMessage);
    //}
  }

  OnTabChange(tabName) {
    this.currentTab = tabName;
  }

  private subscribeToEvents(): void {
    this.signalrService.connectionEstablished.subscribe(() => {
      this.canSendMessage = true;
    });

    this.signalrService.messageReceived.subscribe((message: WorldExplorerClientLogMessage) => {
      this._ngZone.run(() => {
        this.logMessage = new WorldExplorerClientLogMessage();
        let room = this.tabs.find(t => t.heading == message.topic);
        if (room) {
          room.messageHistory.push(new WorldExplorerClientLogMessage(message.topic, message.message));
        }
      });
    });
  }
}
