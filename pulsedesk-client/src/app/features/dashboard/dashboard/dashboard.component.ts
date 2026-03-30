import { HttpClient } from '@angular/common/http';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { ColDef, GridApi, GridReadyEvent } from 'ag-grid-community';
import {
  ApexAxisChartSeries,
  ApexChart,
  ApexXAxis,
  ApexDataLabels,
  ApexTitleSubtitle,
  ApexStroke,
} from 'ng-apexcharts';
import { Subject, takeUntil } from 'rxjs';
import { ApiResponse, PagedResponse } from 'src/app/core/models/api.models';
import {
  DashboardStats,
  UserListItem,
  UserRegisteredEvent,
} from 'src/app/core/models/dashboard.models';
import { AuthService } from 'src/app/core/services/aut.service';
import { SignalRService } from 'src/app/core/services/signalr.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();
  private gridApi!: GridApi;

  // stats
  stats: DashboardStats = {
    totalUsers: 0,
    activeUsers: 0,
    newUsersToday: 0,
    newUsersThisWeek: 0,
  };

  // connection state
  isConnected = false;

  // Recent Registration show in grid
  recentRegistration: UserRegisteredEvent[] = [];

  // AG Grid column definitions
  columnDefs: ColDef[] = [
    {
      field: 'fullName',
      headerName: 'Name',
      flex: 1,
      cellRenderer: (params: any) => `
        <div style="display:flex;align-items:center;gap:8px;">
          <div style="width:32px;height:32px;border-radius:50%;
                      background:#1a237e;color:#fff;
                      display:flex;align-items:center;
                      justify-content:center;font-weight:600;font-size:12px;">
            ${params.value?.charAt(0) ?? '?'}
          </div>
          <span>${params.value}</span>
        </div>
      `,
    },
    { field: 'email', headerName: 'Email', flex: 1 },
    {
      field: 'role',
      headerName: 'Role',
      width: 120,
      cellRenderer: (params: any) => `
        <span style="background:#e8eaf6;color:#1a237e;
                     padding:4px 10px;border-radius:12px;
                     font-size:12px;font-weight:600;">
          ${params.value}
        </span>
      `,
    },
    { field: 'department', headerName: 'Department', flex: 1 },
    {
      field: 'registeredAt',
      headerName: 'Registered',
      width: 180,
      valueFormatter: (params) => new Date(params.value).toLocaleString(),
    },
  ];
  defaultColDef: ColDef = {
    sortable: true,
    resizable: true,
  };
  // ApexCharts config
  chartOptions: Partial<ChartOptions> = this.getInitialChartOptions();

  constructor(
    private readonly signalRService: SignalRService,
    private readonly http: HttpClient,
    readonly authService: AuthService,
  ) {}
  ngOnInit(): void {
    this.loadInitialStats();
    this.subscribetoSignalR();
  }
  private getInitialChartOptions(): Partial<ChartOptions> {
    return {
      series: [{ name: 'New Users Today', data: [] }],
      chart: {
        type: 'area',
        height: 250,
        animations: { enabled: true },
        toolbar: { show: false },
      },
      xaxis: { categories: [] },
      dataLabels: { enabled: false },
      stroke: { curve: 'smooth', width: 2 },
      title: { text: 'Live Registration Activity', align: 'left' },
    };
  }

  private loadInitialStats(): void {
    this.http
      .get<ApiResponse<DashboardStats>>(`${environment.apiUrl}/Dashboard/stats`)
      .subscribe({
        next: (res: any) => {
          if (res?.success) {
            debugger;
            this.stats = res.data;
            setTimeout(() => {
              this.seedChart(res.data);
            }, 100);
          }
        },
        error: (err: any) => {
          console.error('Error loading stats:', err);
        },
      });
    // Load recent registrations for the grid
    this.http
      .get<ApiResponse<PagedResponse<UserListItem>>>(
        `${environment.apiUrl}/Users`,
        {
          params: {
            pageSize: '10',
            page: '1',
            sortBy: 'CreatedAt',
            sortDirection: 'DESC',
          },
        },
      )
      .subscribe((res) => {
        if (res.success) {
          // Map UserListItem to UserRegisteredEvent shape
          this.recentRegistration = res.data.items.map((u) => ({
            id: u.id,
            fullName: u.fullName,
            email: u.email,
            role: u.role,
            department: u.department,
            registeredAt: u.createdAt,
          }));
        }
      });
  }
  private seedChart(stats: DashboardStats): void {
    const now = new Date();

    const categories = Array.from({ length: 6 }, (_, i) => {
      const d = new Date(now.getTime() - (5 - i) * 60000);
      return d.toLocaleTimeString();
    });

    // Build rising sequence ending at current value
    // e.g. if newUsersToday = 4 → [0, 1, 2, 2, 3, 4]
    const current = stats.newUsersToday;
    const data = Array.from({ length: 6 }, (_, i) => {
      return Math.round((current / 5) * i);
    });
    // Make sure last point is always the real value
    data[5] = current;

    this.chartOptions = {
      ...this.getInitialChartOptions(),
      series: [{ name: 'New Users Today', data }],
      xaxis: { categories },
    };
  }

  private subscribetoSignalR(): void {
    // 1. connection status
    this.signalRService.isConnected$
      .pipe(takeUntil(this.destroy$))
      .subscribe((connected) => {
        this.isConnected = connected;
      });

    // 2.new user registered add it to the board
    this.signalRService.userRegistered$
      .pipe(takeUntil(this.destroy$))
      .subscribe((event) => {
        this.recentRegistration = [event, ...this.recentRegistration];
        // adding the latest record at the top
        //this.gridApi.applyTransactionAsync({ add: [event], addIndex: 0 });
      });

    // 3.stats updated - updating the cunt
    this.signalRService.dashboardStats$
      .pipe(takeUntil(this.destroy$))
      .subscribe((stats) => {
        if (stats) {
          this.stats = stats;
          this.updateChart(stats);
        }
      });

    // 4. user deactivated. remove that row and update the stats
    this.signalRService.userDeactivated$
      .pipe(takeUntil(this.destroy$))
      .subscribe((userId) => {
        this.recentRegistration = this.recentRegistration.filter(
          (u) => u.id != userId,
        );
      });
  }

  private updateChart(stats: DashboardStats): void {
    const now = new Date().toLocaleTimeString();

    const currentData = (this.chartOptions.series?.[0] as any)?.data ?? [];
    const currentCats = (this.chartOptions.xaxis as any)?.categories ?? [];

    // Keep last 10 points
    const newData = [...currentData.slice(-9), stats.newUsersToday];
    const newCategories = [...currentCats.slice(-9), now];

    // Full object replacement — required for change detection
    this.chartOptions = {
      ...this.chartOptions,
      series: [{ name: 'New Users Today', data: newData }],
      xaxis: { categories: newCategories },
    };
  }
  onGridReady(params: GridReadyEvent): void {
    this.gridApi = params.api;
  }
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}

export type ChartOptions = {
  series: ApexAxisChartSeries;
  chart: ApexChart;
  xaxis: ApexXAxis;
  dataLabels: ApexDataLabels;
  title: ApexTitleSubtitle;
  stroke: ApexStroke;
};
