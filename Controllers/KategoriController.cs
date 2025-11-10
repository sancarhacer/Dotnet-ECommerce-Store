using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using dotnet_store.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace dotnet_store.Controllers;

public class KategoriController : Controller
{
    private readonly DataContext _context;
    public KategoriController(DataContext context)
    {
        _context = context;
    }


    public ActionResult Index()
    {
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
        var entitiy = new Kategori
        {
            KategoriAdi = model.KategoriAdi,
            Url = model.Url
        };
        _context.Kategoriler.Add(entitiy);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }

}
