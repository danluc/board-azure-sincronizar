import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseControllerService } from './base-contrller.service';
import { Configuracao } from '../models/configuracao';

@Injectable({
    providedIn: 'root',
})
export class ConfiguracoesControllerService extends BaseControllerService {
    constructor(
        _http: HttpClient
    ) {
        super(_http);
    }

    public lista(): Observable<any> {
        return this.get<any>(`Configuracoes`);
    }

    public cadastrar(dados: Configuracao[]): Observable<any> {
        return this.post(`Configuracoes`, dados);
    }

    public atualizar(dados: Configuracao[]): Observable<any> {
        return this.put(`Configuracoes`, dados);
    }
}
