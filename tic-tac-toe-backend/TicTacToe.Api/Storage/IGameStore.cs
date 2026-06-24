using TicTacToe.Api.Domain;

namespace TicTacToe.Api.Storage;

/// <summary>
/// Owns all server-side state: the live games and the session scoreboard.
/// An interface so the storage strategy (in-memory now, SQLite later) can change
/// without touching the controllers.
/// </summary>
public interface IGameStore
{
    /// <summary>Creates and stores a new game in the given mode.</summary>
    Game CreateGame(GameMode mode);

    /// <summary>Returns the game with the given id, or null if it doesn't exist.</summary>
    Game? GetGame(string id);

    /// <summary>The single, session-level scoreboard shared across all games.</summary>
    Scoreboard Scoreboard { get; }

    /// <summary>Updates the scoreboard from a just-completed game (win or draw).</summary>
    void RecordResult(Game game);

    /// <summary>Zeroes the scoreboard counters.</summary>
    void ResetScoreboard();
}
