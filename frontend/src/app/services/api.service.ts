import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  SearchFlightsResponse,
  ParsedSearchQuery,
  CreateBookingRequest,
  BookingResponse,
} from '../models/flight.model';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly base = '/api';

  searchFlights(
    origin: string,
    destination: string,
    departureDate: string,
    passengers: number,
    cabin: string
  ): Observable<SearchFlightsResponse> {
    const params = new HttpParams()
      .set('origin', origin)
      .set('destination', destination)
      .set('departureDate', departureDate)
      .set('passengers', passengers)
      .set('cabin', cabin);
    return this.http.get<SearchFlightsResponse>(`${this.base}/flights/search`, { params });
  }

  parseSearch(text: string): Observable<ParsedSearchQuery> {
    return this.http.post<ParsedSearchQuery>(`${this.base}/ai/parse-search`, { text });
  }

  createBooking(request: CreateBookingRequest): Observable<BookingResponse> {
    return this.http.post<BookingResponse>(`${this.base}/bookings`, request);
  }

  getBooking(id: string): Observable<BookingResponse> {
    return this.http.get<BookingResponse>(`${this.base}/bookings/${id}`);
  }
}
