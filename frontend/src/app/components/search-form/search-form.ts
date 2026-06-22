import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormsModule, FormBuilder, Validators } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { StateService } from '../../services/state.service';
import { AIRPORTS } from '../../models/flight.model';

@Component({
  selector: 'app-search-form',
  imports: [ReactiveFormsModule, FormsModule],
  templateUrl: './search-form.html',
  styleUrl: './search-form.scss',
})
export class SearchFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(ApiService);
  protected readonly state = inject(StateService);

  protected readonly airports = AIRPORTS;
  protected aiText = '';
  protected readonly isParsing = signal(false);
  protected readonly parseError = signal<string | null>(null);
  protected readonly parseFilled = signal(false);

  protected readonly form = this.fb.group({
    origin: ['', Validators.required],
    destination: ['', Validators.required],
    departureDate: ['', Validators.required],
    passengers: [1, [Validators.required, Validators.min(1), Validators.max(9)]],
    cabin: ['Economy', Validators.required],
  });

  protected autofill(): void {
    const text = this.aiText.trim();
    if (!text) return;
    this.isParsing.set(true);
    this.parseError.set(null);
    this.parseFilled.set(false);
    this.api.parseSearch(text).subscribe({
      next: parsed => {
        this.fill(
          parsed.origin ?? '',
          parsed.destination ?? '',
          parsed.departureDate ?? '',
          parsed.passengers ?? 1,
          parsed.cabin ?? 'Economy',
        );
        this.parseFilled.set(true);
        this.isParsing.set(false);
      },
      error: () => {
        this.parseError.set('Could not parse — fill the fields manually.');
        this.isParsing.set(false);
      },
    });
  }

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
