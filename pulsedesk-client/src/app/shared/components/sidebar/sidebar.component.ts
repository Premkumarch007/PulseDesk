import { Component, OnDestroy, OnInit } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { filter, Subject, takeUntil } from 'rxjs';
import { AuthService } from 'src/app/core/services/aut.service';

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss'],
})
export class SidebarComponent implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();

  isCollapsed = false;
  activeRoute = '';

  navItems: NavItem[] = [
    {
      label: 'Dashboard',
      icon: 'dashboard',
      route: '/dashboard',
    },
    {
      label: 'Users',
      icon: 'people',
      route: '/users',
      policy: 'CanManageUsers',
    },
    {
      label: 'Reports',
      icon: 'bar_chart',
      route: '/reports',
      policy: 'CanViewReports',
    },
  ];
  constructor(
    readonly authService: AuthService,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
    this.router.events
      .pipe(
        filter((e) => e instanceof NavigationEnd),
        takeUntil(this.destroy$),
      )
      .subscribe((e: any) => {
        this.activeRoute = e.urlAfterRedirects;
      });
    this.activeRoute = this.router.url;
  }
  isActive(item: string): boolean {
    return this.activeRoute.startsWith(item);
  }
  isVisible(item: NavItem): boolean {
    if (!item?.policy) return true;
    return this.authService.hasPolicy(item?.policy);
  }
  toggleSidebar(): void {
    this.isCollapsed = !this.isCollapsed;
  }
  navigate(route: string): void {
    this.router.navigate([route]);
  }
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
interface NavItem {
  label: string;
  icon: string;
  route: string;
  policy?: string; // only shown if user has this policy
  badge?: number;
}
