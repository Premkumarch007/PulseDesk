import { Component, OnDestroy, OnInit } from '@angular/core';
import { AuthService } from './core/services/aut.service';
import { SignalRService } from './core/services/signalr.service';

@Component({
  selector: 'app-root',
  template: `
    <ng-container *ngIf="authService.isAuthenticated; else publicLayout">
      <div class="app-shell">
        <!-- Sidebar -->
        <app-sidebar></app-sidebar>

        <!-- Main area -->
        <div class="main-content">
          <app-topbar></app-topbar>
          <div class="page-content">
            <router-outlet></router-outlet>
          </div>
        </div>
      </div>
    </ng-container>

    <!-- Public pages (login/register) — no shell -->
    <ng-template #publicLayout>
      <router-outlet></router-outlet>
    </ng-template>
  `,
  styles: [
    `
      .app-shell {
        display: flex;
        height: 100vh;
        overflow: hidden;
      }
      .main-content {
        flex: 1;
        display: flex;
        flex-direction: column;
        overflow: hidden;
        min-width: 0;
      }
      .page-content {
        flex: 1;
        overflow-y: auto;
        background: #f5f5f5;
      }
    `,
  ],
})
export class AppComponent implements OnInit, OnDestroy {
  constructor(
    readonly authService: AuthService,
    private readonly signalRService: SignalRService,
  ) {}

  ngOnInit(): void {
    // statring signalR when app loads and user is authenticated
    if (this.authService.isAuthenticated && this.authService.isAdminOrManager)
      this.signalRService.startConnection();

    // Reconnect signalR when user is logsin
    this.authService.currentUser$.subscribe((user) => {
      if (user && ['Admin', 'Manager'].includes(user.role))
        this.signalRService.startConnection();
      else this.signalRService.stopConnection();
    });
  }
  ngOnDestroy(): void {
    this.signalRService.stopConnection();
  }
}
