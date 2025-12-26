using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using dotnet_store.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace dotnet_store.Controllers;


[Authorize(Roles = "Admin")]
public class KategoriController : Controller
{
    private readonly DataContext _context;
    public KategoriController(DataContext context)
    {
        _context = context;
    }


    
    public ActionResult Index()
    {
        //var kategoriler = _context.Kategoriler.Include(i => Uruns).ToList();
        //yukarıda sadece urun sayısı ihtiyacımız olmasına rağmen tüm ürünleri çekiyoruz perfonmaslı değil
        // Select (Projection): Veritabanından sadece ihtiyacımız olan alanları çekiyoruz.
        // Bu, veritabanı trafiğini azaltır ve performansı artırır.
        
        var kategoriler = _context.Kategoriler.Select(i => new KategoriGetModel
        {
            Id = i.Id,
            KategoriAdi = i.KategoriAdi,
            Url = i.Url,
            UrunSayisi = i.Uruns.Count
        }).ToList();

        return View(kategoriler);
    }

    [HttpGet]
    public ActionResult Create()
    {

        // ViewBag.Kategoriler = _context.Kategoriler.ToList();
        //SelectList, ASP.NET Core MVC'de Html.DropDownListFor veya asp-items Tag Helper'ı gibi yardımcılar tarafından 
        // beklenen özel bir veri formatıdır. Bu sınıf, ham veri listesini alır ve her bir öğenin hangi alanının 
        // görünen metin (Displayed Text) ve hangi alanının değer (Value) olarak kullanılacağını belirtmenize olanak tanır.
        // Id'leri value olarak, KategoriAdi'nı ise görünen metin olarak kullanır
        ViewBag.Kategoriler = new SelectList(_context.Kategoriler.ToList(), "Id", "KategoriAdi");
        return View();
    }

    [HttpPost]
    public ActionResult Create(KategoriCreateModel model)
    {

        if (ModelState.IsValid)
        {
            var entitiy = new Kategori
            {
                KategoriAdi = model.KategoriAdi,
                Url = model.Url
            };
            _context.Kategoriler.Add(entitiy);
            _context.SaveChanges();
            return RedirectToAction("Index");

        }
        return View(model);

    }

    public ActionResult Edit(int id)
    {
        var entitiy = _context.Kategoriler.Select(i => new KategoriEditModel
        {
            Id = i.Id,
            KategoriAdi = i.KategoriAdi,
            Url = i.Url,

        }).FirstOrDefault(i => i.Id == id);
        return View(entitiy);
    }

    [HttpPost]
    public ActionResult Edit(int id, KategoriEditModel model)
    {
        if (id != model.Id)
        {
            NotFound();
        }
        if (ModelState.IsValid)
        {
            var entitiy = _context.Kategoriler.FirstOrDefault(i => i.Id == id);
            if (entitiy != null)
            {
                entitiy.KategoriAdi = model.KategoriAdi;
                entitiy.Url = model.Url;
                _context.SaveChanges();
                
                //farklı action methoda gidince tempdata ya ulaşılabilir
                TempData["Mesaj"] = $"{entitiy.KategoriAdi} kategorisi güncellendi";

                return RedirectToAction("Index");
            }


        }
        return View(model);
    }

    public ActionResult Delete(int? id)
    {
        if (id == null)
        {
            return RedirectToAction("Index");
        }
        var entity = _context.Kategoriler.FirstOrDefault(i => i.Id == id);
        if (entity != null)
        {
            return View(entity);
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public ActionResult DeleteConfirm(int? id)
    {
        if (id == null)
        {
            return RedirectToAction("Index");
        }
        var entitiy = _context.Kategoriler.FirstOrDefault(i => i.Id == id);
        if (entitiy != null)
        {
            _context.Kategoriler.Remove(entitiy);
            _context.SaveChanges();
            TempData["Mesaj"] = $"{entitiy.KategoriAdi} kategorisi güncellendi";
           
        }
        return RedirectToAction("Index");
    }


}
