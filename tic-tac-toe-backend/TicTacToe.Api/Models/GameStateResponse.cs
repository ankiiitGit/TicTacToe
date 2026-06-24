using TicTacToe.Api.Domain;

namespace TicTacToe.Api.Models;

/// <summary>
/// The full game state returned for every game operation. This is the single
/// shape the Angular frontend renders from (see game.models.ts -> GameState).
/// Combines the Game with the shared session Scoreboard.
/// </summary>
public record GameStateResponse(
    string Id,
    Player?[] Board,
    Player CurrentPlayer,
    GameMode Mode,
    GameStatus Status,
    Player? Winner,
    int[] WinningCells,
    IReadOnlyList<Move> Moves,
    Scoreboard Scoreboard)
{
    /// <summary>Maps a domain Game plus the session Scoreboard into the response DTO.</summary>
    public static GameStateResponse From(Game game, Scoreboard scoreboard) => new(
        game.Id,
        game.Board,
        game.CurrentPlayer,
        game.Mode,
        game.Status,
        game.Winner,
        game.WinningCells,
        game.Moves,
        scoreboard);
}
