import { TestBed } from '@angular/core/testing';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AppComponent } from './app.component';
import { GameState } from './models/game.models';
import { environment } from '../environments/environment';

/** A minimal fake game state the backend would return. */
const fakeGame: GameState = {
  id: 'test-1',
  board: Array(9).fill(null),
  currentPlayer: 'X',
  mode: 'TwoPlayer',
  status: 'InProgress',
  winner: null,
  winningCells: [],
  moves: [],
  scoreboard: { xWins: 0, oWins: 0, draws: 0 },
};

describe('AppComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent],
      // provideHttpClient + provideHttpClientTesting let us intercept HTTP
      // calls in tests instead of hitting the real backend.
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    // Ensure no unexpected HTTP calls were made.
    httpMock.verify();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(AppComponent);
    expect(fixture.componentInstance).toBeTruthy();

    // ngOnInit fires createGame() -> POST /games. Answer it with a fake game.
    const req = httpMock.expectOne(`${environment.apiBaseUrl}/games`);
    expect(req.request.method).toBe('POST');
    req.flush(fakeGame);
  });

  it('should load a game on init and render the board', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges(); // triggers ngOnInit

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/games`);
    req.flush(fakeGame);
    fixture.detectChanges(); // re-render with the loaded game

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelectorAll('.cell').length).toBe(9);
    expect(fixture.componentInstance.game?.id).toBe('test-1');
  });
});
