import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AuthService } from 'src/app/core/services/aut.service';
import { UserService } from 'src/app/core/services/user.service';

@Component({
  selector: 'app-userdetaildialog',
  template: `
    <div class="dialog-container">
      <!-- Header -->
      <div class="dialog-header">
        <div class="avatar-large">
          {{ data.firstName?.[0] }}{{ data.lastName?.[0] }}
        </div>
        <div class="header-info">
          <h2>{{ data.fullName }}</h2>
          <span class="role-chip">{{ data.role }}</span>
        </div>
        <button mat-icon-button (click)="dialogRef.close()">
          <mat-icon>close</mat-icon>
        </button>
      </div>

      <mat-divider></mat-divider>

      <!-- Details -->
      <div class="details-grid">
        <div class="detail-item">
          <span class="detail-label">Email</span>
          <span class="detail-value">{{ data.email }}</span>
        </div>

        <div class="detail-item">
          <span class="detail-label">Department</span>
          <span class="detail-value">{{ data.department ?? '—' }}</span>
        </div>

        <div class="detail-item">
          <span class="detail-label">Job Title</span>
          <span class="detail-value">{{ data.jobTitle ?? '—' }}</span>
        </div>

        <div class="detail-item">
          <span class="detail-label">Status</span>
          <span
            class="detail-value"
            [style.color]="data.isActive ? '#2e7d32' : '#c62828'"
          >
            {{ data.isActive ? '● Active' : '● Inactive' }}
          </span>
        </div>

        <div class="detail-item">
          <span class="detail-label">Joined</span>
          <span class="detail-value">
            {{ data.createdAt | date: 'mediumDate' }}
          </span>
        </div>

        <div class="detail-item">
          <span class="detail-label">Last Login</span>
          <span class="detail-value">
            {{
              data.lastLoginAt ? (data.lastLoginAt | date: 'medium') : 'Never'
            }}
          </span>
        </div>

        <!-- Policies -->
        <div class="detail-item full-width">
          <span class="detail-label">Policies</span>
          <div class="policies-list">
            <span *ngFor="let policy of data.policies" class="policy-chip">
              {{ policy }}
            </span>
            <span *ngIf="!data.policies?.length" class="detail-value">
              No policies
            </span>
          </div>
        </div>
      </div>

      <!-- Actions -->
      <div
        class="dialog-actions"
        *ngIf="authService.hasPolicy('CanManageUsers')"
      >
        <button
          mat-stroked-button
          color="warn"
          *ngIf="data.isActive"
          [disabled]="deactivating"
          (click)="deactivate()"
        >
          <mat-spinner diameter="16" *ngIf="deactivating"></mat-spinner>
          <mat-icon *ngIf="!deactivating">block</mat-icon>
          Deactivate User
        </button>
      </div>
    </div>
  `,
  styles: [
    `
      .dialog-container {
        padding: 0;
        min-width: 420px;
      }

      .dialog-header {
        display: flex;
        align-items: center;
        gap: 16px;
        padding: 20px 24px;
      }

      .avatar-large {
        width: 56px;
        height: 56px;
        border-radius: 50%;
        background: #1a237e;
        color: #fff;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 20px;
        font-weight: 700;
        flex-shrink: 0;
        letter-spacing: -1px;
      }

      .header-info {
        flex: 1;
        h2 {
          margin: 0;
          font-size: 18px;
          font-weight: 700;
          color: #212121;
        }
      }

      .role-chip {
        background: #e8eaf6;
        color: #1a237e;
        padding: 2px 10px;
        border-radius: 10px;
        font-size: 12px;
        font-weight: 600;
      }

      .details-grid {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 16px;
        padding: 20px 24px;
      }

      .detail-item {
        display: flex;
        flex-direction: column;
        gap: 4px;
        &.full-width {
          grid-column: 1 / -1;
        }
      }

      .detail-label {
        font-size: 11px;
        font-weight: 600;
        text-transform: uppercase;
        letter-spacing: 0.5px;
        color: #9e9e9e;
      }

      .detail-value {
        font-size: 14px;
        color: #212121;
      }

      .policies-list {
        display: flex;
        flex-wrap: wrap;
        gap: 6px;
        margin-top: 4px;
      }

      .policy-chip {
        background: #e0f7fa;
        color: #006064;
        padding: 3px 10px;
        border-radius: 10px;
        font-size: 12px;
        font-weight: 600;
      }

      .dialog-actions {
        padding: 12px 24px 20px;
        display: flex;
        justify-content: flex-end;
      }
    `,
  ],
})
export class UserdetaildialogComponent {
  deactivating = false;

  constructor(
    public dialogRef: MatDialogRef<UserdetaildialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    readonly authService: AuthService,
    private readonly userService: UserService,
    private readonly snackBar: MatSnackBar,
  ) {}
  deactivate(): void {
    if (
      !confirm(
        `Are you sure you want to deactivate this user?: -- ${this.data?.fullName ?? 'Unknown User'} --`,
      )
    )
      return;

    this.deactivating = true;
    this.userService.deactivateUser(this.data?.id).subscribe({
      next: (response) => {
        if (response?.success) {
          this.data.isActive = false;
          this.snackBar.open('User deactivated successfully', 'Close', {
            duration: 3000,
          });
        } else {
          this.snackBar.open(
            response.message || 'Failed to deactivate user',
            'Close',
            { duration: 3000 },
          );
        }
        this.deactivating = false;
      },
      error: () => {
        this.snackBar.open(
          'An error occurred while deactivating user',
          'Close',
          {
            duration: 3000,
          },
        );
        this.deactivating = false;
      },
      complete: () => {
        this.deactivating = false;
      },
    });
  }
}
