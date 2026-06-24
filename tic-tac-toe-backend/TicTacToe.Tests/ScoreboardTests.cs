using TicTacToe.Api.Domain;
using TicTacToe.Api.Storage;
using Xunit;

namespace TicTacToe.Tests;

/// <summary>
/// Tests for scoreboard behaviour via the in-memory store. The helper mirrors
/// exactly what GamesController does: apply a move, and record the result only on
/// the transition from InProgress to a finished state (so it counts once).
/// </summary>
public class ScoreboardTests
{
    private static void ApplyAndScore(Game game, IGameStore store, int index)
    {
        var wasInProgress = game.Status == GameStatus.InProgress;
        game.ApplyMove(game.CurrentPlayer, index / 3, index % 3);
        if (wasInProgress && game.Status != GameStatus.InProgress)
        {
            store.RecordResult(game);
        }
    }

    [Fact]
    public void Scoreboard_StartsAtZero()
    {
        var store = new InMemoryGameStore();
        Assert.Equal(0, store.Scoreboard.XWins);
        Assert.Equal(0, store.Scoreboard.OWins);
        Assert.Equal(0, store.Scoreboard.Draws);
    }

    [Fact]
    public void Scoreboard_IncrementsXWins_OnXVictory()
    {
        var store = new InMemoryGameStore();
        var game = store.CreateGame(GameMode.TwoPlayer);

        // X wins the top row: X@0,1,2 with O@3,4 between.
        foreach (var i in new[] { 0, 3, 1, 4, 2 })
        {
            ApplyAndScore(game, store, i);
        }

        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal(1, store.Scoreboard.XWins);
        Assert.Equal(0, store.Scoreboard.OWins);
        Assert.Equal(0, store.Scoreboard.Draws);
    }

    [Fact]
    public void Scoreboard_IncrementsDraws_OnDraw()
    {
        var store = new InMemoryGameStore();
        var game = store.CreateGame(GameMode.TwoPlayer);

        foreach (var i in new[] { 0, 1, 2, 5, 3, 6, 4, 8, 7 }) // full board, no winner
        {
            ApplyAndScore(game, store, i);
        }

        Assert.Equal(GameStatus.Draw, game.Status);
        Assert.Equal(1, store.Scoreboard.Draws);
    }

    [Fact]
    public void Scoreboard_UpdatesOnlyOnce_PerCompletedGame()
    {
        var store = new InMemoryGameStore();
        var game = store.CreateGame(GameMode.TwoPlayer);

        foreach (var i in new[] { 0, 3, 1, 4, 2 }) // X wins
        {
            ApplyAndScore(game, store, i);
        }
        Assert.Equal(1, store.Scoreboard.XWins);

        // A further move after completion is rejected, so the score can't grow.
        Assert.Throws<InvalidMoveException>(() =>
            game.ApplyMove(Player.O, 2, 2));
        Assert.Equal(1, store.Scoreboard.XWins);
    }

    [Fact]
    public void ResetScoreboard_ZeroesCounters()
    {
        var store = new InMemoryGameStore();
        store.Scoreboard.XWins = 3;
        store.Scoreboard.OWins = 2;
        store.Scoreboard.Draws = 1;

        store.ResetScoreboard();

        Assert.Equal(0, store.Scoreboard.XWins);
        Assert.Equal(0, store.Scoreboard.OWins);
        Assert.Equal(0, store.Scoreboard.Draws);
    }

    [Fact]
    public void ResetGame_DoesNotChangeScoreboard()
    {
        var store = new InMemoryGameStore();
        var game = store.CreateGame(GameMode.TwoPlayer);

        foreach (var i in new[] { 0, 3, 1, 4, 2 }) // X wins -> XWins = 1
        {
            ApplyAndScore(game, store, i);
        }

        game.Reset();

        Assert.Equal(1, store.Scoreboard.XWins); // unchanged by reset
        Assert.Equal(GameStatus.InProgress, game.Status);
    }
}
