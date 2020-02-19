using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebAPI.Models;
using WebAPI.Filters;
using BookingService.WebAPI.Models;
using BookingService.WebAPI.DTO;
using BookingService.WebAPI.Enums;
using Newtonsoft.Json;

namespace WebAPI.Controllers
{
    [IdentityBasicAuthentication]
    [Authorize]
    public class MenuItemsController : ApiController
    {

        //http://localhost:8082/digimakerwebapi/api/menuitems/getmenu

        public IEnumerable<Menu> GetMenu(int menuItemId, string extendedPropertiesId = "")
        {
            List<Menu> menus = new List<Menu>();
            Digimaker.Schemas.Web.MenuItemViewData menuItem = SiteBuilder.Content.MenuItem.Subtree(menuItemId.ToString(), menuItemId, new int[] { 0, 1, 2 }, new int[] { 0, 1, 4 }, 1, false, false, int.MaxValue);

            for (int i = 0; i < menuItem.MenuItem.Count; i++)
            {
                var menuitemId = menuItem.MenuItem[i].MenuItemID;
                var list = new Dictionary<string, string>();
                if ( extendedPropertiesId != "" )
                {
                    string[] formIds = extendedPropertiesId.Split( ';' );
                    foreach( var formId in formIds )
                    {
                        var extendedList = DMBase.Core.ContentExtension.GetValues( Convert.ToInt32( formId ), menuitemId);
                        foreach( var item in extendedList )
                        {
                            list.Add( item.Key, item.Value );
                        }
                    }
                }
                menus.Add(new Menu { MenuId = menuitemId,
                    MenuName = menuItem.MenuItem[i].MenuItemName.ToString(),
                    ExtendedProperteis = list
                } );
                
            }
            return menus;
        }

        [HttpGet]
        [ActionName("GetMenuDetail")]
        public List<Menu> GetMenuDetail(string menuItemID, int includeArticle = 0 )
        {
            var idList = menuItemID.Split('-');
            List<Menu> menuList = new List<Menu>();

            foreach( var id in idList )
            {
                int menuId = 0;
                if (menuItemID != null)
                {
                    menuId = Convert.ToInt32(id);
                }

                Menu menu;
                using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
                {
                    menu = new Menu();
                    var result = entity.MenuItems.Where(w => w.MenuItemID == menuId).Select(s => s).FirstOrDefault();
                    menu.MenuId = result.MenuItemID;
                    menu.MenuName = result.MenuItemName;
                    menu.MenuDesc = result.MetaDescription;
                    menu.PicturePath = entity.PictureProperties.Where(w => w.PictureMainID == result.Pictureid).Select(s => s.Filepath).ToString();
                }

                if(includeArticle ==1)
                {
                    var rows = SiteBuilder.Content.Article.ByMenuIds(id, Digimaker.Data.Content.ArticleSortOrder.Default, 0, true, SiteBuilder.Content.Article.ReturnValues.AbstractAndFullstory, 1).Article.Rows;
                    if (rows.Count > 0)
                    {
                        var article = (Digimaker.Schemas.Web.ArticleViewData.ArticleRow)rows[0];
                        var articleModel = new ArticleModel();
                        articleModel.ArticleDesc = article.Abstract.Replace("{RelatedPersons}", "");
                        articleModel.ArticleName = article.Headline;
                        articleModel.PicturePath = article.Filepath;
                        articleModel.ArticleId = article.MainID;
                        articleModel.ArticleBody = article.Fullstory;

                        var children = new List<ArticleModel>();
                        children.Add(articleModel);
                        menu.Children = children;
                    }

                }
                menuList.Add( menu );
            }
            return menuList;
        }

        [HttpGet]
        [ActionName("getChildMenuList")]
        [IdentityBasicAuthentication]
        [Authorize]
        public IHttpActionResult GetChildMenuList(int parentMenuId)
        {

            ApiResponse response = new ApiResponse();
            if (parentMenuId != 0)
            {
                using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
                {
                    var result = entity.MenuItems.Where(w => w.Parentid == parentMenuId).Select(s => s).ToList();
                    if (result.Count > 0)
                    {
                        List<MenuItems> menulist = new List<MenuItems>();
                        MenuItems menu;
                        foreach (var item in result)
                        {
                            menu = new MenuItems();
                            menu.MenuItemID = item.MenuItemID;
                            menu.MenuName = item.MenuItemName;
                            menulist.Add(menu);
                        }
                    
                        return Ok(menulist);
                    }
                    else
                    {
                        response.message = "No data found";
                        response.errorType = (int)ErrorType.Error;
                        return Ok(response);
                    }
                }
            }
            else
            {
                response.message = "please enter valid menuid";
                response.errorType = (int)ErrorType.Error;
                return Ok(response);
            }
           

        }

    }
}