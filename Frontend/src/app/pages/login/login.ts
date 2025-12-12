import {Component, OnInit} from '@angular/core';
import {CommonModule} from '@angular/common';
import {FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import {AuthService} from '../../services/auth';
import {Router} from '@angular/router';

@Component({
  selector: 'app-login',
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login implements OnInit {
  public loginForm!: FormGroup;
  public loginError: boolean = false;

  constructor(private fb: FormBuilder,
              private router: Router,
              private authService: AuthService) {
    this.loginForm = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required]
    });
  }

  public async ngOnInit() {
    if (this.authService.isLoggedIn()) {
      await this.router.navigateByUrl('/menu');
    }
  }

  public async onSubmit() {
    if (this.loginForm.valid) {
      this.loginError = false;
      const result = await this.authService.login(this.loginForm.value);
      if (result) {
        await this.router.navigateByUrl('/menu');
      }
      else {
        this.loginError = true;
        this.loginForm.reset();
      }
    }
    else {
      // Marca todos os campos como "touched" pra forçar exibir os erros
      this.loginForm.markAllAsTouched();
    }
  }

}
