using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Domain;
using TicTacToe.Api.Models;
using TicTacToe.Api.Storage;

namespace TicTacToe.Api.Controllers;

/// <summary>
/// REST endpoints for game operations. Thin by design: each action validates
/// minimally, delegates rules to the domain, updates the scoreboard on completion,
/// and returns the full game state.
///
/// [ApiController] enables automatic model binding/validation and JSON responses.
/// [Route("api/[controller]")] => base path "api/games" ([controller] = "Games").
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IGameStore _store;

    // The store is injected by ASP.NET Core's DI (registered in Program.cs).
    public GamesController(IGameStore store) => _store = store;

    /// <summary>POST /api/games — create a new game session.</summary>
    [HttpPost]
    public ActionResult<GameStateResponse> Create([FromBody] CreateGameRequest request)
    {
        var game = _store.CreateGame(request.Mode);
        var response = GameStateResponse.From(game, _store.Scoreboard);
        // 201 Created with a Location header pointing at GET /api/games/{id}.
        return CreatedAtAction(nameof(Get), new { id = game.Id }, response);
    }

    /// <summary>GET /api/games/{id} — fetch the current state of a game.</summary>
    [HttpGet("{id}")]
    public ActionResult<GameStateResponse> Get(string id)
    {
        var game = _store.GetGame(id);
        if (game is null) return NotFound();
        return GameStateResponse.From(game, _store.Scoreboard);
    }

    /// <summary>POST /api/games/{id}/moves — submit a move.</summary>
    [HttpPost("{id}/moves")]
    public ActionResult<GameStateResponse> Move(string id, [FromBody] MoveRequest request)
    {
        var game = _store.GetGame(id);
        if (game is null) return NotFound();

        try
        {
            var wasInProgress = game.Status == GameStatus.InProgress;
            game.ApplyMove(request.Player, request.Row, request.Col);

            // Update the scoreboard exactly once: only when this move ended the game.
            if (wasInProgress && game.Status != GameStatus.InProgress)
            {
                _store.RecordResult(game);
            }

            return GameStateResponse.From(game, _store.Scoreboard);
        }
        catch (InvalidMoveException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>POST /api/games/{id}/undo — undo the last move (or pair in Computer mode).</summary>
    [HttpPost("{id}/undo")]
    public ActionResult<GameStateResponse> Undo(string id)
    {
        var game = _store.GetGame(id);
        if (game is null) return NotFound();

        try
        {
            game.Undo();
            return GameStateResponse.From(game, _store.Scoreboard);
        }
        catch (InvalidMoveException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>POST /api/games/{id}/reset — clear the board; scoreboard is preserved.</summary>
    [HttpPost("{id}/reset")]
    public ActionResult<GameStateResponse> Reset(string id)
    {
        var game = _store.GetGame(id);
        if (game is null) return NotFound();

        game.Reset();
        return GameStateResponse.From(game, _store.Scoreboard);
    }
}
