import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from 'src/app/core/services/aut.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent implements OnInit {
  form!: FormGroup;
  loading = false;
  error = '';

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly router: Router,
    private readonly actRoute: ActivatedRoute,
  ) {}
  ngOnInit(): void {
    // Redirecting If already logged in
    if (this.authService.isAuthenticated) this.router.navigate(['/dashboard']);
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;

    this.loading = true;
    this.error = '';

    this.authService.login(this.form.value).subscribe({
      next: (resp) => {
        if (resp.success) {
          const returnUrl = this.actRoute.snapshot.queryParams['returnUrl'] || [
            '/dashboard',
          ];
          this.router.navigateByUrl(returnUrl);
        } else {
          this.error = resp.errors[0] ?? 'Login Failed';
          this.loading = false;
        }
      },
      error: (err) => {
        this.error = err.error?.errors?.[0] ?? 'Invalid email or password';
        this.loading = false;
      },
    });
  }
}
