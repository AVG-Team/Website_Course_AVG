using Imgur.API.Endpoints;
using Imgur.API.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using Imgur.API.Authentication;
using System.Threading.Tasks;
using Website_Course_AVG.Models;
using Website_Course_AVG.Managers;
using Website_Course_AVG.Areas.Admin.Data.ViewModels;
using Website_Course_AVG.Attributes;

namespace Website_Course_AVG.Areas.Admin.Controllers
{
    public class ImageController : Controller
    {
        MyDataDataContext _data = new MyDataDataContext();
        // GET: Admin/Image


        [Admin]
        public ActionResult Index()
        {
            return View();
        }

        [Admin]
        public ActionResult Insert()
        {
            return View();
        }


        [Admin]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Insert(IEnumerable<HttpPostedFileBase> files, string CategorySelection, bool? Category)
        {
            try
            {
                var apiClient = new ApiClient("6efaec52e38d148", "5de13ec766f236d3d39808cb21fec395962922cf");
                var httpClient = new HttpClient();

                var oAuth2Endpoint = new OAuth2Endpoint(apiClient, httpClient);
                var authUrl = oAuth2Endpoint.GetAuthorizationUrl();

                var token = new OAuth2Token
                {
                    AccessToken = "4e5b5d07a81334ea5c5459dc5c1ef63458c296eb",
                    RefreshToken = "6e653d2f3b7c99cb1c135f690c5b297b8a1555b1",
                    AccountId = 180393165,
                    AccountUsername = "lvxadoniss1",
                    ExpiresIn = 315360000,
                    TokenType = "bearer"
                };

                apiClient.SetOAuth2Token(token);
                var imageEndpoint = new ImageEndpoint(apiClient, httpClient);
                var code = "AVG_IMG_" + Helpers.GenerateRandomString(8);

                foreach (var file in files)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        var fileStream = file.InputStream;
                        var imageUpload = await imageEndpoint.UploadImageAsync(fileStream);
                        var imageUrl = imageUpload.Link;
                        bool? categoryValue = null;
                        if (bool.TryParse(CategorySelection, out bool result))
                        {
                            categoryValue = result;
                        }

                        var newImage = new image
                        {
                            image1 = imageUrl,
                            type = true,
                            category = categoryValue,
                            code = code,
                        };
                        _data.images.InsertOnSubmit(newImage);
                        _data.SubmitChanges();
                    }
                }

                ViewBag.Message = "Image uploaded successfully!";
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, "Error");
                var categories = _data.categories.Where(cate => cate.deleted_at == null).ToList();
                var adminView = new AdminViewModels()
                {
                    Categories = categories
                };
                return View(adminView);
            }
        }
    }
}