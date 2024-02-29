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
    try {
      this.connection = new SignalR.HubConnectionBuilder()
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: () => 1000 + Math.random() * 100,
        })
        .withUrl(`${environment.apiUrl}hubs/notification`)
        .configureLogging(SignalR.LogLevel.Trace)
        .build();

      this.connection
        .start()
        .then(() => {
          console.log("Sinal conectado");
        })
        .catch((err) => console.log(err));
      sessionStorage.setItem("SingnalConectado", "true");
    } catch (error) {
      console.log(error);
      sessionStorage.setItem("SingnalConectado", "false");
    }
  }

  public async stopConnection() {
    await this.connection.stop();
  }
}
