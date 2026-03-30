import { Component, OnDestroy, OnInit } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { filter, Subject, takeUntil } from 'rxjs';
import { AuthService } from 'src/app/core/services/aut.service';
import { SignalRService } from 'src/app/core/services/signalr.service';

@Component({
  selector: 'app-topbar',
  templateUrl: './topbar.component.html',
  styleUrls: ['./topbar.component.scss'],
})
export class TopbarComponent implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();

  pageTitle = 'Dashboard';
  isConnected = false;
  currentTime = new Date();
  notifCount = 0;

  // Map route to page title
  private readonly titleMap: Record<string, string> = {
    '/dashboard': 'Admin Dashboard',
    '/users': 'User Management',
    '/reports': 'Reports & Analytics',
  };
  constructor(
    readonly authService: AuthService,
    private readonly signalRService: SignalRService,
    private readonly router: Router,
  ) {}
  ngOnInit(): void {
    this.router.events
      .pipe(
        filter((e) => e instanceof NavigationEnd),
        takeUntil(this.destroy$),
      )
      .subscribe((e: any) => {
        const url = e.urlAfterRedirects.split('?')[0];
        this.pageTitle = this.titleMap[url] ?? 'PulseDesk';
      });
    // Setting Initial titles
    const url = this.router.url.split('?')[0];
    this.pageTitle = this.titleMap[url] ?? 'PulseDesk';

    // signalr connection status
    this.signalRService.isConnected$
      .pipe(takeUntil(this.destroy$))
      .subscribe((c) => (this.isConnected = c));

    this.signalRService.userRegistered$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.notifCount++);

    //Updating clock every minute
    setInterval(() => (this.currentTime = new Date()), 60000);
  }

  clearNotifications = () => (this.notifCount = 0);
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
