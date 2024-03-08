import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { BaseControllerService } from "./base-contrller.service";
import { Azure } from "../models/azure";

@Injectable({
  providedIn: "root",
})
export class AzureControllerService extends BaseControllerService {
  constructor(_http: HttpClient) {
    super(_http);
  }

  public lista(): Observable<any> {
    return this.get<Azure[]>(`Azure`);
  }

  public cadastrar(dados: Azure[]): Observable<any> {
    return this.post(`Azure`, dados);
  }

  public atualizar(dados: Azure[]): Observable<any> {
    return this.put(`Azure`, dados);
  }
}
