import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CellValue } from '../../models/game.models';

/**
 * BoardComponent renders the 3x3 grid. It is a "presentational" component:
 * it owns no game logic. It receives the board to draw via @Input, and tells
 * its parent which cell was clicked via @Output. The parent decides what to do.
 */
@Component({
  selector: 'app-board',
  imports: [],
  templateUrl: './board.component.html',
  styleUrl: './board.component.css',
})
export class BoardComponent {
  /** The 9 cells to render (index = row * 3 + col). Set by the parent. */
  @Input() board: CellValue[] = Array(9).fill(null);

  /** Indices to highlight as the winning line. */
  @Input() winningCells: number[] = [];

  /** When true, no cell can be clicked (game over, or a request is in flight). */
  @Input() locked = false;

  /** Fires the clicked cell's index up to the parent. */
  @Output() cellClick = new EventEmitter<number>();

  /** Called by the template on click; forwards the index to the parent. */
  onCellClick(index: number): void {
    this.cellClick.emit(index);
  }
}
