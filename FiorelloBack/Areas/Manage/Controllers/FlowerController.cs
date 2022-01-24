using FiorelloBack.DAL;
using FiorelloBack.Extensions;
using FiorelloBack.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FiorelloBack.Areas.Manage.Controllers
{   
    [Area("Manage")]
    public class FlowerController : Controller
    {
        
        
            private readonly AppDbContext _context;
            private readonly IWebHostEnvironment _env;

            public FlowerController(AppDbContext context, IWebHostEnvironment env)
            {
                _context = context;
                _env = env;
            }
            public IActionResult Index(int page=1)
            {
                ViewBag.TotalPage = Math.Ceiling((decimal)_context.Flowers.Count() / 2);
                ViewBag.CurrentPage = page;
                List<Flower> model = _context.Flowers.Include(f => f.FlowerImages).Skip((page - 1) * 2).Take(2).ToList();

            return View(model);
            }

        public IActionResult Create()
        {
            ViewBag.Campaigns = _context.Campaigns.ToList();
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Flower flower)
        {

            ViewBag.Campaigns = _context.Campaigns.ToList();
            ViewBag.Categories = _context.Categories.ToList();
            if (!ModelState.IsValid) return View();

            if (flower.CampaignId == 0)
            {
                flower.CampaignId = null;
            }
            flower.FlowerCategories = new List<FlowerCategory>();
            flower.FlowerImages = new List<FlowerImage>();
            foreach (int id in flower.CategoryIds)
            {
                FlowerCategory fCategory = new FlowerCategory
                {
                    Flower = flower,
                    CategoryId = id
                };
                flower.FlowerCategories.Add(fCategory);
            }
            if (flower.ImageFiles.Count > 4)
            {
                ModelState.AddModelError("ImageFiles", "You can choose only 4 images");
                return View();
            }
            if (flower.MainImage != null)
            {
                if (!flower.MainImage.IsImage())
                {
                    ModelState.AddModelError("ImageFiles", "Duzgun file secin");
                    return View();
                }
                if (!flower.MainImage.IsSizeOkay(2))
                {
                    ModelState.AddModelError("ImageFiles", "Image olcusu max 2MB olmalidir.");
                    return View();
                }
                FlowerImage flowerImage = new FlowerImage
                {
                    Image = flower.MainImage.SaveImg(_env.WebRootPath, "assets/images"),
                    IsMain = true,
                    Flower = flower
                };
                flower.FlowerImages.Add(flowerImage);
            }
          
            foreach (var image in flower.ImageFiles)
            {
                if (!image.IsImage())
                {
                    ModelState.AddModelError("ImageFiles", "Duzgun file secin");
                    return View();
                }
                if (!image.IsSizeOkay(2))
                {
                    ModelState.AddModelError("ImageFiles", "Image olcusu max 2MB olmalidir.");
                    return View();
                }

            }
            foreach (var image in flower.ImageFiles)
            {
                FlowerImage flowerImage = new FlowerImage
                {
                    Image = image.SaveImg(_env.WebRootPath, "assets/images"),
                    IsMain = false,
                    Flower = flower
                };
                flower.FlowerImages.Add(flowerImage);
            }
            _context.Flowers.Add(flower);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));

        }
        public IActionResult Edit(int id)
        {
            ViewBag.Campaigns = _context.Campaigns.ToList();
            ViewBag.Categories = _context.Categories.ToList();

            Flower flower = _context.Flowers.Include(f => f.FlowerCategories).Include(f=>f.FlowerImages).FirstOrDefault(f => f.Id == id);
            if (flower == null) return NotFound();
            return View(flower);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Flower flower)
        {

            ViewBag.Campaigns = _context.Campaigns.ToList();
            ViewBag.Categories = _context.Categories.ToList();
            Flower existedFlower = _context.Flowers.Include(f => f.FlowerImages).Include(f => f.FlowerCategories).FirstOrDefault(f => f.Id == flower.Id);


            if (!ModelState.IsValid) return View(existedFlower);

            if (existedFlower == null) return NotFound();
            //if (flower.MainImage == null)
            //{
            //    ModelState.AddModelError("MainImage", "Please select MainImage");
            //    return View(existedFlower);
            //}
           
            if (flower.ImageFiles != null)
            {
                foreach (var image in flower.ImageFiles)
                {
                    if (!image.IsImage())
                    {
                        ModelState.AddModelError("ImageFiles", "Please select the image file");
                        return View(existedFlower);
                    }
                    if (!image.IsSizeOkay(2))
                    {
                        ModelState.AddModelError("ImageFiles", "You can choose file which size is max 2MB");
                        return View(existedFlower);
                    }
                }

                List<FlowerImage> removableImages = existedFlower.FlowerImages.Where(fi =>!flower.ImageIds.Contains(fi.Id)).ToList();

                existedFlower.FlowerImages.RemoveAll(fi => removableImages.Any(ri => ri.Id == fi.Id));

                foreach (var item in removableImages)
                {
                    Helpers.Helper.DeleteImg(_env.WebRootPath, "assets/images", item.Image);
                }

                foreach (var image in flower.ImageFiles)
                {
                    FlowerImage flowerImage = new FlowerImage
                    {
                        Image = image.SaveImg(_env.WebRootPath, "assets/images"),
                        IsMain = false,
                        FlowerId = existedFlower.Id
                    };
                    existedFlower.FlowerImages.Add(flowerImage);
                }


                List<FlowerCategory> removableCategories = existedFlower.FlowerCategories.Where(fc => !flower.CategoryIds.Contains(fc.Id)).ToList();

                existedFlower.FlowerCategories.RemoveAll(fc => removableCategories.Any(rc => fc.Id == rc.Id));
                foreach (var categoryId in flower.CategoryIds)
                {
                    FlowerCategory flowerCategory = existedFlower.FlowerCategories.FirstOrDefault(fc => fc.CategoryId == categoryId);
                    if (flowerCategory == null)
                    {
                        FlowerCategory fCategory = new FlowerCategory
                        {
                            CategoryId = categoryId,
                            FlowerId = existedFlower.Id
                        };
                        existedFlower.FlowerCategories.Add(fCategory);
                    }
                }
            }
            else
            {
               

                List<FlowerImage> removableImages = existedFlower.FlowerImages.Where(fi => !flower.ImageIds.Contains(fi.Id)).ToList();

                existedFlower.FlowerImages.RemoveAll(fi => removableImages.Any(ri => ri.Id == fi.Id));

                foreach (var item in removableImages)
                {
                    Helpers.Helper.DeleteImg(_env.WebRootPath, "assets/images", item.Image);
                }

             


                List<FlowerCategory> removableCategories = existedFlower.FlowerCategories.Where(fc => !flower.CategoryIds.Contains(fc.Id)).ToList();

                existedFlower.FlowerCategories.RemoveAll(fc => removableCategories.Any(rc => fc.Id == rc.Id));
                foreach (var categoryId in flower.CategoryIds)
                {
                    FlowerCategory flowerCategory = existedFlower.FlowerCategories.FirstOrDefault(fc => fc.CategoryId == categoryId);
                    if (flowerCategory == null)
                    {
                        FlowerCategory fCategory = new FlowerCategory
                        {
                            CategoryId = categoryId,
                            FlowerId = existedFlower.Id
                        };
                        existedFlower.FlowerCategories.Add(fCategory);
                    }
                }
            }
            //List<FlowerCategory> removableCategories = existedFlower.FlowerCategories.Where(fc => !flower.CategoryIds.Contains(fc.Id)).ToList();

            //existedFlower.FlowerCategories.RemoveAll(fc => removableCategories.Any(rc => fc.Id == rc.Id));
            //foreach (var categoryId in flower.CategoryIds)
            //{
            //    FlowerCategory flowerCategory = existedFlower.FlowerCategories.FirstOrDefault(fc => fc.CategoryId == categoryId);
            //    if (flowerCategory == null)
            //    {
            //        FlowerCategory fCategory = new FlowerCategory
            //        {
            //            CategoryId = categoryId,
            //            FlowerId = existedFlower.Id
            //        };
            //        existedFlower.FlowerCategories.Add(fCategory);
            //    }
            //}

            existedFlower.Name = flower.Name;
            existedFlower.Price = flower.Price;
            existedFlower.Description = flower.Description;
            existedFlower.Weight = flower.Weight;
            existedFlower.Dimension = flower.Dimension;
            existedFlower.SkuCode = flower.SkuCode;
            existedFlower.InStock = flower.InStock;
            if (flower.CampaignId == 0)
            {
                flower.CampaignId = null;
            }
            existedFlower.Campaign = flower.Campaign;

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Delete(int id)
        {
            Flower flower = _context.Flowers.Include(f=>f.FlowerImages).FirstOrDefault(f => f.Id == id);
            Flower existedFlower= _context.Flowers.Include(f => f.FlowerImages).FirstOrDefault(f => f.Id == flower.Id);

            if (existedFlower == null) return NotFound();
            if (flower == null) return Json(new { status = 404 });

            //List<FlowerImage> removableImages = existedFlower.FlowerImages.Where(fi => !flower.ImageIds.Contains(fi.Id)).ToList();

            //existedFlower.FlowerImages.RemoveAll(fi => removableImages.Any(ri => ri.Id == fi.Id));

            foreach (var item in existedFlower.FlowerImages)
            {
                Helpers.Helper.DeleteImg(_env.WebRootPath, "assets/images", item.Image);
            }


            


            _context.Flowers.Remove(flower);
            _context.SaveChanges();

            return Json(new { status = 200 });

        }

    }
}
