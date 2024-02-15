import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseControllerService } from './base-contrller.service';
import { Conta } from '../models/conta';

@Injectable({
    providedIn: 'root',
})
export class ContasControllerService extends BaseControllerService {
    constructor(
        _http: HttpClient
    ) {
        super(_http);
    }

    public lista(): Observable<any> {
        return this.get<Conta[]>(`Contas`);
    }

    public cadastrar(dados: Conta[]): Observable<any> {
        return this.post(`Contas`, dados);
    }

    public atualizar(dados: Conta[]): Observable<any> {
        return this.put(`Contas`, dados);
    }
}
