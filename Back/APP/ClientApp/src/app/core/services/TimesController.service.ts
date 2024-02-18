import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { BaseControllerService } from "./base-contrller.service";
import { BuscarIterationsDTO, BuscarProjetoDTO } from "../models/team-project-reference";

@Injectable({
  providedIn: "root",
})
export class TimesControllerService extends BaseControllerService {
  constructor(_http: HttpClient) {
    super(_http);
  }

  public lista(projetoNome: string, dados: BuscarProjetoDTO): Observable<any> {
    return this.post(`Times/${projetoNome}`, dados);
  }

  public listaAreas(dados: BuscarIterationsDTO): Observable<any> {
    return this.post(`Times/BuscarAreas/`, dados);
  }
}
