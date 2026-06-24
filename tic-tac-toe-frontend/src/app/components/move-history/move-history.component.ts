import { Component, Input } from '@angular/core';
import { Move } from '../../models/game.models';

/**
 * MoveHistoryComponent renders the move list for the current game as a table:
 * Move # | Player | Position. It only displays data given by the parent.
 */
@Component({
  selector: 'app-move-history',
  imports: [],
  templateUrl: './move-history.component.html',
  styleUrl: './move-history.component.css',
})
export class MoveHistoryComponent {
  /** The current game's move history, provided by the parent. */
  @Input() moves: Move[] = [];
}
