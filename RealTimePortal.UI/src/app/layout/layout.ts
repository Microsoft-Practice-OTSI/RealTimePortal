import {
  Component,
  inject,
  signal
} from '@angular/core';

import {
  Router,
  RouterLink,
  RouterLinkActive,
  RouterOutlet
} from '@angular/router';

import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

import { Auth } from '../auth/auth';
import { ThemeService } from '../core/services/theme.service';

@Component({
  selector: 'app-layout',

  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatIconModule,
    MatButtonModule
  ],

  templateUrl: './layout.html'
})
export class Layout {

  sidebarCollapsed = signal(false);

  readonly themeService = inject(ThemeService);

  get currentUserName(): string {
    return this.auth.getCurrentUserName();
  }

  get currentUserRole(): string {
    return this.auth.getCurrentUserRole();
  }

  get currentUserInitials(): string {
    return this.auth.getCurrentUserInitials();
  }


  constructor(
    private auth: Auth,
    private router: Router
  ) {}


  toggleSidebar(): void {

    this.sidebarCollapsed.update(
      value => !value
    );
  }


  toggleTheme(): void {
    this.themeService.toggleTheme();
  }


  logout(): void {

    this.auth.logout();

    this.router.navigate([
      '/login'
    ]);
  }
}