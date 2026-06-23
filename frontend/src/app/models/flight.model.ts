export type CabinClass = 'Economy' | 'Business' | 'First';
export type DocumentType = 'NationalId' | 'Passport';

export interface Airport {
  code: string;
  city: string;
  name: string;
  country: string;
}

export interface FlightOffer {
  provider: string;
  flightNumber: string;
  origin: string;
  destination: string;
  departureUtc: string;
  arrivalUtc: string;
  cabin: CabinClass;
  passengers: number;
  totalPrice: number;
  currency: string;
}

export interface SearchFlightsResponse {
  offers: FlightOffer[];
  errors: string[];
}

export interface ParsedSearchQuery {
  origin: string | null;
  destination: string | null;
  departureDate: string | null;
  passengers: number | null;
  cabin: string | null;
}

export interface PassengerDetail {
  name: string;
  email: string;
  documentNumber: string;
}

export interface CreateBookingRequest {
  passengers: PassengerDetail[];
  documentType: number;
  provider: string;
  flightNumber: string;
  origin: string;
  destination: string;
  departureUtc: string;
  arrivalUtc: string;
  cabin: number;
  totalPrice: number;
}

export interface BookingResponse {
  id: string;
  passengers: PassengerDetail[];
  documentType: string;
  provider: string;
  flightNumber: string;
  origin: string;
  destination: string;
  departureUtc: string;
  arrivalUtc: string;
  cabin: string;
  passengerCount: number;
  totalPrice: number;
  currency: string;
  bookedAt: string;
}

export const AIRPORTS: Airport[] = [
  { code: 'JFK', city: 'New York', name: 'John F. Kennedy International', country: 'US' },
  { code: 'LAX', city: 'Los Angeles', name: 'Los Angeles International', country: 'US' },
  { code: 'ORD', city: 'Chicago', name: "O'Hare International", country: 'US' },
  { code: 'MIA', city: 'Miami', name: 'Miami International', country: 'US' },
  { code: 'SFO', city: 'San Francisco', name: 'San Francisco International', country: 'US' },
  { code: 'LHR', city: 'London', name: 'Heathrow', country: 'GB' },
  { code: 'CDG', city: 'Paris', name: 'Charles de Gaulle', country: 'FR' },
  { code: 'AMS', city: 'Amsterdam', name: 'Schiphol', country: 'NL' },
  { code: 'FRA', city: 'Frankfurt', name: 'Frankfurt Airport', country: 'DE' },
  { code: 'DXB', city: 'Dubai', name: 'Dubai International', country: 'AE' },
  { code: 'SIN', city: 'Singapore', name: 'Changi Airport', country: 'SG' },
  { code: 'NRT', city: 'Tokyo', name: 'Narita International', country: 'JP' },
  { code: 'SYD', city: 'Sydney', name: 'Kingsford Smith', country: 'AU' },
  { code: 'GRU', city: 'São Paulo', name: 'Guarulhos International', country: 'BR' },
  { code: 'BOG', city: 'Bogotá', name: 'El Dorado International', country: 'CO' },
];

export const AIRPORT_COUNTRY: Record<string, string> = Object.fromEntries(
  AIRPORTS.map(a => [a.code, a.country])
);

export const CABIN_NUMBER: Record<CabinClass, number> = {
  Economy: 1,
  Business: 2,
  First: 3,
};
