import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { BaseControllerService } from "./base-contrller.service";
import { Sincronizar } from "../models/sincronizar";

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

  public ultimo(): Observable<Sincronizar> {
    return this.get<Sincronizar>(`Sincronizar/Ultimo`);
  }

  public sincronizar(): Observable<any> {
    return this.post(`Sincronizar`, null);
  }
}
