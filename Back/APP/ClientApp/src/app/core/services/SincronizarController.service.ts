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

  public sincronizar(): Observable<any> {
    return this.get(`Sincronizar`);
  }
}
