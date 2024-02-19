import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { BaseControllerService } from "./base-contrller.service";

@Injectable({
  providedIn: "root",
})
export class SincronizarControllerService extends BaseControllerService {
  constructor(_http: HttpClient) {
    super(_http);
  }

  public listar(): Observable<any> {
    return this.get(`Sincronizar`);
  }

  public sincronizar(): Observable<any> {
    return this.post(`Sincronizar`, null);
  }
}
