using dotnet_store.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;



namespace dotnet_store.Controllers;

public class UrunController : Controller
{
    private readonly DataContext _context;
    public UrunController(DataContext context)
    {
        _context = context;
    }

    public ActionResult Index()
    {
        var urunler = _context.Urunler.Select(i => new UrunGetModel
        {
            Id = i.Id,
            UrunAdi = i.UrunAdi,
            Fiyat = i.Fiyat,
            Aktif = i.Aktif,
            Anasayfa = i.Anasayfa,
            KategoriAdi = i.Kategori.KategoriAdi,
            Resim = i.Resim,
        }).ToList();

        return View(urunler);
    }

    //http://localhost:5101/urunler/telefon?q=watch
    //route params: url => value
    //query string: q => value
    public ActionResult List(string url, string q)
    {
        // var urunler = _context.Urunler.Where(i => i.Aktif && i.Kategori.Url == url).ToList();


        // var query = _context.Urunler; 
        //  var query = _context.Urunler.AsQueryable; //Queryable Veritabanı sorgusu henüz çalışmadı .ToList()veya FirstOrDefault(), Count() kullanınca çalışacak
        var query = _context.Urunler.Where(i => i.Aktif);
        if (!string.IsNullOrEmpty(url))
        {
            query = query.Where(i => i.Kategori.Url == url);
        }
        if (!string.IsNullOrEmpty(q))
        {
            query = query.Where(i => i.UrunAdi.ToLower().Contains(q.ToLower()));
            ViewData["q"] = q;
        }

        // Sorguyu sonlandır ve çalıştır:
        // .ToList() metodu çağrıldığında, yukarıdaki tüm Where filtreleri birleştirilir,
        // tek bir SQL sorgusu oluşturulur ve veritabanına gönderilir.
        return View(query.ToList());
    }

    public ActionResult Details(int id)
    {
        // var urun = _context.Urunler.FirstOrDefault(urun => urun.Id == id); //bulunan ilk kaydı alır
        var urun = _context.Urunler.Find(id); //sadece id ye göre yapılıyor bu

        if (urun == null)
        {
            return RedirectToAction("Index", "Home");
        }
        ViewData["BenzerUrunler"] = _context.Urunler
                                        .Where(i => i.Aktif && i.Kategori.Id == urun.Id && i.Id != id)
                                        .Take(4) //en fazla d tane alır baştan 4 tane
                                        .ToList();
        return View(urun);
    }

    public ActionResult Create()
    {
        // ViewBag.Kategoriler = _context.Kategoriler.ToList();
        ViewBag.Kategoriler = new SelectList(_context.Kategoriler.ToList(), "Id", "KategoriAdi");
        return View();
    }

    [HttpPost]
    public ActionResult Create(UrunCreateModel model)
    {
        var entitiy = new Urun()
        {
            UrunAdi = model.UrunAdi,
            Aciklama = model.Aciklama,
            Fiyat = model.Fiyat,
            Aktif = true,
            Anasayfa = true,
            Resim = "1.jpeg",
            KategoriId = 1,
        };

        _context.Urunler.Add(entitiy);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }
}