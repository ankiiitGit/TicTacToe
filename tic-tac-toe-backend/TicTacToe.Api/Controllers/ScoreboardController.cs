using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Domain;
using TicTacToe.Api.Storage;

namespace TicTacToe.Api.Controllers;

/// <summary>
/// REST endpoints for the session scoreboard. Base path "api/scoreboard".
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ScoreboardController : ControllerBase
{
    private readonly IGameStore _store;

    public ScoreboardController(IGameStore store) => _store = store;

    /// <summary>GET /api/scoreboard — read the current tally.</summary>
    [HttpGet]
    public ActionResult<Scoreboard> Get() => _store.Scoreboard;

    /// <summary>POST /api/scoreboard/reset — zero the tally.</summary>
    [HttpPost("reset")]
    public ActionResult<Scoreboard> Reset()
    {
        _store.ResetScoreboard();
        return _store.Scoreboard;
    }
}
