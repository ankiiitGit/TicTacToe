using TicTacToe.Api.Domain;

namespace TicTacToe.Api.Models;

/// <summary>Body for POST /api/games. Matches the frontend CreateGameRequest.</summary>
public record CreateGameRequest(GameMode Mode);

/// <summary>
/// Body for POST /api/games/{id}/moves. Matches the frontend MoveRequest.
/// Row and Col are 0-based.
/// </summary>
public record MoveRequest(Player Player, int Row, int Col);
