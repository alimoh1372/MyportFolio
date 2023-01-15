using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyportFolio.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MyportFolio.Controllers
{
    public class HomeController : Controller
    {
        private readonly List<Portfolio> _portfolios= new List<Portfolio>()
        {
            new Portfolio{Id = 1,Image ="01.jpg" ,Title = "Accounting",Description = "یک برنامه برای انجام امور حسابداری شخصی تحت ویندوز"},

            new Portfolio{Id = 2,Image ="02.jpg" ,Title = "مدیریت مدرسه",Description = "کمک به مدیریت مدرسه تمام داینامیک جهت اضافه کردن دانش اموزان معلمین و سایر آبجکت های مدرسه نحت ویندوز"},

            new Portfolio { Id = 3, Image = "03.jpg", Title = " سایت فروشگاه", Description = "سایت فروشگاه به تحت MVC و مدیریت محصولات دسته بندی محصولات و ..." },


            new Portfolio { Id = 4, Image = "04.jpg", Title = "مدیریت ساختمان", Description = "برنامه ساده و کاربردی به جهت مدیریت امور ساختمان و هزینه های آن" },

        };
public IActionResult Index()
        {
            return View(_portfolios);
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetContact()
        {
            ContactInfo contact = new ContactInfo()
            {
                Email = "Alimohammadzade7976@gmail.com",
                Number = "09383738757",
                Github = "Github.com/alimoh1372"
            };
           var result= Json(data: contact);
            return result;
        }
        [HttpGet]
        public JsonResult GetPortfolios()
        {
            return Json(data: _portfolios);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
