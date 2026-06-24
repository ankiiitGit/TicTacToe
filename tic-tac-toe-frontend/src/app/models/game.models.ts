// =============================================================================
// API CONTRACT
// These interfaces describe the exact JSON shape exchanged with the .NET backend.
// They are TypeScript-only: they disappear after compilation and exist purely so
// the compiler can type-check our code and give us autocomplete.
// The backend MUST return objects matching these shapes.
// =============================================================================

/** The two players. A "union of string literals" — a Player can only ever be
 *  the string 'X' or the string 'O', nothing else. */
export type Player = 'X' | 'O';

/** What a single board cell can hold: a player's mark, or null when empty. */
export type CellValue = Player | null;

/** Lifecycle status of a game, mirrored from the backend. */
export type GameStatus = 'InProgress' | 'Won' | 'Draw';

/** The two game modes from the problem statement. */
export type GameMode = 'TwoPlayer' | 'Computer';

/** One entry in the move history. */
export interface Move {
  moveNumber: number; // 1-based order the move was played
  player: Player;     // who played it
  row: number;        // 0-based row (0..2)
  col: number;        // 0-based column (0..2)
}

/** Session-level scoreboard, owned by the backend. */
export interface Scoreboard {
  xWins: number;
  oWins: number;
  draws: number;
}

/** The full game state the backend returns for every game operation.
 *  The frontend renders directly from this — the backend is the source of truth. */
export interface GameState {
  id: string;              // unique game session id
  board: CellValue[];      // flat array of 9 cells; index = row * 3 + col
  currentPlayer: Player;   // whose turn it is now
  mode: GameMode;          // TwoPlayer or Computer
  status: GameStatus;      // InProgress | Won | Draw
  winner: Player | null;   // set when status === 'Won'
  winningCells: number[];  // board indices to highlight; empty when no win
  moves: Move[];           // full move history for this game
  scoreboard: Scoreboard;  // current session scoreboard
}

// ----- Request bodies (what the frontend SENDS) -----------------------------

/** Body for POST /api/games — start a new game in the chosen mode. */
export interface CreateGameRequest {
  mode: GameMode;
}

/** Body for POST /api/games/{id}/moves — submit a move.
 *  We send row+col; the backend validates and updates the board. */
export interface MoveRequest {
  player: Player;
  row: number; // 0-based
  col: number; // 0-based
}
