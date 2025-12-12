import {Component, signal} from '@angular/core';
import {NavigationEnd, Router, RouterLink, RouterOutlet} from '@angular/router';
import {AuthService} from './services/auth';
import {LoadingService} from './services/loading';
import {filter} from 'rxjs';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('Frontend');

  protected readonly isLoading = signal(false);

  constructor(private router: Router,
              protected authService: AuthService,
              private loadingService: LoadingService) {
    router.events.pipe(
      filter(e => e instanceof NavigationEnd))
      .subscribe(() => window.scrollTo({ top: 0, behavior: 'auto' })
      );

    this.loadingService.loading$.subscribe(loading => {
      this.isLoading.set(loading);
    });
  }

  public async logout() {
    this.authService.logout();
    await this.router.navigateByUrl('/');
  }

}
