import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { HubConnection, HubConnectionBuilder, LogLevel, HttpTransportType } from '@microsoft/signalr';
import { from } from 'rxjs';
import { tap } from 'rxjs/operators';
import { MessagePackHubProtocol } from '@microsoft/signalr-protocol-msgpack';
import { ApiConfig } from '../configs/api.config';
import { EnvAndUrlService } from './env-and-url.service';

export interface chatMesage {
  Text: string;
  ConnectionId: string;
  DateTime: Date;
}

@Injectable({
  providedIn: 'root'
})
export class SignalRService {

  constructor(
    private envAndUrlService: EnvAndUrlService,
  ) { }

  /** Do not shift it to above constructor */
  private hubConnection: HubConnection;
  private connectionUrl = this.envAndUrlService.SIGNAL_R_URL
  text: string = "";
  group: string = "";
  messages: chatMesage[] = [];

  connectionId: string = "";

  public connect = (group: string) => {
    this.group = group;
    this.startConnection();
    this.addListeners();
  }

  public join(group: string = '') {
    this.hubConnection.invoke("JoinGroup", group).catch(err => console.error(err));
  }

  public sendMessageToHub(message: string) {
    var promise = this.hubConnection.invoke("BroadcastText", message)
      .then(() => { console.log('message sent successfully to hub'); })
      .catch((err) => console.log('error while sending a message to hub: ' + err));

    return from(promise);
  }

  public sendChatMessageToHub(message: string) {
    var promise = this.hubConnection.invoke("BroadcastChat", this.buildChatMessage(message))
      .then(() => { console.log('message sent successfully to hub'); })
      .catch((err) => console.log('error while sending a message to hub: ' + err));

    return from(promise);
  }

  private buildChatMessage(message: string): chatMesage {
    return {
      ConnectionId: this.hubConnection.connectionId,
      Text: message,
      DateTime: new Date()
    };
  }

  private getConnection(): HubConnection {
    return new HubConnectionBuilder()
      .withUrl(this.connectionUrl)
      .withHubProtocol(new MessagePackHubProtocol())
      .configureLogging(LogLevel.Trace)
      .build();
  }

  private startConnection() {
    this.hubConnection = this.getConnection();

    this.hubConnection.start()
      .then(() => {
        console.log('connection started');
        this.connectionId = this.hubConnection.connectionId;
        if (this.group !== '') {
          this.join(this.group);
        }
      })
      .catch((err) => console.log('error while establishing signalr connection: ' + err))
  }

  private addListeners() {

    //Receive text from all users
    this.hubConnection.on("ReceiveText", (data) => {
      console.log("message received from Hub")
      this.text = data;
    })

    this.hubConnection.on("ReceiveChat", (data: chatMesage) => {
      console.log("chat message received from Hub")
      this.messages.push(data);
    })

    //Notify all when new user join
    this.hubConnection.on("group-" + this.group, (data) => {
      //console.log("message received from group, " + data)
      this.sendMessageToHub(this.text);
    })
  }
}
