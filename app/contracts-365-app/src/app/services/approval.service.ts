import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { StartApprovalResponse } from '../models/app.model';

@Injectable({ providedIn: 'root' })
export class ApprovalService {
  private apiUrl = 'http://localhost:7069/api';

  constructor(private http: HttpClient) {}

  startApproval(userEmail: string): Observable<StartApprovalResponse> {
    return this.http.post<StartApprovalResponse>(
      `${this.apiUrl}/StartApproval`,
      { userEmail }
    );
  }

  approve(instanceId?: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/Approve`, { instanceId });
  }

  reject(instanceId?: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/Reject`, { instanceId });
  }
}