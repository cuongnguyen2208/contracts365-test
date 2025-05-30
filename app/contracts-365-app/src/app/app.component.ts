import { Component } from '@angular/core';
import { ApprovalService } from './services/approval.service';
import { HttpClientModule } from '@angular/common/http';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    HttpClientModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    FormsModule
    ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
   public title = 'contract-365-app';
   public userEmail: string = '';
  public instanceId?: string = '';
   constructor(private approvalService: ApprovalService) {}

  startApproval() {
    if (!this.userEmail) {
      console.log('Email is required');
      return;
    }
    this.approvalService.startApproval(this.userEmail).subscribe({
      next: (response) => {
        this.instanceId = response.instanceId;
        console.log('Start Approve:', response);
      },
      error: (error) => console.error('Error:', error)
    });
  }

  approve() {
     if (!this.instanceId) {
      console.log('Instance Id is invalid.');
      return;
    }
    this.approvalService.approve(this.instanceId).subscribe({
      next: (response) => {
        console.log('Approve:', response);
        this.instanceId = '';
      },
      error: (error) => console.log('Error:', error)
    });
  }

  reject() {
     if (!this.instanceId) {
      console.log('Instance Id is invalid.');
      return;
    }
    this.approvalService.reject(this.instanceId).subscribe({
      next: (response) => {
        console.log('Reject:', response);
        this.instanceId = '';
      },
      error: (error) => console.log('Error:', error)
    });
  }
}
