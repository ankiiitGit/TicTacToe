import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Scoreboard } from '../../models/game.models';

/**
 * ScoreboardComponent shows the session tally (X wins / O wins / draws) and a
 * "Reset Scoreboard" button. Like the board, it holds no logic: the parent
 * supplies the numbers and handles the reset event.
 */
@Component({
  selector: 'app-scoreboard',
  imports: [],
  templateUrl: './scoreboard.component.html',
  styleUrl: './scoreboard.component.css',
})
export class ScoreboardComponent {
  /** Current tally, provided by the parent. Defaults to all zeros. */
  @Input() scoreboard: Scoreboard = { xWins: 0, oWins: 0, draws: 0 };

  /** Emitted when the user clicks "Reset Scoreboard". */
  @Output() resetScoreboard = new EventEmitter<void>();
}
