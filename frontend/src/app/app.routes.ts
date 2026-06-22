import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/search-page/search-page').then(m => m.SearchPageComponent),
  },
  {
    path: 'book',
    loadComponent: () =>
      import('./pages/booking-page/booking-page').then(m => m.BookingPageComponent),
  },
  {
    path: 'confirmation',
    loadComponent: () =>
      import('./pages/confirmation-page/confirmation-page').then(m => m.ConfirmationPageComponent),
  },
  { path: '**', redirectTo: '' },
];
