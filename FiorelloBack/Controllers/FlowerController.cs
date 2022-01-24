using FiorelloBack.DAL;
using FiorelloBack.Models;
using FiorelloBack.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FiorelloBack.Controllers
{
    public class FlowerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        public FlowerController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Details(int id,int categoryId)
        {
            Flower flower = _context.Flowers.Include(f => f.Campaign).Include(f => f.FlowerCategories).ThenInclude(fc => fc.Category).Include(f => f.FlowerTags).ThenInclude(ft => ft.Tag).Include(f => f.FlowerImages).Include(f => f.FlowerImages).Include(f => f.Comments).ThenInclude(c => c.AppUser).FirstOrDefault(f => f.Id == id);
            if (flower == null) return NotFound();
           
            //var kategori=_context.Categories.Where(c=>c.Id==id).Select(c=>c.Name).FirstOrDefault();
            //ViewBag.viewCategory = kategori;
            ViewBag.RelatedFlowers = _context.Flowers.Include(f => f.FlowerImages).Include(f => f.Campaign).Include(f => f.FlowerCategories).Where(f => f.FlowerCategories.FirstOrDefault().CategoryId == categoryId && f.Id != id).OrderByDescending(f => f.Id).Take(4).ToList();
            ViewBag.FlowerRelatedCategory = _context.Categories.ToList();
            return View(flower);
        }
        [Authorize]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> AddComment(Comment comment)
        {
            AppUser user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (!ModelState.IsValid) return RedirectToAction("Details", "Flower", new { id = comment.FlowerId });
            if (!_context.Flowers.Any(f => f.Id == comment.FlowerId)) return NotFound();
            Comment cmnt = new Comment
            {
                Text = comment.Text,
                FlowerId = comment.FlowerId,
                CreatedTime = DateTime.Now,
                AppUserId = user.Id
            };
            _context.Comments.Add(cmnt);
            _context.SaveChanges();
            return RedirectToAction("Details", "Flower", new { id = comment.FlowerId });
        }
        [Authorize]
        //public async Task<IActionResult> DeleteComment(int id)
        //{
        //    AppUser user = await _userManager.FindByNameAsync(User.Identity.Name);
        //    if (!ModelState.IsValid) return RedirectToAction("Index", "Flower");
        //    if (!_context.Comments.Any(c => c.Id == id && c.IsAccess == true && c.AppUserId == user.Id)) return NotFound();
        //    Comment comment = _context.Comments.FirstOrDefault(c => c.Id == id && c.AppUserId == user.Id);
        //    _context.Comments.Remove(comment);
        //    _context.SaveChanges();
        //    return RedirectToAction("Details", "Flower", new { id = comment.FlowerId });
        //}

        public IActionResult AddBasket(int id)
        {
            Flower flower = _context.Flowers.FirstOrDefault(f => f.Id == id);

            string basket = HttpContext.Request.Cookies["Basket"];

           
            if (basket == null)
            {
                BasketVM basketVM = new BasketVM
                {
                    BasketItems = new List<BasketItemVM>(),
                    TotalPrice = flower.Price,
                    Count = 1

                };

                BasketItemVM basketItemVM = new BasketItemVM
                {
                    Flower = flower,
                    Count = 1
                };

                basketVM.BasketItems.Add(basketItemVM);
                string basketStr = JsonConvert.SerializeObject(basketVM);

                HttpContext.Response.Cookies.Append("Basket", basketStr);
                
            }
            else
            {
                BasketVM basketVM = JsonConvert.DeserializeObject<BasketVM>(basket);
                BasketItemVM basketItemVM = basketVM.BasketItems.FirstOrDefault(f => f.Flower.Id == flower.Id);
                if (basketItemVM == null)
                {
                    basketItemVM = new BasketItemVM
                    {
                        Flower = flower,
                        Count = 1
                    };
                    basketVM.BasketItems.Add(basketItemVM);
                    basketVM.Count++;
                }
                else
                {
                    basketItemVM.Count++;
                }

                basketVM.TotalPrice += flower.Price;
                Math.Round(basketVM.TotalPrice, 2);
                string basketStr = JsonConvert.SerializeObject(basketVM);

                HttpContext.Response.Cookies.Append("Basket", basketStr);
                
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult ShowBasket()
        {
            string basketStr = HttpContext.Request.Cookies["Basket"];
            BasketVM basket = JsonConvert.DeserializeObject<BasketVM>(basketStr);
            return Json(basket);
        }

        public IActionResult SearchResult(string Name)
        {
            List<Flower> flowers = _context.Flowers.Where(f => f.Name.ToLower().Contains(Name.ToLower())).Include(f => f.FlowerImages).Include(f=>f.FlowerCategories).ThenInclude(fc => fc.Category).Include(f => f.FlowerTags).ThenInclude(ft => ft.Tag).Include(f=>f.Campaign).ToList();
            if (!flowers.Any(f => f.Name.ToLower().Contains(Name.ToLower())))
            {
                ModelState.AddModelError("Name", "No Result");
            }


            return View(flowers);
        }
    }
}
