import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./pages/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'register',
    loadComponent: () => import('./pages/register/register.component').then((m) => m.RegisterComponent),
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layout/shell/shell.component').then((m) => m.ShellComponent),
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./pages/dashboard/dashboard.component').then((m) => m.DashboardComponent),
      },
      {
        path: 'incomes',
        loadComponent: () => import('./pages/incomes/incomes.component').then((m) => m.IncomesComponent),
      },
      {
        path: 'expenses',
        loadComponent: () => import('./pages/expenses/expenses.component').then((m) => m.ExpensesComponent),
      },
      {
        path: 'pending',
        loadComponent: () => import('./pages/pending/pending.component').then((m) => m.PendingComponent),
      },
      {
        path: 'accounts',
        loadComponent: () => import('./pages/accounts/accounts.component').then((m) => m.AccountsComponent),
      },
      {
        path: 'budgets',
        loadComponent: () => import('./pages/budgets/budgets.component').then((m) => m.BudgetsComponent),
      },
      {
        path: 'stats',
        loadComponent: () => import('./pages/stats/stats.component').then((m) => m.StatsComponent),
      },
      {
        path: 'settings',
        loadComponent: () => import('./pages/settings/settings.component').then((m) => m.SettingsComponent),
      },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    ],
  },
  { path: '**', redirectTo: '' },
];
