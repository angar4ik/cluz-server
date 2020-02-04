using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CLUZServer.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Serilog;

namespace CLUZServer.Controllers
{
    public class DiagController : Controller
    {
        private GamePool _gamePool;
        private readonly IHubContext<PlayersHub> _hubContext;

        public DiagController(GamePool gamePool, IHubContext<PlayersHub> hubContext)
        {
            _gamePool = gamePool;
            _hubContext = hubContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult List()
        {
            List<Game> gameList = _gamePool.Games.Values.ToList();

            //List<Game> gameList = new List<Game> { new Game("Game1", "111", _hubContext) };

            return View(gameList);
        }

        public IActionResult Details(Guid id)
        {
            //Log.Information("GameDetails: '{id}'", id);

            Game game = _gamePool.Games[id];

            return View(game);
        }

        public IActionResult Players(Guid id)
        {
            //Log.Information("Players: '{id}'", id);

            List<Player> players = _gamePool.Games[id].Players.Values.ToList();

            return View(players);
        }
    }
}