namespace TicTacToe.Api.Domain;

/// <summary>
/// Session-level scoreboard. Mutable because counters change over time.
/// Serialized to camelCase JSON: xWins, oWins, draws.
/// </summary>
public class Scoreboard
{
    public int XWins { get; set; }
    public int OWins { get; set; }
    public int Draws { get; set; }
}
