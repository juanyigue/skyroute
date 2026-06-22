import { Component, inject } from '@angular/core';
import { SearchFormComponent } from '../../components/search-form/search-form';
import { ResultsListComponent } from '../../components/results-list/results-list';
import { StateService } from '../../services/state.service';

@Component({
  selector: 'app-search-page',
  imports: [SearchFormComponent, ResultsListComponent],
  templateUrl: './search-page.html',
  styleUrl: './search-page.scss',
})
export class SearchPageComponent {
  protected readonly state = inject(StateService);
}
