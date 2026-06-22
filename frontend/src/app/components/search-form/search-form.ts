import { Component, inject } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { StateService } from '../../services/state.service';
import { AIRPORTS } from '../../models/flight.model';

@Component({
  selector: 'app-search-form',
  imports: [ReactiveFormsModule],
  templateUrl: './search-form.html',
  styleUrl: './search-form.scss',
})
export class SearchFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(ApiService);
  protected readonly state = inject(StateService);

  protected readonly airports = AIRPORTS;

  protected readonly form = this.fb.group({
    origin: ['', Validators.required],
    destination: ['', Validators.required],
    departureDate: ['', Validators.required],
    passengers: [1, [Validators.required, Validators.min(1), Validators.max(9)]],
    cabin: ['Economy', Validators.required],
  });

  protected search(): void {
    if (this.form.invalid) return;
    const { origin, destination, departureDate, passengers, cabin } = this.form.value;
    this.state.isLoading.set(true);
    this.state.searchError.set(null);
    this.api
      .searchFlights(origin!, destination!, departureDate!, passengers!, cabin!)
      .subscribe({
        next: results => {
          this.state.searchResults.set(results);
          this.state.isLoading.set(false);
        },
        error: () => {
          this.state.searchError.set('Search failed. Is the backend running?');
          this.state.isLoading.set(false);
        },
      });
  }

  fill(origin: string, destination: string, date: string, passengers: number, cabin: string): void {
    this.form.patchValue({ origin, destination, departureDate: date, passengers, cabin });
  }
}
