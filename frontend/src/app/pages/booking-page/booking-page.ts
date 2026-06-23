import { Component, computed, inject, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormArray, FormGroup, Validators } from '@angular/forms';
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
export class BookingPageComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  protected readonly state = inject(StateService);
  private readonly api = inject(ApiService);

  protected submitting = false;
  protected submitError: string | null = null;

  protected readonly isInternational = computed(() => {
    const offer = this.state.selectedOffer();
    if (!offer) return false;
    return AIRPORT_COUNTRY[offer.origin] !== AIRPORT_COUNTRY[offer.destination];
  });

  protected readonly documentLabel = computed(() =>
    this.isInternational() ? 'Passport' : 'National ID'
  );

  protected readonly documentTypeValue = computed(() =>
    this.isInternational() ? 1 : 0
  );

  protected readonly form = this.fb.group({
    passengers: this.fb.array([]),
  });

  get passengerArray(): FormArray {
    return this.form.get('passengers') as FormArray;
  }

  get passengerGroups(): FormGroup[] {
    return this.passengerArray.controls as FormGroup[];
  }

  ngOnInit(): void {
    const count = this.state.selectedOffer()?.passengers ?? 1;
    for (let i = 0; i < count; i++) {
      this.passengerArray.push(this.fb.group({
        name: ['', [Validators.required, Validators.minLength(2)]],
        email: ['', [Validators.required, Validators.email]],
        documentNumber: ['', Validators.required],
      }));
    }
  }

  protected ctrl(groupIndex: number, field: string) {
    return this.passengerGroups[groupIndex].get(field);
  }

  protected goBack(): void {
    this.router.navigate(['/']);
  }

  protected confirm(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const offer = this.state.selectedOffer();
    if (!offer) return;

    this.submitting = true;
    this.submitError = null;

    const passengers = this.passengerArray.value.map((p: { name: string; email: string; documentNumber: string }) => ({
      name: p.name,
      email: p.email,
      documentNumber: p.documentNumber,
    }));

    this.api.createBooking({
      passengers,
      documentType: this.documentTypeValue(),
      provider: offer.provider,
      flightNumber: offer.flightNumber,
      origin: offer.origin,
      destination: offer.destination,
      departureUtc: offer.departureUtc,
      arrivalUtc: offer.arrivalUtc,
      cabin: CABIN_NUMBER[offer.cabin],
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
