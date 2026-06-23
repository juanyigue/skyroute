import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { SearchFormComponent } from './search-form';
import { ApiService } from '../../services/api.service';
import { StateService } from '../../services/state.service';
import { ParsedSearchQuery } from '../../models/flight.model';

const parsed = (override: Partial<ParsedSearchQuery> = {}): ParsedSearchQuery => ({
  origin: null, destination: null, departureDate: null, passengers: null, cabin: null,
  ...override,
});

describe('SearchFormComponent', () => {
  let fixture: ComponentFixture<SearchFormComponent>;
  let component: SearchFormComponent;
  let api: { parseSearch: ReturnType<typeof vi.fn>; searchFlights: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    api = { parseSearch: vi.fn(), searchFlights: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [SearchFormComponent],
      providers: [
        { provide: ApiService, useValue: api },
        StateService,
        provideRouter([]),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(SearchFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('creates the component', () => {
    expect(component).toBeTruthy();
  });

  it('form is initially invalid', () => {
    expect((component as any).form.invalid).toBe(true);
  });

  it('autofill does nothing when aiText is empty', () => {
    (component as any).aiText = '';
    (component as any).autofill();
    expect(api.parseSearch).not.toHaveBeenCalled();
  });

  it('autofill patches form and marks parseFilled on success', () => {
    api.parseSearch.mockReturnValue(of(parsed({ origin: 'JFK', destination: 'LHR', passengers: 2, cabin: 'Business', departureDate: '2026-09-01' })));
    (component as any).aiText = 'JFK to London 2 passengers business';
    (component as any).autofill();

    expect((component as any).form.value.origin).toBe('JFK');
    expect((component as any).form.value.destination).toBe('LHR');
    expect((component as any).form.value.passengers).toBe(2);
    expect((component as any).form.value.cabin).toBe('Business');
    expect((component as any).parseFilled()).toBe(true);
    expect((component as any).parseError()).toBeNull();
  });

  it('autofill tracks which fields were filled', () => {
    api.parseSearch.mockReturnValue(of(parsed({ origin: 'JFK', destination: 'LHR' })));
    (component as any).aiText = 'JFK to LHR';
    (component as any).autofill();

    const filled: Set<string> = (component as any).filledFields();
    expect(filled.has('origin')).toBe(true);
    expect(filled.has('destination')).toBe(true);
    expect(filled.has('passengers')).toBe(false);
  });

  it('autofill sets parseError on API failure', () => {
    api.parseSearch.mockReturnValue(throwError(() => new Error('network')));
    (component as any).aiText = 'some text';
    (component as any).autofill();

    expect((component as any).parseError()).toBe('Could not parse — fill the fields manually.');
    expect((component as any).parseFilled()).toBe(false);
  });
});
