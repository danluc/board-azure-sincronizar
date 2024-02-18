import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { BaseControllerService } from "./base-contrller.service";
import { BuscarProjetoDTO } from "../models/team-project-reference";

@Injectable({
  providedIn: "root",
})
export class ProjetosControllerService extends BaseControllerService {
  constructor(_http: HttpClient) {
    super(_http);
  }

  public lista(dados: BuscarProjetoDTO): Observable<any> {
    return this.post(`Projetos`, dados);
  }
}
