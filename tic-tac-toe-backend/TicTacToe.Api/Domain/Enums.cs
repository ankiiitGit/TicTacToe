namespace TicTacToe.Api.Domain;

/// <summary>The two players. Serialized to JSON as "X" / "O".</summary>
public enum Player
{
    X,
    O
}

/// <summary>Lifecycle status of a game. Serialized as "InProgress" / "Won" / "Draw".</summary>
public enum GameStatus
{
    InProgress,
    Won,
    Draw
}

/// <summary>Game mode. Serialized as "TwoPlayer" / "Computer".</summary>
public enum GameMode
{
    TwoPlayer,
    Computer
}
