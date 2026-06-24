namespace TicTacToe.Api.Domain;

/// <summary>
/// One entry in a game's move history. A positional <c>record</c>: an immutable
/// value object whose properties (MoveNumber, Player, Row, Col) are set once at
/// creation. Serialized to camelCase JSON: moveNumber, player, row, col.
/// Row and Col are 0-based (0..2); board index = Row * 3 + Col.
/// </summary>
public record Move(int MoveNumber, Player Player, int Row, int Col);
