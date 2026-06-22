import { Component, computed, inject } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { StateService } from '../../services/state.service';
import { ApiService } from '../../services/api.service';
import { AIRPORT_COUNTRY, CABIN_NUMBER } from '../../models/flight.model';

@Component({
  selector: 'app-booking-page',
  imports: [ReactiveFormsModule, CurrencyPipe, DatePipe],
  templateUrl: './booking-page.html',
  styleUrl: './booking-page.scss',
})
export class BookingPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  protected readonly state = inject(StateService);
  private readonly api = inject(ApiService);

  protected readonly isSubmitting = computed(() => false);
  protected submitting = false;
  protected submitError: string | null = null;

  // Derives domestic vs international from the selected offer's airport countries
  protected readonly isInternational = computed(() => {
    const offer = this.state.selectedOffer();
    if (!offer) return false;
    return AIRPORT_COUNTRY[offer.origin] !== AIRPORT_COUNTRY[offer.destination];
  });

  protected readonly documentLabel = computed(() =>
    this.isInternational() ? 'Passport' : 'National ID'
  );

  // 0 = NationalId, 1 = Passport (matches backend DocumentType enum)
  protected readonly documentTypeValue = computed(() =>
    this.isInternational() ? 1 : 0
  );

  protected readonly form = this.fb.group({
    passengerName: ['', [Validators.required, Validators.minLength(2)]],
    documentNumber: ['', Validators.required],
  });

  protected goBack(): void {
    this.router.navigate(['/']);
  }

  protected confirm(): void {
    if (this.form.invalid) return;
    const offer = this.state.selectedOffer();
    if (!offer) return;

    this.submitting = true;
    this.submitError = null;

    this.api.createBooking({
      passengerName: this.form.value.passengerName!,
      documentType: this.documentTypeValue(),
      documentNumber: this.form.value.documentNumber!,
      provider: offer.provider,
      flightNumber: offer.flightNumber,
      origin: offer.origin,
      destination: offer.destination,
      departureUtc: offer.departureUtc,
      arrivalUtc: offer.arrivalUtc,
      cabin: CABIN_NUMBER[offer.cabin],
      passengers: offer.passengers,
      totalPrice: offer.totalPrice,
    }).subscribe({
      next: booking => {
        this.state.lastBooking.set(booking);
        this.router.navigate(['/confirmation']);
      },
      error: err => {
        this.submitError = err.error ?? 'Booking failed. Please try again.';
        this.submitting = false;
      },
    });
  }
}
