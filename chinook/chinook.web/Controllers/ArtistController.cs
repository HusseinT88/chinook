using chinook.web.Models;
using Microsoft.AspNetCore.Mvc;

namespace chinook.web.Controllers
{
    public class ArtistController : Controller
    {
        private readonly IRunner _runner;

        public ArtistController(IRunner runner)
        {
            _runner = runner;
        }
        // GET
        public IActionResult Index()
        {
            var sql = "select artist_id as \"Id\",name as \"Name\" from artist;";
            var artists = _runner.Execute<Artist>(sql,null);
            return
                View(artists);
        }

        public IActionResult View(int id)
        {
            var sql = $"select artist_id as \"Id\",name as \"Name\" from artist where artist_id = @0;";
            var artist = _runner.ExecuteSingle<Artist>(sql, new object[] {id});
            return View(artist);
        }
    }
}