import {CanActivateFn, Router, Routes} from '@angular/router';
import {inject} from '@angular/core';
import {AuthService} from './services/auth';
import {Login} from './pages/login/login';
import {Menu} from './pages/menu/menu';


export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isLoggedIn()) {
    return true;
  }

  // Redirect to login
  return router.createUrlTree(['/']);
};


export const routes: Routes = [
  // Always open routes
  {path: '', component: Login},

  // Protected routes
  {
    path: '',
    canActivate: [authGuard],
    children: [
      {path: '', redirectTo: 'menu', pathMatch: 'full'},
      {path: 'menu', component: Menu},
      ]
  }

];
