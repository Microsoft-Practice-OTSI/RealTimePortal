import { Injectable } from '@angular/core';

import {
  HttpClient
} from '@angular/common/http';

import {
  Observable
} from 'rxjs';

import { environment } from '../../environments/environment';

import {
  Process
} from '../processes/models/process';


@Injectable({
  providedIn: 'root'
})
export class ProcessDetailApi {

  private readonly baseUrl =
    `${environment.apiUrl}/processes`;


  constructor(
    private http: HttpClient
  ) {}


  getById(
    processId: number
  ): Observable<Process> {

    return this.http.get<Process>(
      `${this.baseUrl}/${processId}`
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

  updateProgress(
  executionId: string,
  step: string,
  status: string,
  percentage: number,
  message?: string | null
): Observable<any> {

  return this.http.post(
    `${environment.apiUrl}/progress/update`,
    {
      executionId,
      step,
      status,
      percentage,
      message: message ?? null
    }
  );
}
}