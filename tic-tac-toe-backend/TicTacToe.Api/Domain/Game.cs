namespace TicTacToe.Api.Domain;

/// <summary>
/// A single game session: its board, move history, status, and the rules for
/// applying moves, undoing, and resetting. In Computer mode the human is X and
/// the computer is O; the computer replies automatically inside ApplyMove.
///
/// Invariant: X always moves first, so the player to move is derived purely from
/// how many moves have been played (even count => X, odd => O). This keeps the
/// turn state impossible to desync.
/// </summary>
public class Game
{
    public const Player Human = Player.X;
    public const Player Computer = Player.O;

    public string Id { get; }
    public GameMode Mode { get; }

    /// <summary>9 cells, index = row * 3 + col; null = empty.</summary>
    public Player?[] Board { get; private set; } = new Player?[9];

    public List<Move> Moves { get; private set; } = new();

    public GameStatus Status { get; private set; } = GameStatus.InProgress;
    public Player? Winner { get; private set; }
    public int[] WinningCells { get; private set; } = Array.Empty<int>();

    /// <summary>Whose turn it is, derived from the move count (X goes first).</summary>
    public Player CurrentPlayer => Moves.Count % 2 == 0 ? Player.X : Player.O;

    public Game(string id, GameMode mode)
    {
        Id = id;
        Mode = mode;
    }

    /// <summary>
    /// Validates and applies a player's move. In Computer mode, if the game is
    /// still in progress afterwards and it's the computer's turn, the computer
    /// also plays in the same call. Throws <see cref="InvalidMoveException"/> on
    /// any rule violation.
    /// </summary>
    public void ApplyMove(Player player, int row, int col)
    {
        if (Status != GameStatus.InProgress)
            throw new InvalidMoveException("The game is already completed.");

        if (row is < 0 or > 2 || col is < 0 or > 2)
            throw new InvalidMoveException("Move is outside the board.");

        if (player != CurrentPlayer)
            throw new InvalidMoveException($"It is player {CurrentPlayer}'s turn.");

        var index = row * 3 + col;
        if (Board[index] != null)
            throw new InvalidMoveException("That cell is already occupied.");

        PlaceMark(player, row, col);

        // Computer mode: let O reply automatically while the game is in progress.
        if (Mode == GameMode.Computer &&
            Status == GameStatus.InProgress &&
            CurrentPlayer == Computer)
        {
            var aiIndex = GameEngine.FindComputerMove(Board, Computer, Human);
            if (aiIndex >= 0)
            {
                PlaceMark(Computer, aiIndex / 3, aiIndex % 3);
            }
        }
    }

    /// <summary>
    /// Undo the last move. Two Player mode removes one move; Computer mode removes
    /// the pair (computer's move + the human's previous move). Disabled once the
    /// game is over (Option A) and when there is nothing to undo.
    /// </summary>
    public void Undo()
    {
        if (Status != GameStatus.InProgress)
            throw new InvalidMoveException("Undo is disabled after the game is completed.");

        if (Moves.Count == 0)
            throw new InvalidMoveException("There are no moves to undo.");

        var toRemove = Mode == GameMode.Computer ? 2 : 1;
        toRemove = Math.Min(toRemove, Moves.Count);

        Moves.RemoveRange(Moves.Count - toRemove, toRemove);
        RebuildFromMoves();
    }

    /// <summary>
    /// Reset for a fresh game: clears the board, history, and result, and sets the
    /// turn back to X. Keeps the same Id and Mode. Does NOT touch the scoreboard
    /// (the scoreboard lives outside the game).
    /// </summary>
    public void Reset()
    {
        Board = new Player?[9];
        Moves = new List<Move>();
        Status = GameStatus.InProgress;
        Winner = null;
        WinningCells = Array.Empty<int>();
    }

    // ---- internals -----------------------------------------------------------

    private void PlaceMark(Player player, int row, int col)
    {
        Board[row * 3 + col] = player;
        Moves.Add(new Move(Moves.Count + 1, player, row, col));
        Recalculate();
    }

    private void RebuildFromMoves()
    {
        Board = new Player?[9];
        foreach (var move in Moves)
        {
            Board[move.Row * 3 + move.Col] = move.Player;
        }
        Recalculate();
    }

    /// <summary>Re-derives Status / Winner / WinningCells from the current board.</summary>
    private void Recalculate()
    {
        var (winner, cells) = GameEngine.DetectWinner(Board);
        if (winner != null)
        {
            Winner = winner;
            WinningCells = cells;
            Status = GameStatus.Won;
        }
        else if (GameEngine.IsBoardFull(Board))
        {
            Winner = null;
            WinningCells = Array.Empty<int>();
            Status = GameStatus.Draw;
        }
        else
        {
            Winner = null;
            WinningCells = Array.Empty<int>();
            Status = GameStatus.InProgress;
        }
    }
}
