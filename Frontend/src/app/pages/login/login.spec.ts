import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Login } from './login';
import { AuthService } from '../../services/auth';

describe('Login', () => {
  let component: Login;
  let fixture: ComponentFixture<Login>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let routerSpy: jasmine.SpyObj<Router>;

  beforeEach(async () => {
    const authSpy = jasmine.createSpyObj('AuthService', ['login', 'isLoggedIn']);
    const routerSpyObj = jasmine.createSpyObj('Router', ['navigateByUrl']);

    await TestBed.configureTestingModule({
      imports: [Login, ReactiveFormsModule],
      providers: [
        { provide: AuthService, useValue: authSpy },
        { provide: Router, useValue: routerSpyObj }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Login);
    component = fixture.componentInstance;
    authServiceSpy = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    routerSpy = TestBed.inject(Router) as jasmine.SpyObj<Router>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('form initialization', () => {
    it('should initialize login form with empty values', () => {
      expect(component.loginForm).toBeDefined();
      expect(component.loginForm.get('username')?.value).toBe('');
      expect(component.loginForm.get('password')?.value).toBe('');
    });

    it('should have required validators on username and password', () => {
      const usernameControl = component.loginForm.get('username');
      const passwordControl = component.loginForm.get('password');

      expect(usernameControl?.hasError('required')).toBe(true);
      expect(passwordControl?.hasError('required')).toBe(true);
    });
  });

  describe('ngOnInit', () => {
    it('should redirect to menu if already logged in', async () => {
      authServiceSpy.isLoggedIn.and.returnValue(true);
      routerSpy.navigateByUrl.and.returnValue(Promise.resolve(true));

      await component.ngOnInit();

      expect(routerSpy.navigateByUrl).toHaveBeenCalledWith('/menu');
    });

    it('should not redirect if not logged in', async () => {
      authServiceSpy.isLoggedIn.and.returnValue(false);

      await component.ngOnInit();

      expect(routerSpy.navigateByUrl).not.toHaveBeenCalled();
    });
  });

  describe('onSubmit', () => {
    it('should not submit when form is invalid', async () => {
      component.loginForm.patchValue({ username: '', password: '' });

      await component.onSubmit();

      expect(authServiceSpy.login).not.toHaveBeenCalled();
      expect(component.loginForm.get('username')?.touched).toBe(true);
      expect(component.loginForm.get('password')?.touched).toBe(true);
    });

    it('should call authService.login with form values when form is valid', async () => {
      const credentials = { username: 'testuser', password: 'testpass' };
      component.loginForm.patchValue(credentials);
      authServiceSpy.login.and.returnValue(Promise.resolve(true));
      routerSpy.navigateByUrl.and.returnValue(Promise.resolve(true));

      await component.onSubmit();

      expect(authServiceSpy.login).toHaveBeenCalledWith(credentials);
    });

    it('should navigate to menu on successful login', async () => {
      component.loginForm.patchValue({ username: 'test', password: 'pass' });
      authServiceSpy.login.and.returnValue(Promise.resolve(true));
      routerSpy.navigateByUrl.and.returnValue(Promise.resolve(true));

      await component.onSubmit();

      expect(routerSpy.navigateByUrl).toHaveBeenCalledWith('/menu');
      expect(component.loginError).toBe(false);
    });

    it('should show error and reset form on failed login', async () => {
      component.loginForm.patchValue({ username: 'test', password: 'wrong' });
      authServiceSpy.login.and.returnValue(Promise.resolve(false));

      await component.onSubmit();

      expect(component.loginError).toBe(true);
      expect(component.loginForm.get('username')?.value).toBeNull();
      expect(component.loginForm.get('password')?.value).toBeNull();
    });

    it('should reset loginError before attempting login', async () => {
      component.loginError = true;
      component.loginForm.patchValue({ username: 'test', password: 'pass' });
      authServiceSpy.login.and.returnValue(Promise.resolve(true));
      routerSpy.navigateByUrl.and.returnValue(Promise.resolve(true));

      await component.onSubmit();

      expect(component.loginError).toBe(false);
    });
  });

  describe('form validation', () => {
    it('should be invalid when username is empty', () => {
      component.loginForm.patchValue({ username: '', password: 'password' });
      expect(component.loginForm.valid).toBe(false);
    });

    it('should be invalid when password is empty', () => {
      component.loginForm.patchValue({ username: 'username', password: '' });
      expect(component.loginForm.valid).toBe(false);
    });

    it('should be valid when both fields are filled', () => {
      component.loginForm.patchValue({ username: 'user', password: 'pass' });
      expect(component.loginForm.valid).toBe(true);
    });
  });
});
