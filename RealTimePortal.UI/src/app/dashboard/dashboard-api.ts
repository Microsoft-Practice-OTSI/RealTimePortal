import { Injectable } from '@angular/core';

import {
  HttpClient,
  HttpParams
} from '@angular/common/http';

import { Observable } from 'rxjs';

import { environment } from '../../environments/environment';

import { DashboardSummary } from './models/dashboard-summary';
import { ProcessTypeSummary } from './models/process-type-summary';
import { DashboardProcess } from './models/dashboard-process';
import { DashboardTrend } from './models/dashboard-trend';


@Injectable({
  providedIn: 'root'
})
export class DashboardApi {

  private readonly baseUrl =
    `${environment.apiUrl}/dashboard`;


  constructor(
    private http: HttpClient
  ) {}


  getSummary(
    period: number
  ): Observable<DashboardSummary> {

    const params =
      new HttpParams()
        .set('period', period);

    return this.http.get<DashboardSummary>(
      `${this.baseUrl}/summary`,
      { params }
    );
  }


  getProcessTypes(
    period: number
  ): Observable<ProcessTypeSummary[]> {

    const params =
      new HttpParams()
        .set('period', period);

    return this.http.get<ProcessTypeSummary[]>(
      `${this.baseUrl}/process-types`,
      { params }
    );
  }


  getRecentProcesses(
    period: number,
    count = 10
  ): Observable<DashboardProcess[]> {

    const params =
      new HttpParams()
        .set('period', period)
        .set('count', count);

    return this.http.get<DashboardProcess[]>(
      `${this.baseUrl}/recent-processes`,
      { params }
    );
  }


  getRunningProcesses(
    period: number
  ): Observable<DashboardProcess[]> {

    const params =
      new HttpParams()
        .set('period', period);

    return this.http.get<DashboardProcess[]>(
      `${this.baseUrl}/running-processes`,
      { params }
    );
  }


  getFailedProcesses(
    period: number,
    count = 10
  ): Observable<DashboardProcess[]> {

    const params =
      new HttpParams()
        .set('period', period)
        .set('count', count);

    return this.http.get<DashboardProcess[]>(
      `${this.baseUrl}/failed-processes`,
      { params }
    );
  }


  getTrend(
    period: number
  ): Observable<DashboardTrend[]> {

    const params =
      new HttpParams()
        .set('period', period);

    return this.http.get<DashboardTrend[]>(
      `${this.baseUrl}/trend`,
      { params }
    );
  }
}