import { Injectable, computed, signal } from '@angular/core';
import { FlightOffer, SearchFlightsResponse, BookingResponse } from '../models/flight.model';

export type SortKey =
  | 'price-asc' | 'price-desc'
  | 'departure-asc' | 'departure-desc'
  | 'duration-asc' | 'duration-desc';

@Injectable({ providedIn: 'root' })
export class StateService {
  readonly searchResults = signal<SearchFlightsResponse | null>(null);
  readonly isLoading = signal(false);
  readonly searchError = signal<string | null>(null);
  readonly sortOrder = signal<SortKey>('price-asc');
  readonly selectedOffer = signal<FlightOffer | null>(null);
  readonly lastBooking = signal<BookingResponse | null>(null);

  readonly sortedOffers = computed(() => {
    const results = this.searchResults();
    if (!results) return [];
    const order = this.sortOrder();
    return [...results.offers].sort((a, b) => {
      switch (order) {
        case 'price-asc':      return a.totalPrice - b.totalPrice;
        case 'price-desc':     return b.totalPrice - a.totalPrice;
        case 'departure-asc':  return a.departureUtc.localeCompare(b.departureUtc);
        case 'departure-desc': return b.departureUtc.localeCompare(a.departureUtc);
        case 'duration-asc':   return StateService.durationMs(a) - StateService.durationMs(b);
        case 'duration-desc':  return StateService.durationMs(b) - StateService.durationMs(a);
      }
    });
  });

  setSortOrder(order: SortKey): void {
    this.sortOrder.set(order);
  }

  selectOffer(offer: FlightOffer): void {
    this.selectedOffer.set(offer);
  }

  reset(): void {
    this.searchResults.set(null);
    this.selectedOffer.set(null);
    this.searchError.set(null);
    this.sortOrder.set('price-asc');
  }

  private static durationMs(offer: FlightOffer): number {
    return new Date(offer.arrivalUtc).getTime() - new Date(offer.departureUtc).getTime();
  }
}
