import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from 'src/app/services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
})
export class RegisterComponent {
  @Output() cancelRegister = new EventEmitter();
  model: any = {};

  constructor(
    private accountService: AccountService,
    private router: Router,
    private toaster: ToastrService
  ) {}

  register() {
    this.accountService.register(this.model).subscribe({
      next: () => {
        this.cancel();
        this.router.navigateByUrl('/pets');
      },
      error: (error) => {
        this.toaster.error(error.error);
        console.log(error);
      },
    });
  }

  cancel() {
    this.cancelRegister.emit(false);
  }
}
