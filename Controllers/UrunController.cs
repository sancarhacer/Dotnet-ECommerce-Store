using System.Threading.Tasks;
using dotnet_store.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualBasic;



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
    public async Task<ActionResult> Create(UrunCreateModel model)
    {
        var fileName = Path.GetRandomFileName() + ".jpg";
        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);
        using (var stream = new FileStream(path, FileMode.Create))
        {
            await model.Resim!.CopyToAsync(stream);

        }
        
        var entitiy = new Urun()
        {
            UrunAdi = model.UrunAdi,
            Aciklama = model.Aciklama,
            Fiyat = model.Fiyat,
            Aktif = model.Aktif,
            Anasayfa = model.Anasayfa,
            Resim = fileName,
            KategoriId = model.KategoriId,
        };

        _context.Urunler.Add(entitiy);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }


    public ActionResult Edit(int id)
    {
        var entitiy = _context.Urunler.Select(i => new UrunEditModel
        {
            Id = i.Id,
            UrunAdi = i.UrunAdi,
            Aciklama = i.Aciklama,
            Fiyat = i.Fiyat,
            Aktif = i.Aktif,
            Anasayfa = i.Anasayfa,
            ResimAdi = i.Resim,
            KategoriId = i.KategoriId



        }).FirstOrDefault(i => i.Id == id);

        ViewBag.Kategoriler = new SelectList(_context.Kategoriler.ToList(), "Id", "KategoriAdi");
        return View(entitiy);
    }

    [HttpPost]
    public async Task<ActionResult> Edit(int id, UrunEditModel model)
    {
        if (id != model.Id)
        {
            NotFound();
        }

        var entity = _context.Urunler.FirstOrDefault(i => i.Id == id);
        if (entity != null)
        {
            if(model.ResimDosyasi != null)
            {
                var fileName = Path.GetRandomFileName() + ".jpg";
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await model.ResimDosyasi!.CopyToAsync(stream);

                }
                entity.Resim = fileName;
                
            }

            entity.Id = model.Id;
            entity.UrunAdi = model.UrunAdi;
            entity.Aciklama = model.Aciklama;
            entity.Fiyat = model.Fiyat;
            entity.Aktif = model.Aktif;
            entity.Anasayfa = model.Anasayfa;
            entity.KategoriId = model.KategoriId;

            _context.SaveChanges();

            TempData["Mesaj"] = $"{entity.UrunAdi} urunu  güncellendi";

            return RedirectToAction("Index","Urun");
        }

        return View(model);
    }

}