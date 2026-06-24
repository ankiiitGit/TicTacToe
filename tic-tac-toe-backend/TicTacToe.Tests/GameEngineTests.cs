using TicTacToe.Api.Domain;
using Xunit;

namespace TicTacToe.Tests;

/// <summary>
/// Unit tests for the pure rule helpers in GameEngine: win detection and the
/// computer move-selection priority.
/// </summary>
public class GameEngineTests
{
    // Builds a 9-cell board from a 9-char string. 'X'/'O' set a mark, any other
    // char (e.g. '.') leaves the cell empty. Index = row * 3 + col.
    private static Player?[] Board(string layout)
    {
        var board = new Player?[9];
        for (var i = 0; i < 9; i++)
        {
            board[i] = layout[i] switch
            {
                'X' => Player.X,
                'O' => Player.O,
                _ => null
            };
        }
        return board;
    }

    [Fact]
    public void DetectWinner_FindsRow()
    {
        var (winner, cells) = GameEngine.DetectWinner(Board("XXX......"));
        Assert.Equal(Player.X, winner);
        Assert.Equal(new[] { 0, 1, 2 }, cells);
    }

    [Fact]
    public void DetectWinner_FindsColumn()
    {
        var (winner, cells) = GameEngine.DetectWinner(Board("O..O..O.."));
        Assert.Equal(Player.O, winner);
        Assert.Equal(new[] { 0, 3, 6 }, cells);
    }

    [Fact]
    public void DetectWinner_FindsDiagonal()
    {
        var (winner, cells) = GameEngine.DetectWinner(Board("X...X...X"));
        Assert.Equal(Player.X, winner);
        Assert.Equal(new[] { 0, 4, 8 }, cells);
    }

    [Fact]
    public void DetectWinner_NoWinner_ReturnsNull()
    {
        var (winner, cells) = GameEngine.DetectWinner(Board("XO......."));
        Assert.Null(winner);
        Assert.Empty(cells);
    }

    [Fact]
    public void Computer_Priority1_TakesWinningMove()
    {
        // O has two in the top row; the winning cell is index 2.
        var move = GameEngine.FindComputerMove(Board("OO......."), Player.O, Player.X);
        Assert.Equal(2, move);
    }

    [Fact]
    public void Computer_Priority2_BlocksOpponentWin()
    {
        // X threatens the top row at index 2; O cannot win, so it must block at 2.
        var move = GameEngine.FindComputerMove(Board("XX......."), Player.O, Player.X);
        Assert.Equal(2, move);
    }

    [Fact]
    public void Computer_PrefersWin_OverBlock()
    {
        // O can win at 2 (OO.) AND X threatens at 5 (XX in row 2). Win wins.
        var move = GameEngine.FindComputerMove(Board("OO.XX...."), Player.O, Player.X);
        Assert.Equal(2, move);
    }

    [Fact]
    public void Computer_Priority3_TakesCenter()
    {
        // No win, no block, center (4) is free -> take it.
        var move = GameEngine.FindComputerMove(Board("X........"), Player.O, Player.X);
        Assert.Equal(4, move);
    }

    [Fact]
    public void Computer_Priority4_TakesCorner_WhenCenterTaken()
    {
        // Center taken, no threats -> take a corner (index 0).
        var move = GameEngine.FindComputerMove(Board("....X...."), Player.O, Player.X);
        Assert.Equal(0, move);
    }
}
