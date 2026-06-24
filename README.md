# Tic Tac Toe

A browser-based Tic Tac Toe game with an **Angular** frontend and a **.NET Web API** backend, running locally. Players can play two-player or against a basic computer opponent, with move history, undo, win/draw detection, and a session scoreboard.

---
<img width="574" height="658" alt="image" src="https://github.com/user-attachments/assets/3cd1fa04-8fc0-4b7e-a56b-5d79ddde5f0f" />

---

## 1. Project Overview

This project implements a full Tic Tac Toe application split into two locally-run apps that communicate over a REST API:

- The **Angular frontend** renders the board and UI, sends player actions to the backend, and displays whatever state the backend returns. It contains no game rules of its own.
- The **.NET backend** owns the game session(s), validates every move, detects wins/draws, runs the computer opponent, and maintains the scoreboard.

This separation means the rules are enforced and tested in one place (the backend), and the two sides stay consistent.

```
TicTacToe/
├── tic-tac-toe-frontend/   # Angular 19 + TypeScript SPA (http://localhost:4200)
└── tic-tac-toe-backend/    # .NET 9 Web API + xUnit tests (http://localhost:5000)
```

---

## 2. Tech Stack

| Layer | Technology |
|-------|-----------|
| Frontend | Angular 19, TypeScript, standalone components, RxJS (Behaviour Subject), plain CSS |
| Backend | .NET 9, ASP.NET Core Web API (controllers) |
| API style | REST (JSON over HTTP) |
| Storage | In-memory (singleton store; so that state resets on restart) |
| Backend tests | xUnit |
| Frontend tests | Karma + Jasmine (which is an Angular default) |
| Source control | Git / GitHub |

---

## 3. Features Implemented

1. Game Board
•	A standard 3 × 3 Tic Tac Toe board.
o	Each cell is clickable when it is empty.
o	Once a move is made, the selected cell displays either X or O.
o	A selected cell remains locked for the rest of the current game state.
________________________________________
2. Player Turns
•	The game supports two players:
•	Player X 
•	Player O 
•	The application clearly displays whose turn it is.
•	Players alternate turns after every valid move.
•	Invalid moves does not change the current turn.
________________________________________
3. Win Detection
•	The application detects a winner when a player completes:
•	One full row 
•	One full column 
•	One diagonal 
•	When a player wins, the application:
•	Shows the winning player 
•	Highlights the winning cells 
•	Prevents additional moves for the completed game 
•	Updates the scoreboard 
________________________________________
4. Draw Detection
•	If all 9 cells are filled and there is no winner, the game is marked as a draw.
•	When the game is drawn, the application:
•	Shows a draw message 
•	Prevents additional moves for the completed game 
•	Updates the scoreboard 
________________________________________
5. Reset Game
•	Provides a Reset Game option. Reset Game:
•	Clears the current board 
•	Clears the move history 
•	Clears winner or draw status 
•	Sets the current player back to X 
•	Starts a fresh game session 
•	Keeps the scoreboard unchanged 

---

## 4. How to Run the Backend Locally

