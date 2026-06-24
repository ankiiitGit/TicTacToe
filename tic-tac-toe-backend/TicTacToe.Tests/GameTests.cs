using TicTacToe.Api.Domain;
using Xunit;

namespace TicTacToe.Tests;

/// <summary>
/// Unit tests for the core Game rules and state transitions. These exercise the
/// domain directly (no HTTP), which is the fast, reliable way to prove the rules.
/// </summary>
public class GameTests
{
    // Helper: play a list of board indices (0..8), alternating X/O automatically
    // starting with X. Used to drive games concisely in Two Player mode.
    private static void Play(Game game, params int[] indices)
    {
        foreach (var index in indices)
        {
            game.ApplyMove(game.CurrentPlayer, index / 3, index % 3);
        }
    }

    private static Game NewTwoPlayer() => new("g1", GameMode.TwoPlayer);

    [Fact]
    public void ValidMove_PlacesMark_AndRecordsHistory()
    {
        var game = NewTwoPlayer();

        game.ApplyMove(Player.X, 0, 0);

        Assert.Equal(Player.X, game.Board[0]);
        Assert.Single(game.Moves);
        Assert.Equal(new Move(1, Player.X, 0, 0), game.Moves[0]);
        Assert.Equal(GameStatus.InProgress, game.Status);
    }

    [Fact]
    public void Turns_Alternate_AfterValidMoves()
    {
        var game = NewTwoPlayer();
        Assert.Equal(Player.X, game.CurrentPlayer);

        game.ApplyMove(Player.X, 0, 0);
        Assert.Equal(Player.O, game.CurrentPlayer);

        game.ApplyMove(Player.O, 1, 1);
        Assert.Equal(Player.X, game.CurrentPlayer);
    }

    [Fact]
    public void InvalidMove_OccupiedCell_Throws_AndDoesNotChangeTurn()
    {
        var game = NewTwoPlayer();
        game.ApplyMove(Player.X, 0, 0);

        Assert.Throws<InvalidMoveException>(() => game.ApplyMove(Player.O, 0, 0));
        // Turn unchanged: still O's turn after the rejected move.
        Assert.Equal(Player.O, game.CurrentPlayer);
    }

    [Fact]
    public void InvalidMove_WrongPlayer_Throws()
    {
        var game = NewTwoPlayer();
        // It's X's turn; O tries to move.
        Assert.Throws<InvalidMoveException>(() => game.ApplyMove(Player.O, 0, 0));
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, 3)]
    [InlineData(3, 3)]
    public void InvalidMove_OutsideBoard_Throws(int row, int col)
    {
        var game = NewTwoPlayer();
        Assert.Throws<InvalidMoveException>(() => game.ApplyMove(Player.X, row, col));
    }

    [Fact]
    public void RowWin_IsDetected_WithWinningCells()
    {
        var game = NewTwoPlayer();
        // X: 0,1,2 (top row).  O: 3,4 in between.
        Play(game, 0, 3, 1, 4, 2);

        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal(Player.X, game.Winner);
        Assert.Equal(new[] { 0, 1, 2 }, game.WinningCells);
    }

    [Fact]
    public void ColumnWin_IsDetected()
    {
        var game = NewTwoPlayer();
        // X: 0,3,6 (left column).  O: 1,2.
        Play(game, 0, 1, 3, 2, 6);

        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal(Player.X, game.Winner);
        Assert.Equal(new[] { 0, 3, 6 }, game.WinningCells);
    }

    [Fact]
    public void DiagonalWin_IsDetected()
    {
        var game = NewTwoPlayer();
        // X: 0,4,8 (main diagonal).  O: 1,2.
        Play(game, 0, 1, 4, 2, 8);

        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal(Player.X, game.Winner);
        Assert.Equal(new[] { 0, 4, 8 }, game.WinningCells);
    }

    [Fact]
    public void Draw_IsDetected_WhenBoardFull_WithNoWinner()
    {
        var game = NewTwoPlayer();
        // A full board with no three-in-a-row.
        // X O X / X X O / O X O
        Play(game, 0, 1, 2, 5, 3, 6, 4, 8, 7);

        Assert.Equal(GameStatus.Draw, game.Status);
        Assert.Null(game.Winner);
        Assert.Empty(game.WinningCells);
    }

    [Fact]
    public void MoveAfterCompletion_IsRejected()
    {
        var game = NewTwoPlayer();
        Play(game, 0, 3, 1, 4, 2); // X wins
        Assert.Equal(GameStatus.Won, game.Status);

        // Any further move must be rejected.
        Assert.Throws<InvalidMoveException>(() => game.ApplyMove(Player.O, 2, 2));
    }

    [Fact]
    public void Reset_ClearsBoardAndHistory_ButKeepsIdAndMode()
    {
        var game = NewTwoPlayer();
        Play(game, 0, 3, 1, 4, 2); // X wins

        game.Reset();

        Assert.Equal("g1", game.Id);
        Assert.Equal(GameMode.TwoPlayer, game.Mode);
        Assert.All(game.Board, cell => Assert.Null(cell));
        Assert.Empty(game.Moves);
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Equal(Player.X, game.CurrentPlayer);
        Assert.Null(game.Winner);
    }

    [Fact]
    public void Undo_TwoPlayerMode_RemovesOnlyLastMove()
    {
        var game = NewTwoPlayer();
        game.ApplyMove(Player.X, 0, 0);
        game.ApplyMove(Player.O, 1, 1);
        Assert.Equal(Player.X, game.CurrentPlayer);

        game.Undo();

        // O's move was removed; it's O's turn again.
        Assert.Single(game.Moves);
        Assert.Equal(Player.X, game.Board[0]);
        Assert.Null(game.Board[4]);
        Assert.Equal(Player.O, game.CurrentPlayer);
    }

    [Fact]
    public void Undo_ComputerMode_RemovesComputerAndHumanMovesTogether()
    {
        var game = new Game("g2", GameMode.Computer);
        // Human X plays; computer O replies automatically -> 2 moves total.
        game.ApplyMove(Player.X, 0, 0);
        Assert.Equal(2, game.Moves.Count);

        game.Undo();

        // Both moves removed; board empty; back to X's turn.
        Assert.Empty(game.Moves);
        Assert.All(game.Board, cell => Assert.Null(cell));
        Assert.Equal(Player.X, game.CurrentPlayer);
    }

    [Fact]
    public void Undo_WithNoMoves_Throws()
    {
        var game = NewTwoPlayer();
        Assert.Throws<InvalidMoveException>(() => game.Undo());
    }

    [Fact]
    public void Undo_AfterCompletion_IsDisabled_OptionA()
    {
        var game = NewTwoPlayer();
        Play(game, 0, 3, 1, 4, 2); // X wins
        Assert.Throws<InvalidMoveException>(() => game.Undo());
    }

    [Fact]
    public void ComputerMode_PlaysAutomatically_AfterHumanMove()
    {
        var game = new Game("g3", GameMode.Computer);
        game.ApplyMove(Player.X, 0, 0); // human X

        Assert.Equal(2, game.Moves.Count);           // human + computer
        Assert.Equal(Player.O, game.Moves[1].Player); // computer is O
        Assert.Equal(Player.X, game.CurrentPlayer);   // back to human
    }
}
