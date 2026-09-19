import { Routes } from '@angular/router';

import { Login } from './auth/login/login';
import { Dashboard } from './dashboard/dashboard';
import { Layout } from './layout/layout';
import { Processes } from './processes/processes';
import { ProcessDetail } from './process-detail/process-detail';

import { authGuard } from './auth/auth-guard';

export const routes: Routes = [

  {
    path: 'login',
    component: Login
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./auth/register/register')
        .then(m => m.RegisterComponent)
  },
  {
    path: '',
    component: Layout,
    canActivate: [authGuard],

    children: [

      {
        path: 'dashboard',
        component: Dashboard
      },

      {
        path: 'processes',
        component: Processes
      },

      {
        path: 'processes/:id',
        component: ProcessDetail
      },

      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      }

    ]
  },

  {
    path: '**',
    redirectTo: 'dashboard'
  }

];