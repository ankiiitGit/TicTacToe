using System.Collections.Concurrent;
using TicTacToe.Api.Domain;

namespace TicTacToe.Api.Storage;

/// <summary>
/// In-memory implementation of <see cref="IGameStore"/>. Registered as a singleton,
/// so one instance holds all games and the scoreboard for the lifetime of the
/// running server. State is lost on restart (acceptable per the problem statement).
///
/// ConcurrentDictionary makes game storage thread-safe; a lock guards the
/// scoreboard counter increments.
/// </summary>
public class InMemoryGameStore : IGameStore
{
    private readonly ConcurrentDictionary<string, Game> _games = new();
    private readonly object _scoreLock = new();

    public Scoreboard Scoreboard { get; } = new();

    public Game CreateGame(GameMode mode)
    {
        var game = new Game(Guid.NewGuid().ToString(), mode);
        _games[game.Id] = game;
        return game;
    }

    public Game? GetGame(string id) => _games.TryGetValue(id, out var game) ? game : null;

    public void RecordResult(Game game)
    {
        lock (_scoreLock)
        {
            switch (game.Status)
            {
                case GameStatus.Won when game.Winner == Player.X:
                    Scoreboard.XWins++;
                    break;
                case GameStatus.Won when game.Winner == Player.O:
                    Scoreboard.OWins++;
                    break;
                case GameStatus.Draw:
                    Scoreboard.Draws++;
                    break;
            }
        }
    }

    public void ResetScoreboard()
    {
        lock (_scoreLock)
        {
            Scoreboard.XWins = 0;
            Scoreboard.OWins = 0;
            Scoreboard.Draws = 0;
        }
    }
}
