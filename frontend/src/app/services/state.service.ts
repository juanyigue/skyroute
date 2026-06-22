import { Injectable, computed, signal } from '@angular/core';
import { FlightOffer, SearchFlightsResponse, BookingResponse } from '../models/flight.model';

@Injectable({ providedIn: 'root' })
export class StateService {
  readonly searchResults = signal<SearchFlightsResponse | null>(null);
  readonly isLoading = signal(false);
  readonly searchError = signal<string | null>(null);
  readonly sortOrder = signal<'asc' | 'desc'>('asc');
  readonly selectedOffer = signal<FlightOffer | null>(null);
  readonly lastBooking = signal<BookingResponse | null>(null);

  readonly sortedOffers = computed(() => {
    const results = this.searchResults();
    if (!results) return [];
    return [...results.offers].sort((a, b) =>
      this.sortOrder() === 'asc'
        ? a.totalPrice - b.totalPrice
        : b.totalPrice - a.totalPrice
    );
  });

  toggleSort(): void {
    this.sortOrder.update(o => (o === 'asc' ? 'desc' : 'asc'));
  }

  selectOffer(offer: FlightOffer): void {
    this.selectedOffer.set(offer);
  }

  reset(): void {
    this.searchResults.set(null);
    this.selectedOffer.set(null);
    this.searchError.set(null);
    this.sortOrder.set('asc');
  }
}
