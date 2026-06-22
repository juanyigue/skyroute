import { Component, inject } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { StateService } from '../../services/state.service';
import { FlightOffer } from '../../models/flight.model';

@Component({
  selector: 'app-results-list',
  imports: [CurrencyPipe, DatePipe],
  templateUrl: './results-list.html',
  styleUrl: './results-list.scss',
})
export class ResultsListComponent {
  protected readonly state = inject(StateService);
  private readonly router = inject(Router);

  protected book(offer: FlightOffer): void {
    this.state.selectOffer(offer);
    this.router.navigate(['/book']);
  }

  protected durationMinutes(departure: string, arrival: string): string {
    const diff = new Date(arrival).getTime() - new Date(departure).getTime();
    const h = Math.floor(diff / 3_600_000);
    const m = Math.floor((diff % 3_600_000) / 60_000);
    return `${h}h ${m}m`;
  }
}
