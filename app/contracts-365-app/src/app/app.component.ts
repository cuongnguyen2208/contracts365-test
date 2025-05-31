import { Component, OnInit } from '@angular/core';
import { ApprovalService } from './services/approval.service';
import { HttpClientModule } from '@angular/common/http';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ToastrService } from 'ngx-toastr';
import { LocalStorageService } from './services/localStorage.service';
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
export class AppComponent implements OnInit{
   public title = 'contract-365-app';
   public userEmail: string = '';
  public instanceId?: string = '';
   constructor(private approvalService: ApprovalService,
    private toastr: ToastrService,
    private localStorageService: LocalStorageService
   ) {}

   ngOnInit(): void {
    // Load instanceId from localStorage when component initializes
    this.instanceId = this.localStorageService.getInstanceId();
  }

  startApproval() {
    if (!this.userEmail) {
      this.toastr.error('Email is required.', 'Error');
      return;
    }
    this.approvalService.startApproval(this.userEmail).subscribe({
      next: (response) => {
        this.instanceId = response.instanceId;
        this.localStorageService.storeInstanceId(response.instanceId);
        this.toastr.success('Approval process started', 'Success');
      }
    });
  }

  approve() {
     if (!this.instanceId) {
      this.toastr.error('Instance Id is invalid.', 'Error');
      return;
    }
    this.approvalService.approve(this.instanceId).subscribe({
      next: (response) => {
        this.toastr.success('Task is approved', 'Success');
        this.instanceId = '';
        this.localStorageService.clearInstanceId();
      }
    });
  }

  reject() {
     if (!this.instanceId) {
       this.toastr.error('Instance Id is invalid.', 'Error');
      return;
    }
    this.approvalService.reject(this.instanceId).subscribe({
      next: (response) => {
        this.toastr.success('Task is rejected', 'Success');
        this.instanceId = '';
        this.localStorageService.clearInstanceId();
      }
    });
  }
}
