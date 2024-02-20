import { environment } from "environments/environment";
import { Injectable, OnDestroy } from "@angular/core";
import * as SignalR from "@microsoft/signalr";

@Injectable({
  providedIn: "root",
})
export class NotificationHubService {
  public connection: SignalR.HubConnection;

  constructor() {
    window.onbeforeunload = () => {
      this.stopConnection();
    };
  }

  public startConnection() {
    this.connection = new SignalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}hubs/notification`)
      .configureLogging(SignalR.LogLevel.Error)
      .build();

    this.connection.start();
  }

  public async stopConnection() {
    this.connection.off("NovaNotificacao");
    await this.connection.stop();
  }
}
