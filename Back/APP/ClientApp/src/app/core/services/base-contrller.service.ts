import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'environments/environment';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root',
})
export abstract class BaseControllerService {
    constructor(
        private http: HttpClient,
    ) {}

    protected get<T>(rota: string): Observable<T> {
        return this.http.get<T>(`${environment.apiUrl}${rota}`);
    }

    protected post<T, TParam>(rota: string, body: TParam | any): Observable<T> {
        return this.http.post<T>(`${environment.apiUrl}${rota}`, body);
    }
    protected postFile<T, TParam>(
        rota: string,
        body: TParam | any
    ): Observable<T> {
        let params = new HttpParams();

        const options = {
            params: params,
            reportProgress: true,
        };

        return this.http.post<T>(`${environment.apiUrl}${rota}`, body, options);
    }

    protected put<T, TParam>(rota: string, body: TParam | any): Observable<T> {
        return this.http.put<T>(`${environment.apiUrl}${rota}`, body);
    }

    protected patch<T, TParam>(
        rota: string,
        body: TParam | any
    ): Observable<T> {
        return this.http.patch<T>(`${environment.apiUrl}${rota}`, body);
    }

    protected delete<T>(rota: string): Observable<T> {
        return this.http.delete<T>(`${environment.apiUrl}${rota}`);
    }

    protected async getAsync<T>(rota: string): Promise<T> {
        return this.http.get<T>(`${environment.apiUrl}${rota}`).toPromise();
    }

    protected async postAsync<T, TParam>(rota: string, body: TParam | any): Promise<T> {
        return this.http.post<T>(`${environment.apiUrl}${rota}`, body).toPromise();
    }

    protected async putAsync<T, TParam>(rota: string, body: TParam | any): Promise<T> {
        return this.http.put<T>(`${environment.apiUrl}${rota}`, body).toPromise();;
    }
}