**Prerequisites:** [.NET 9 SDK](https://dotnet.microsoft.com/download)

```bash
cd tic-tac-toe-backend/TicTacToe.Api
dotnet run
```

The API starts on **http://localhost:5000**. It allows CORS from the Angular dev server at **http://localhost:4200**.

> The port is configured in `tic-tac-toe-backend/TicTacToe.Api/Properties/launchSettings.json`. If you change it, you will also need to update `apiBaseUrl` in the frontend's `src/environments/environment*.ts`.

---

## 5. How to Run the Frontend Locally

**Prerequisites:** [Node.js](https://nodejs.org/) 18+ and npm

```bash
cd tic-tac-toe-frontend
npm install        # first time only
npm start          # = ng serve
```

Open **http://localhost:4200** in a browser. Make sure the backend is running first, otherwise the UI will show a "Could not reach the backend" message.

---

## 6. API Endpoint Summary

Base URL: `http://localhost:5000/api`

| Method | Endpoint | Purpose |
|--------|----------|---------|
| POST | `/games` | Create a new game session. Body: `{ "mode": "TwoPlayer" \| "Computer" }` |
| GET | `/games/{id}` | Get the current game state |
| POST | `/games/{id}/moves` | Submit a move. Body: `{ "player": "X" \| "O", "row": 0-2, "col": 0-2 }` |
| POST | `/games/{id}/undo` | Undo last move (or move pair in Computer mode) |
| POST | `/games/{id}/reset` | Reset the current game (scoreboard preserved) |
| GET | `/scoreboard` | Get the session scoreboard |
| POST | `/scoreboard/reset` | Reset the scoreboard |

**Game state response shape** (returned by all `/games` endpoints):

```jsonc
{
  "id": "guid-string",
  "board": ["X", null, "O", null, null, null, null, null, null], // 9 cells, index = row*3 + col
  "currentPlayer": "X",
  "mode": "TwoPlayer",
  "status": "InProgress",          // "InProgress" | "Won" | "Draw"
  "winner": null,                  // "X" | "O" | null
  "winningCells": [],              // board indices to highlight, e.g. [0,1,2]
  "moves": [
    { "moveNumber": 1, "player": "X", "row": 0, "col": 0 }
  ],
  "scoreboard": { "xWins": 0, "oWins": 0, "draws": 0 }
}
```

**Validation:** invalid moves return **HTTP 400** with `{ "error": "..." }`. Rejected cases: move outside the board, move on an occupied cell, move after the game is completed, and move by the wrong player. Unknown game ids return **HTTP 404**.

---

## 7. How to Run Tests

**Backend (xUnit) — primary test coverage:**

```bash
cd tic-tac-toe-backend
dotnet test
```

Tests cover: valid/invalid moves, turn switching, row/column/diagonal wins, draw, reset, undo in both modes, scoreboard update (incl. once-only), computer move selection priority, and move-after-completion.

**Frontend (Karma + Jasmine):**

```bash
cd tic-tac-toe-frontend
npm test
```

The `AppComponent` spec verifies the app loads a game on init (with a mocked HTTP backend) and renders the 9 board cells.

---

## 8. AI Tools and Prompt Summary

This project was built with the assistance of AI.

**How it was used:**
- Adding appropriate CSS.
- Implementing Backend and Frontend Tests.
- Writing ReadMe file.

**Representative prompts:**
- *"Add styling using plain CSS for the TicTacToe Board component, Scoreboard and Move-History."*
- *"Write test cases using Angular default approach."*
- *"Write test cases for the tic-tac-toe-backend project."*
- *"Should I use Signals or Zone.js in this project?"* (a design-discussion prompt)
- *"Analyze the requirements and my code (frontend+backend) then Create a README document covering project overview, tech stack, features, run instructions, API summary, tests, design decisions, assumptions, limitations, and future improvements."*

All AI-generated code/text was reviewed, built, and verified.

---

## 9. Design Decisions

- **Backend is the single source of truth.** All rules, validation, win/draw detection, undo, the computer opponent, and the scoreboard live in the backend. The frontend only renders the returned state. This keeps behaviour consistent and centrally testable.
- **Layered backend.** Domain logic (`Game`, `GameEngine`) is separated from HTTP (controllers) and storage (`IGameStore`). The domain has no framework dependencies, which makes it fast and simple to unit-test.
- **Turn state is derived, not stored.** Since X always moves first, `currentPlayer` is computed from the move count (even → X, odd → O). This makes it impossible for the turn to desync from the board.
- **Computer move runs server-side within the same request.** In Computer mode, a single `POST /moves` applies the human's move and the computer's reply, so the frontend stays simple.
- **In-memory storage as a singleton.** In-memory is acceptable as per the requirement; simple to run and review. Hidden behind `IGameStore` for storage so it could be swapped for SQLite without touching controllers.
- **Classic Angular reactivity (not signals).** Plain component properties + `@Input()`/`@Output()` + zone.js change detection were chosen for simplicity; the app's reactivity needs are minimal. (Signals would be a valid alternative and a possible future refactor.)
- **String-based enums in JSON.** Backend enums serialize as strings (`"X"`, `"InProgress"`, `"TwoPlayer"`) to match the frontend's TypeScript string-literal types exactly.

---

## 10. Clarifications and Assumptions

- **Scoreboard & Undo — Option A (Disable Undo After Completion).** Once a game is Won or Drawn, Undo is disabled and the scoreboard result is final. This is enforced in both the backend (`Game.Undo` throws) and the frontend (`canUndo` is false).
- **Computer mode roles:** human is always **X**, computer is always **O**, and X moves first.
- **Scoreboard is session-level** and shared across games; it persists only while the backend process is running.
- **Single concurrent game per browser session** is assumed for the UI, though the backend can hold multiple games by id.
- **No authentication / multi-user separation** — this is a local, single-user demo.
- **Move coordinates** are 0-based (`row`/`col` in `0..2`); the UI displays them 1-based ("Row 1, Column 1").

---

## 11. Known Limitations

- **State is in-memory only** — restarting the backend clears all games and the scoreboard.
- **Computer AI is rule-based, not optimal.** It follows the required win → block → center → corner → any priority, which plays well but is not a full minimax; a perfect player could occasionally force outcomes the heuristic doesn't anticipate.
- **No persistence layer** (SQLite was allowed but not implemented).
- **No real-time/multiplayer** across browsers; each browser drives its own game via the API.

---

## 12. Future Improvements

- Add **SQLite (EF Core)** persistence so games and the scoreboard survive restarts.
- Add a **minimax** (unbeatable) with a higher difficulty option alongside the current mode.
- Convert the frontend to **Angular signals** (`signal()` / `computed()` / `input()` / `output()`) and explore a **zoneless** build.
- Expand **frontend tests** (board interactions, undo/reset flows, error states) and add **backend integration tests** with `WebApplicationFactory`.
- Add **Swagger UI** for interactive API exploration during review.
- Polish UX: animations, sound, keyboard play, and clearer turn/result feedback.
- Add **CI** (GitHub Actions) to build and run both test suites on every push.
```
