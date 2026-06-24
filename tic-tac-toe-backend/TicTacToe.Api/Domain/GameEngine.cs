namespace TicTacToe.Api.Domain;

/// <summary>
/// Pure, stateless game-rule helpers. No HTTP, no storage, no mutation of shared
/// state — every method takes a board and returns a result. This is what makes
/// the core rules easy to unit-test in isolation.
///
/// The board is a flat array of 9 cells; index = row * 3 + col:
///     0 | 1 | 2
///     3 | 4 | 5
///     6 | 7 | 8
/// A cell is null when empty, or a Player when occupied.
/// </summary>
public static class GameEngine
{
    /// <summary>All 8 winning lines: 3 rows, 3 columns, 2 diagonals (as cell indices).</summary>
    public static readonly int[][] Lines =
    {
        new[] { 0, 1, 2 }, new[] { 3, 4, 5 }, new[] { 6, 7, 8 }, // rows
        new[] { 0, 3, 6 }, new[] { 1, 4, 7 }, new[] { 2, 5, 8 }, // columns
        new[] { 0, 4, 8 }, new[] { 2, 4, 6 }                     // diagonals
    };

    /// <summary>
    /// Returns the winning player and the three cells forming the line, or
    /// (null, empty) if there is no winner yet.
    /// </summary>
    public static (Player? Winner, int[] Cells) DetectWinner(Player?[] board)
    {
        foreach (var line in Lines)
        {
            var first = board[line[0]];
            if (first != null && board[line[1]] == first && board[line[2]] == first)
            {
                return (first, line);
            }
        }
        return (null, Array.Empty<int>());
    }

    /// <summary>True when every cell is occupied.</summary>
    public static bool IsBoardFull(Player?[] board) => board.All(cell => cell != null);

    /// <summary>
    /// Chooses the computer's move index following the required priority:
    ///   1. If the computer can win now, take that winning cell.
    ///   2. Else if the opponent could win next, block that cell.
    ///   3. Else take the center.
    ///   4. Else take any corner.
    ///   5. Else take any remaining cell.
    /// Returns -1 only if the board is full.
    /// </summary>
    public static int FindComputerMove(Player?[] board, Player computer, Player human)
    {
        // 1. Winning move for the computer.
        var winning = FindWinningMove(board, computer);
        if (winning != -1) return winning;

        // 2. Block the opponent's winning move.
        var block = FindWinningMove(board, human);
        if (block != -1) return block;

        // 3. Center.
        if (board[4] == null) return 4;

        // 4. A corner.
        foreach (var corner in new[] { 0, 2, 6, 8 })
        {
            if (board[corner] == null) return corner;
        }

        // 5. Any available cell.
        for (var i = 0; i < board.Length; i++)
        {
            if (board[i] == null) return i;
        }

        return -1; // board full
    }

    /// <summary>
    /// Returns the index where <paramref name="player"/> would immediately win,
    /// or -1 if no such single move exists. Works by trying each empty cell,
    /// checking for a win, then undoing the trial (no lasting mutation).
    /// </summary>
    private static int FindWinningMove(Player?[] board, Player player)
    {
        for (var i = 0; i < board.Length; i++)
        {
            if (board[i] != null) continue;

            board[i] = player;                  // try
            var (winner, _) = DetectWinner(board);
            board[i] = null;                    // undo the trial

            if (winner == player) return i;
        }
        return -1;
    }
}
