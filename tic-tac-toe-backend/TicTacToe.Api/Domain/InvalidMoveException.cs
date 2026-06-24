namespace TicTacToe.Api.Domain;

/// <summary>
/// Thrown by the domain when a requested action breaks the game rules
/// (occupied cell, wrong player, game already over, out of bounds, nothing to undo).
/// The controller catches this and returns HTTP 400 with the message.
/// </summary>
public class InvalidMoveException : Exception
{
    public InvalidMoveException(string message) : base(message) { }
}
