import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { StateService } from '../../services/state.service';

@Component({
  selector: 'app-confirmation-page',
  imports: [CurrencyPipe, DatePipe],
  templateUrl: './confirmation-page.html',
  styleUrl: './confirmation-page.scss',
})
export class ConfirmationPageComponent {
  protected readonly state = inject(StateService);
  private readonly router = inject(Router);

  protected newSearch(): void {
    this.state.reset();
    this.router.navigate(['/']);
  }
}
