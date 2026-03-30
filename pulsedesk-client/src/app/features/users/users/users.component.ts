import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import {
  GridApi,
  ColDef,
  ICellRendererParams,
  ValueFormatterParams,
  GridReadyEvent,
} from 'ag-grid-community';
import {
  debounce,
  debounceTime,
  distinctUntilChanged,
  Subject,
  takeUntil,
} from 'rxjs';
import { UserListItem } from 'src/app/core/models/dashboard.models';
import { AuthService } from 'src/app/core/services/aut.service';
import { SignalRService } from 'src/app/core/services/signalr.service';
import { UserService } from 'src/app/core/services/user.service';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { UserdetaildialogComponent } from '../userdetaildialog/userdetaildialog.component';
import { UpdateRoledialogComponent } from '../update-roledialog/update-roledialog.component';

@Component({
  selector: 'app-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.scss'],
})
export class UsersComponent implements OnInit, OnDestroy {
  [x: string]: any;
  private destroy$ = new Subject<void>();
  private gridApi!: GridApi;

  // Search
  searchControl = new FormControl('');
  selectedRole = '';
  showActive: boolean | undefined = undefined;

  // Pagination
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;

  loading = false;
  users: UserListItem[] = [];

  readonly roles = ['', 'Admin', 'Manager', 'Agent', 'User'];

  // ── AG Grid Column Definitions ──────────────────────────────
  columnDefs: ColDef[] = [
    {
      field: 'fullName',
      headerName: 'Name',
      flex: 1,
      minWidth: 180,
      cellRenderer: (p: ICellRendererParams) => `
        <div style="display:flex;align-items:center;gap:10px;height:100%">
          <div style="
            width:34px;height:34px;border-radius:50%;
            background:#1a237e;color:#fff;
            display:flex;align-items:center;justify-content:center;
            font-weight:700;font-size:13px;flex-shrink:0;letter-spacing:-0.5px">
            ${(p.data.firstName?.[0] ?? '') + (p.data.lastName?.[0] ?? '')}
          </div>
          <div style="display:flex;flex-direction:column;min-width:0">
            <span style="font-weight:600;font-size:13px;color:#212121;
                         white-space:nowrap;overflow:hidden;text-overflow:ellipsis">
              ${p.value}
            </span>
            <span style="font-size:11px;color:#9e9e9e;
                         white-space:nowrap;overflow:hidden;text-overflow:ellipsis">
              ${p.data.jobTitle ?? ''}
            </span>
          </div>
        </div>`,
    },
    {
      field: 'email',
      headerName: 'Email',
      flex: 1,
      minWidth: 200,
      cellRenderer: (p: ICellRendererParams) => `
        <span style="color:#1565c0;font-size:13px">${p.value}</span>`,
    },
    {
      field: 'role',
      headerName: 'Role',
      width: 120,
      cellRenderer: (p: ICellRendererParams) => {
        const colors: Record<string, string> = {
          Admin: '#4a148c',
          Manager: '#1a237e',
          Agent: '#006064',
          User: '#37474f',
        };
        const bg: Record<string, string> = {
          Admin: '#f3e5f5',
          Manager: '#e8eaf6',
          Agent: '#e0f7fa',
          User: '#eceff1',
        };
        const c = colors[p.value] ?? '#37474f';
        const b = bg[p.value] ?? '#eceff1';
        return `<span style="
          background:${b};color:${c};
          padding:3px 10px;border-radius:12px;
          font-size:12px;font-weight:600">
          ${p.value}
        </span>`;
      },
    },
    {
      field: 'department',
      headerName: 'Department',
      flex: 1,
      minWidth: 130,
      valueFormatter: (p: ValueFormatterParams) => p.value ?? '—',
    },
    {
      field: 'isActive',
      headerName: 'Status',
      width: 110,
      cellRenderer: (p: ICellRendererParams) =>
        p.value
          ? `<span style="color:#2e7d32;font-size:12px;font-weight:600">
             ● Active
           </span>`
          : `<span style="color:#c62828;font-size:12px;font-weight:600">
             ● Inactive
           </span>`,
    },
    {
      field: 'createdAt',
      headerName: 'Joined',
      width: 130,
      valueFormatter: (p: ValueFormatterParams) =>
        new Date(p.value).toLocaleDateString(),
    },
    {
      field: 'lastLoginAt',
      headerName: 'Last Login',
      width: 140,
      valueFormatter: (p: ValueFormatterParams) =>
        p.value ? new Date(p.value).toLocaleDateString() : 'Never',
    },
    {
      headerName: 'Actions',
      width: 120,
      sortable: false,
      cellRenderer: (p: ICellRendererParams) => `
        <div style="display:flex;align-items:center;gap:4px;height:100%">
          <button
            data-action="view"
            data-id="${p.data.id}"
            style="border:none;background:#e8eaf6;color:#1a237e;
                   padding:4px 8px;border-radius:6px;cursor:pointer;
                   font-size:12px;font-weight:600">
            View
          </button>
          <button
            data-action="role"
            data-id="${p.data.id}"
            style="border:none;background:#e0f7fa;color:#006064;
                   padding:4px 8px;border-radius:6px;cursor:pointer;
                   font-size:12px;font-weight:600">
            Role
          </button>
        </div>`,
      onCellClicked: (p: any) => {
        const action = p.event?.target?.dataset?.action;
        const id = Number.parseInt(p.event?.target?.dataset?.id);
        if (!action || !id) return;
        if (action === 'view') this.openDetail(id);
        if (action === 'role') this.openRoleDialog(p.data);
      },
    },
  ];

