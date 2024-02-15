import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseControllerService } from './base-contrller.service';
import { Conta } from '../models/conta';

@Injectable({
    providedIn: 'root',
})
export class ConfiguracoesControllerService extends BaseControllerService {
    constructor(
        _http: HttpClient
    ) {
        super(_http);
    }

    public lista(): Observable<Conta[]> {
        return this.get<Conta[]>(`Configuracoes`);
    }

    public cadastrar(dados: Conta[]): Observable<any> {
        return this.post(`Configuracoes`, dados);
    }

    public atualizar(dados: Conta[]): Observable<any> {
        return this.put(`Configuracoes`, dados);
    }
}
