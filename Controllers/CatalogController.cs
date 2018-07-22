using System.Linq;
using LibraryExercise.Models.Catalog;
using Microsoft.AspNetCore.Mvc;

namespace LibraryExercise.Controllers
{
    public class CatalogController : Controller
    {
        private readonly ILibraryAssetService _assets;
        public CatalogController(ILibraryAssetService assets)
        {
            this._assets = assets;
        }

        public IActionResult Index()
        {
            var assetModel = _assets.GetAll();

            var listingResult = assetModel.Select(result => new AssetIndexListingModel 
            {
                Id = result.Id,
                ImageUrl = result.ImageUrl,
                AuthorOrDirector = _assets.GetAuthorOrDirector(result.Id),
                DeweyCallNumber = _assets.GetDeweyIndex(result.Id),
                Title = result.Title,
                Type = _assets.GetType(result.Id)
            });

            var model = new AssetIndexModel()
            {
                Assets = listingResult
            };

            return View(model);
        }
    }
}