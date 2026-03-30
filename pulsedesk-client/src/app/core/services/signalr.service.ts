import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject, Subject } from 'rxjs';
import {
  DashboardStats,
  UserRegisteredEvent,
} from '../models/dashboard.models';
import { AuthService } from './aut.service';
import { environment } from 'src/environments/environment';

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private hubConnection: signalR.HubConnection | null = null;

  // Subjects {components subscribes to these subjects}
  // Subjects = emit values, no initial values (unlike Behavior subjects)
  userRegistered$ = new Subject<UserRegisteredEvent>();
  dashboardStats$ = new BehaviorSubject<DashboardStats | null>(null);
  userDeactivated$ = new Subject<number>();
  userRoleUpdated$ = new Subject<{ userId: number; roleName: string }>();

  // Connection State
  isConnected$ = new BehaviorSubject<boolean>(false);

  constructor(private readonly authService: AuthService) {}
  startConnection(): void {
    if (this.hubConnection?.state == signalR.HubConnectionState.Connected)
      return;
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(environment.hubUrl, {
        // signalR reads token from query string
        accessTokenFactory: () => this.authService.getToken() ?? '',
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000]) // immediatly, 2s, 5s, 10s, 30s
      .configureLogging(signalR.LogLevel.Information)
      .build();
    this.registerHandler();
    this.connect();
  }
  private connect(): void {
    this.hubConnection
      ?.start()
      .then(() => {
        console.log('signalR connected');
        this.isConnected$.next(true);
      })
      .catch((error_) => {
        console.error('SignalR Connection error:', error_);
        this.isConnected$.next(false);
      });
  }
  private registerHandler(): void {
    // Make sure the method names must match with the server method names
    this.hubConnection?.on('UserRegistered', (payload: UserRegisteredEvent) => {
      this.userRegistered$.next(payload);
    });
    this.hubConnection?.on('DashboardStatsUpdated', (stats: DashboardStats) => {
      this.dashboardStats$.next(stats);
    });
    this.hubConnection?.on('UserDeactivated', (userId: number) => {
      this.userDeactivated$.next(userId);
    });
    this.hubConnection?.on(
      'UserRoleUpdated',
      (userId: number, roleName: string) => {
        this.userRoleUpdated$.next({ userId, roleName });
      },
    );

    // Connection Events
    this.hubConnection?.onreconnecting(() => {
      this.isConnected$.next(false);
      console.log('SignalR Reconnecting...!');
    });
    this.hubConnection?.onreconnected(() => {
      this.isConnected$.next(true);
      console.log('SignalR Reconnected...!');
    });

    this.hubConnection?.onclose(() => {
      this.isConnected$.next(false);
      console.log('Signal R Closed');
    });
  }
  stopConnection(): void {
    this.hubConnection?.stop();
    this.isConnected$.next(false);
  }
}
