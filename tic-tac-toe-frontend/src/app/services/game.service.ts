import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  GameState,
  Scoreboard,
  GameMode,
  MoveRequest,
  CreateGameRequest,
} from '../models/game.models';

/**
 * GameService is the ONLY place in the app that talks to the .NET backend.
 *
 * @Injectable marks this class as something Angular's dependency-injection
 * system can create and hand to other classes. `providedIn: 'root'` means
 * there is ONE shared instance for the whole app (a singleton), created lazily
 * the first time it is requested.
 */
@Injectable({ providedIn: 'root' })
export class GameService {
  // `inject(HttpClient)` asks Angular's DI container for the HttpClient we
  // registered in app.config.ts. (This is the modern alternative to declaring
  // it as a constructor parameter — both do the same thing.)
  private readonly http = inject(HttpClient);

  // Base URL for all calls, e.g. http://localhost:5000/api
  private readonly base = environment.apiBaseUrl;

  /**
   * POST /api/games — create a new game session in the given mode.
   * Returns an Observable: a "future stream" of the HTTP response. Nothing
   * actually fires until something `.subscribe()`s to it (components do that,
   * or the async pipe in the template does it for us).
   */
  createGame(mode: GameMode): Observable<GameState> {
    const body: CreateGameRequest = { mode };
    return this.http.post<GameState>(`${this.base}/games`, body);
  }

  /** GET /api/games/{id} — fetch the current state of a game. */
  getGame(id: string): Observable<GameState> {
    return this.http.get<GameState>(`${this.base}/games/${id}`);
  }

  /** POST /api/games/{id}/moves — submit a move; backend validates it. */
  makeMove(id: string, move: MoveRequest): Observable<GameState> {
    return this.http.post<GameState>(`${this.base}/games/${id}/moves`, move);
  }

  /** POST /api/games/{id}/undo — undo the last move (or move pair in Computer mode). */
  undo(id: string): Observable<GameState> {
    return this.http.post<GameState>(`${this.base}/games/${id}/undo`, {});
  }

  /** POST /api/games/{id}/reset — clear the board but keep the scoreboard. */
  resetGame(id: string): Observable<GameState> {
    return this.http.post<GameState>(`${this.base}/games/${id}/reset`, {});
  }

  /** GET /api/scoreboard — read the session scoreboard. */
  getScoreboard(): Observable<Scoreboard> {
    return this.http.get<Scoreboard>(`${this.base}/scoreboard`);
  }

  /** POST /api/scoreboard/reset — zero out X wins / O wins / draws. */
  resetScoreboard(): Observable<Scoreboard> {
    return this.http.post<Scoreboard>(`${this.base}/scoreboard/reset`, {});
  }
}
