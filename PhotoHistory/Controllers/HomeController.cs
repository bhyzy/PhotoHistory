using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PhotoHistory.Models;
using PhotoHistory.Data;

namespace PhotoHistory.Controllers
{
	public class HomeController : Controller
	{
        private const int ALBUMS_IN_CATEGORY = 2;

		[Authorize]
		public ActionResult Index()
		{
            
            List<HomepageAlbumModel> models = new List<HomepageAlbumModel>();
            AlbumRepository repo = new AlbumRepository();

            models.Add(new HomepageAlbumModel()
            {
                Name = "Popular",
                Albums = Helpers.Convert(repo.GetMostPopular(ALBUMS_IN_CATEGORY))
            });

            models.Add(new HomepageAlbumModel()
            {
                Name = "Recently commented",
                Albums = Helpers.Convert(repo.GetRecentlyCommented(ALBUMS_IN_CATEGORY))
            });

            models.Add(new HomepageAlbumModel()
            {
                Name = "Top rated",
                Albums = Helpers.Convert(repo.GetTopRated(ALBUMS_IN_CATEGORY))
            });

            models.Add(new HomepageAlbumModel()
            {
                Name = "Random",
                Albums = Helpers.Convert(repo.GetRandom(ALBUMS_IN_CATEGORY))
            });

			return View(models);
		}
	}
}
