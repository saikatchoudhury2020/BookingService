using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebAPI.Models;
using WebAPI.Filters;
using System.IO;
using System.Text;
using Digimaker;

using System.Web;
using System.Web.Handlers;
using System.Web.Caching;
using System.Drawing;
using System.Data.SqlClient;
using System.Data;
using BookingService.WebAPI.Models;

namespace WebAPI.Controllers
{
    [IdentityBasicAuthentication]
    [Authorize]
    public class FoodController : ApiController
    {
        // GET: Food
        List<Article> articles = new List<Article>();
        List<MenuItem> menuItems = new List<MenuItem>();
        public IEnumerable<Article> GetAllFood(string menuItemId)
        {
            GetArticles(menuItemId);
            return articles;
        }
        private void GetArticles(string menuItemId)
        {
            int MaxData = 100;
            Digimaker.Schemas.Web.ArticleViewData articleDataSet = SiteBuilder.Content.Article.ByMenuIds(menuItemId, Digimaker.Data.Content.ArticleSortOrder.Default, int.MaxValue, false, SiteBuilder.Content.Article.ReturnValues.AbstractAndFullstory, MaxData);
            for (int i = 0; i < articleDataSet.Tables[0].Rows.Count; i++)
            {
                //string imgBase64 = ConvertImageURLToBase64(articleDataSet.Tables[0].Rows[i]["Filepath"].ToString());
                articles.Add(new Article { ArticleID = Convert.ToInt32(articleDataSet.Tables[0].Rows[i]["ArticleID"]), Headline = articleDataSet.Tables[0].Rows[i]["Headline"].ToString(), Abstract = articleDataSet.Tables[0].Rows[i]["Abstract"].ToString(), Fullstory = articleDataSet.Tables[0].Rows[i]["Fullstory"].ToString() });
            }
        }
        private String ConvertImageURLToBase641(String url)
        {
            FileInfo file = null;
            file = new FileInfo(Path.Combine(Digimaker.Config.ConfigDirectory.FullName.ToString() + "dm_pictures", url));

            //StringBuilder _sb = new StringBuilder();
            // Check if the file physically exist on the disk.
            if (file.Exists)
            {
                using (Image image = Image.FromFile(file.FullName))
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        image.Save(m, image.RawFormat);
                        byte[] imageBytes = m.ToArray();

                        // Convert byte[] to Base64 String
                        string base64String = Convert.ToBase64String(imageBytes);
                        return base64String;
                    }
                }
            }
            return null;
        }



        //http://localhost:8082/DigimakerWebApi/api/menuitems/getmenu
        public IEnumerable<MenuItem> GetMenu(int menuItemId)
        {
            Digimaker.Schemas.Web.MenuItemViewData menuItem = SiteBuilder.Content.MenuItem.Subtree(menuItemId.ToString(), menuItemId, new int[] { 0, 1, 2 }, new int[] { 0, 1, 4 }, 1, false, false, int.MaxValue);

            for (int i = 0; i < menuItem.MenuItem.Count; i++)
            {
                menuItems.Add(new MenuItem { MenuItemID = Convert.ToInt32(menuItem.MenuItem[i].MenuItemID), MenuItemName = menuItem.MenuItem[i].MenuItemName.ToString() });
            }
            return menuItems;
        }

    }
}