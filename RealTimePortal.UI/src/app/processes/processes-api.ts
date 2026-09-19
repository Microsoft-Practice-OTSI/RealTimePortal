import { Injectable } from '@angular/core';

import {
  HttpClient
} from '@angular/common/http';

import {
  Observable
} from 'rxjs';

import { environment } from '../../environments/environment';

import { Process } from './models/process';
import { CreateProcessRequest } from './models/create-process-request';

@Injectable({
  providedIn: 'root'
})
export class ProcessesApi {

  private readonly baseUrl =
    `${environment.apiUrl}/processes`;


  constructor(
    private http: HttpClient
  ) {}


  getAll(): Observable<Process[]> {

    return this.http.get<Process[]>(
      this.baseUrl
    );
  }


  getById(
    processId: number
  ): Observable<Process> {

    return this.http.get<Process>(
      `${this.baseUrl}/${processId}`
    );
  }


create(
  request: CreateProcessRequest
): Observable<Process> {
  return this.http.post<Process>(
    this.baseUrl,
    request
  );
}


  start(
    processId: number
  ): Observable<any> {

    return this.http.post(
      `${this.baseUrl}/${processId}/start`,
      {}
    );
  }


  cancel(
    processId: number
  ): Observable<any> {

    return this.http.post(
      `${this.baseUrl}/${processId}/cancel`,
      {}
    );
  }
}