  defaultColDef: ColDef = {
    sortable: true,
    resizable: true,
    filter: false,
  };
  readonly Math = Math;

  constructor(
    private readonly userService: UserService,
    private readonly authService: AuthService,
    private readonly signalRService: SignalRService,
    private readonly dialog: MatDialog,
    private readonly snackBar: MatSnackBar,
  ) {}

  ngOnInit(): void {
    this.setupSearch();
    this.loadUsers();
    this.subscribeToSignalR();
  }

  setupSearch(): void {
    this.searchControl.valueChanges
      .pipe(debounceTime(400), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe(() => {
        this.currentPage = 1;
        this.loadUsers();
      });
  }
  // ── SignalR — live updates ────────────────────────────────────
  private subscribeToSignalR(): void {
    // New user registered → reload first page or prepend if on page 1
    this.signalRService.userRegistered$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        if (this.currentPage === 1) this.loadUsers();
        this.totalCount++;
      });

    // User deactivated → update row in grid without full reload
    this.signalRService.userDeactivated$
      .pipe(takeUntil(this.destroy$))
      .subscribe((userId) => {
        this.users = this.users.map((u) =>
          u.id === userId ? { ...u, isActive: false } : u,
        );
      });

    // Role updated → update row in grid without full reload
    this.signalRService.userRoleUpdated$
      .pipe(takeUntil(this.destroy$))
      .subscribe(({ userId, roleName }) => {
        this.users = this.users.map((u) =>
          u.id === userId ? { ...u, role: roleName } : u,
        );
      });
  }

  // Filters
  onRoleFilter(role: string): void {
    this.selectedRole = role;
    this.currentPage = 1;
    this.loadUsers();
  }
  onStatusFilter(active: boolean | undefined): void {
    this.showActive = active;
    this.currentPage = 1;
    this.loadUsers();
  }

  clearFilters(): void {
    this.searchControl.setValue('');
    this.selectedRole = '';
    this.showActive = undefined;
    this.currentPage = 1;
    this.loadUsers();
  }

  // Loadig users from Dapper API
  loadUsers(): void {
    this.loading = true;

    this.userService
      .getUsers({
        search: this.searchControl.value ?? undefined,
        role: this.selectedRole || undefined,
        isActive: this.showActive,
        page: this.currentPage,
        pageSize: this.pageSize,
        sortBy: 'CreatedAt',
        sortDirection: 'DESC',
      })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.users = res?.data?.items;
            this.totalCount = res?.data?.totalCount;
            this.totalPages = Math.ceil(this.totalCount / this.pageSize);
          }
          this.loading = false;
        },
        error: () => {
          this.loading = false;
          this.showSnack('Failed to load users. Please try again.', 'error');
        },
        complete: () => (this.loading = false),
      });
  }
  get pages(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.loadUsers();
  }
  openDetail(userId: number): void {
    this.userService.getUserById(userId).subscribe((resp) => {
      if (resp.success) {
        this.dialog.open(UserdetaildialogComponent, {
          width: '400px',
          data: resp?.data,
        });
      }
    });
  }
  openRoleDialog(user: UserListItem): void {
    const ref = this.dialog.open(UpdateRoledialogComponent, {
      width: '300px',
      data: user,
    });
    ref.afterClosed().subscribe((newRole: string | undefined) => {
      if (!newRole) return;
      this.userService.updateRole(user.id, newRole).subscribe({
        next: (res) => {
          if (res.success) {
            this.showSnack(`${user.fullName} is now a ${newRole}.`, 'success');
            this.loadUsers();
          } else
            this.showSnack('Failed to update role. Please try again.', 'error');
        },
        error: (err) => {
          this.showSnack(err?.error?.errors?.[0], 'error');
        },
      });
    });
  }

  deactivateUser(user: UserListItem) {
    if (!confirm(`Are you sure you want to deactivate ${user.fullName}?`))
      return;
    this.userService.deactivateUser(user.id).subscribe({
      next: (res) => {
        if (res.success) {
          this.showSnack(`${user.fullName} has been deactivated.`, 'success');
          this.loadUsers();
        } else {
          this.showSnack(
            'Failed to deactivate user. Please try again.',
            'error',
          );
        }
      },
      error: () => {
        this.showSnack('Failed to deactivate user. Please try again.', 'error');
      },
    });
  }
  private showSnack(message: string, type: 'success' | 'error'): void {
    this.snackBar.open(message, 'Close', {
      duration: 3000,
      panelClass: type === 'success' ? 'snack-success' : 'snack-error',
      horizontalPosition: 'right',
      verticalPosition: 'top',
    });
  }
  onGridReady(params: GridReadyEvent): void {
    this.gridApi = params.api;
  }
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
