import { Component, Inject } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { UserListItem } from 'src/app/core/models/dashboard.models';

@Component({
  selector: 'app-update-roledialog',
  template: `
    <div class="dialog-container">
      <h2 mat-dialog-title>Update Role</h2>

      <mat-dialog-content>
        <p class="subtitle">
          Change role for <strong>{{ data.fullName }}</strong>
        </p>

        <mat-form-field appearance="outline" style="width:100%">
          <mat-label>Select Role</mat-label>
          <mat-select [formControl]="roleControl">
            <mat-option *ngFor="let r of roles" [value]="r">
              <div class="role-option">
                <span class="role-dot" [class]="r.toLowerCase()"></span>
                {{ r }}
              </div>
            </mat-option>
          </mat-select>
          <mat-error>Role is required</mat-error>
        </mat-form-field>

        <!-- Role description -->
        <div class="role-desc" *ngIf="roleControl.value">
          <mat-icon>info_outline</mat-icon>
          {{ descriptions[roleControl.value] }}
        </div>
      </mat-dialog-content>

      <mat-dialog-actions align="end">
        <button mat-button (click)="dialogRef.close()">Cancel</button>
        <button
          mat-raised-button
          color="primary"
          [disabled]="roleControl.invalid || roleControl.value === data.role"
          (click)="confirm()"
        >
          Update Role
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [
    `
      .dialog-container {
        padding: 8px;
        min-width: 340px;
      }
      .subtitle {
        color: #616161;
        margin-bottom: 16px;
        font-size: 14px;
      }

      .role-option {
        display: flex;
        align-items: center;
        gap: 8px;
      }

      .role-dot {
        width: 10px;
        height: 10px;
        border-radius: 50%;
        &.admin {
          background: #7b1fa2;
        }
        &.manager {
          background: #1565c0;
        }
        &.agent {
          background: #00838f;
        }
        &.user {
          background: #546e7a;
        }
      }

      .role-desc {
        display: flex;
        align-items: flex-start;
        gap: 8px;
        background: #e8eaf6;
        border-radius: 8px;
        padding: 10px 12px;
        font-size: 13px;
        color: #1a237e;
        margin-top: 8px;
        mat-icon {
          font-size: 16px;
          width: 16px;
          height: 16px;
          flex-shrink: 0;
        }
      }
    `,
  ],
})
export class UpdateRoledialogComponent {
  roles = ['Admin', 'Manager', 'Agent', 'User'];
  roleControl = new FormControl(this.data?.role, Validators.required);

  descriptions: Record<string, string> = {
    Admin: 'Full system access — all policies granted',
    Manager: 'Can assign/close tickets and view reports',
    Agent: 'Can handle and close assigned tickets',
    User: 'Can create and view their own tickets only',
  };
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: UserListItem,
    public dialogRef: MatDialogRef<UpdateRoledialogComponent>,
  ) {}
  confirm() {
    this.dialogRef.close(this.roleControl.value);
  }
}
