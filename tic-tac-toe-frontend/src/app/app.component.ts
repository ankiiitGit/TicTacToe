import { Component, OnInit, inject } from '@angular/core';
import { BoardComponent } from './components/board/board.component';
import { ScoreboardComponent } from './components/scoreboard/scoreboard.component';
import { MoveHistoryComponent } from './components/move-history/move-history.component';
import { GameService } from './services/game.service';
import { GameState, GameMode } from './models/game.models';

/**
 * AppComponent is the root + orchestrator. It:
 *   - holds the current GameState (received from the backend),
 *   - reacts to user actions (clicks, mode switch, undo, reset),
 *   - calls GameService for every action,
 *   - feeds the latest state down into the child components.
 *
 * The backend is the source of truth: after each action we simply replace our
 * local `game` with whatever the backend returns and let the UI re-render.
 *
 * `implements OnInit` lets us run code once when the component is created,
 * via the ngOnInit() lifecycle hook (used here to start the first game).
 */
@Component({
  selector: 'app-root',
  imports: [BoardComponent, ScoreboardComponent, MoveHistoryComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent implements OnInit {
  private readonly gameService = inject(GameService);

  /** The current game from the backend. null while the first game loads. */
  game: GameState | null = null;

  /** Mode chosen in the UI; used when starting a new game. */
  mode: GameMode = 'TwoPlayer';

  /** True while an HTTP request is in flight (used to lock the board). */
  loading = false;

  /** User-facing error message when the backend can't be reached. */
  error: string | null = null;

  /** Lifecycle hook: runs once after construction. Start a fresh game. */
  ngOnInit(): void {
    this.startNewGame(this.mode);
  }

  /** Create a brand-new game session in the given mode. */
  startNewGame(mode: GameMode): void {
    this.mode = mode;
    this.loading = true;
    this.gameService.createGame(mode).subscribe({
      next: (state) => this.applyState(state),
      error: () => this.handleError(),
    });
  }

  /** Switch mode (Two Player <-> Computer). Each switch starts a fresh game. */
  onModeChange(mode: GameMode): void {
    if (mode !== this.mode) {
      this.startNewGame(mode);
    }
  }

  /**
   * Handle a cell click coming up from BoardComponent.
   * `index` is 0..8; convert it to row/col for the request. The player is
   * whoever's turn the backend says it is. We send the move and adopt the
   * returned state (which, in Computer mode, already includes the computer's
   * reply move).
   */
  onCellClick(index: number): void {
    if (!this.game || this.game.status !== 'InProgress' || this.loading) {
      return;
    }
    const row = Math.floor(index / 3);
    const col = index % 3;
    this.loading = true;
    this.gameService
      .makeMove(this.game.id, { player: this.game.currentPlayer, row, col })
      .subscribe({
        next: (state) => this.applyState(state),
        error: () => this.handleError(),
      });
  }

  /** Undo the last move (or move pair in Computer mode). */
  onUndo(): void {
    if (!this.game || !this.canUndo) {
      return;
    }
    this.loading = true;
    this.gameService.undo(this.game.id).subscribe({
      next: (state) => this.applyState(state),
      error: () => this.handleError(),
    });
  }

  /** Reset the board for the current session; scoreboard is preserved by the backend. */
  onResetGame(): void {
    if (!this.game) {
      return;
    }
    this.loading = true;
    this.gameService.resetGame(this.game.id).subscribe({
      next: (state) => this.applyState(state),
      error: () => this.handleError(),
    });
  }

  /** Reset only the scoreboard counters. */
  onResetScoreboard(): void {
    this.loading = true;
    this.gameService.resetScoreboard().subscribe({
      next: (scoreboard) => {
        if (this.game) {
          this.game = { ...this.game, scoreboard };
        }
        this.loading = false;
      },
      error: () => this.handleError(),
    });
  }

  // ---- Derived view helpers (called from the template) ----------------------

  /** Option A: Undo is disabled once a game is Won or Drawn, and when empty. */
  get canUndo(): boolean {
    return (
      !!this.game &&
      this.game.status === 'InProgress' &&
      this.game.moves.length > 0 &&
      !this.loading
    );
  }

  /** Block board clicks when the game is over or a request is pending. */
  get boardLocked(): boolean {
    return !this.game || this.game.status !== 'InProgress' || this.loading;
  }

  // ---- Private helpers ------------------------------------------------------

  private applyState(state: GameState): void {
    this.game = state;
    this.loading = false;
    this.error = null;
  }

  private handleError(): void {
    this.loading = false;
    this.error =
      'Could not reach the backend at ' +
      'http://localhost:5000. Make sure the .NET API is running.';
  }
}
