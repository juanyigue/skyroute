import { TestBed } from '@angular/core/testing';
import { StateService } from './state.service';
import { FlightOffer } from '../models/flight.model';

const offer = (override: Partial<FlightOffer> = {}): FlightOffer => ({
  provider: 'GlobalAir',
  flightNumber: 'GA001',
  origin: 'JFK',
  destination: 'LHR',
  departureUtc: '2026-09-01T08:00:00Z',
  arrivalUtc: '2026-09-01T20:00:00Z',
  cabin: 'Economy',
  passengers: 1,
  totalPrice: 500,
  currency: 'USD',
  ...override,
});

describe('StateService', () => {
  let service: StateService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(StateService);
  });

  it('sortedOffers returns empty array when no results', () => {
    expect(service.sortedOffers()).toEqual([]);
  });

  it('sortedOffers sorts by price ascending by default', () => {
    service.searchResults.set({ offers: [offer({ totalPrice: 800 }), offer({ totalPrice: 300 }), offer({ totalPrice: 500 })], errors: [] });
    expect(service.sortedOffers().map(o => o.totalPrice)).toEqual([300, 500, 800]);
  });

  it('setSortOrder price-desc sorts descending by price', () => {
    service.searchResults.set({ offers: [offer({ totalPrice: 300 }), offer({ totalPrice: 800 })], errors: [] });
    service.setSortOrder('price-desc');
    expect(service.sortedOffers().map(o => o.totalPrice)).toEqual([800, 300]);
  });

  it('setSortOrder departure-asc sorts by departure time', () => {
    service.searchResults.set({
      offers: [
        offer({ flightNumber: 'late', departureUtc: '2026-09-01T15:00:00Z' }),
        offer({ flightNumber: 'early', departureUtc: '2026-09-01T06:00:00Z' }),
      ],
      errors: [],
    });
    service.setSortOrder('departure-asc');
    expect(service.sortedOffers()[0].flightNumber).toBe('early');
  });

  it('setSortOrder departure-desc sorts by departure time descending', () => {
    service.searchResults.set({
      offers: [
        offer({ flightNumber: 'early', departureUtc: '2026-09-01T06:00:00Z' }),
        offer({ flightNumber: 'late', departureUtc: '2026-09-01T15:00:00Z' }),
      ],
      errors: [],
    });
    service.setSortOrder('departure-desc');
    expect(service.sortedOffers()[0].flightNumber).toBe('late');
  });

  it('setSortOrder duration-asc sorts shortest flight first', () => {
    service.searchResults.set({
      offers: [
        offer({ flightNumber: 'long', departureUtc: '2026-09-01T08:00:00Z', arrivalUtc: '2026-09-01T20:00:00Z' }),
        offer({ flightNumber: 'short', departureUtc: '2026-09-01T08:00:00Z', arrivalUtc: '2026-09-01T12:00:00Z' }),
      ],
      errors: [],
    });
    service.setSortOrder('duration-asc');
    expect(service.sortedOffers()[0].flightNumber).toBe('short');
  });

  it('setSortOrder duration-desc sorts longest flight first', () => {
    service.searchResults.set({
      offers: [
        offer({ flightNumber: 'short', departureUtc: '2026-09-01T08:00:00Z', arrivalUtc: '2026-09-01T12:00:00Z' }),
        offer({ flightNumber: 'long', departureUtc: '2026-09-01T08:00:00Z', arrivalUtc: '2026-09-01T20:00:00Z' }),
      ],
      errors: [],
    });
    service.setSortOrder('duration-desc');
    expect(service.sortedOffers()[0].flightNumber).toBe('long');
  });

  it('selectOffer sets the selected offer', () => {
    const o = offer({ flightNumber: 'GA999' });
    service.selectOffer(o);
    expect(service.selectedOffer()).toBe(o);
  });

  it('reset clears all state and restores default sort', () => {
    service.searchResults.set({ offers: [offer()], errors: [] });
    service.selectOffer(offer());
    service.searchError.set('oops');
    service.setSortOrder('duration-desc');

    service.reset();

    expect(service.searchResults()).toBeNull();
    expect(service.selectedOffer()).toBeNull();
    expect(service.searchError()).toBeNull();
    expect(service.sortOrder()).toBe('price-asc');
  });
});
