using System.Threading.Tasks;
using dotnet_store.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualBasic;



namespace dotnet_store.Controllers;
[Authorize(Roles = "Admin")]
public class UrunController : Controller
{
    private readonly DataContext _context;
    public UrunController(DataContext context)
    {
        _context = context;
    }

    
    public ActionResult Index(int? kategori)
    {
        var query = _context.Urunler.AsQueryable();
        if(kategori != null)
        {
            query = query.Where(i=> i.KategoriId == kategori);
        }
        var urunler = query.Select(i => new UrunGetModel
        {
            Id = i.Id,
            UrunAdi = i.UrunAdi,
            Fiyat = i.Fiyat,
            Aktif = i.Aktif,
            Anasayfa = i.Anasayfa,
            KategoriAdi = i.Kategori.KategoriAdi,
            Resim = i.Resim,
        }).ToList();
        ViewBag.Kategoriler = new SelectList(_context.Kategoriler.ToList(), "Id", "KategoriAdi",kategori);

        return View(urunler);
        
    }

    //http://localhost:5101/urunler/telefon?q=watch
    //route params: url => value
    //query string: q => value
    [AllowAnonymous]
    public ActionResult List(string url, string q)
    {
        


        // var query = _context.Urunler; 
        //  var query = _context.Urunler.AsQueryable; 
        // //Queryable Veritabanı sorgusu henüz çalışmadı .ToList()veya FirstOrDefault(), Count() kullanınca çalışacak
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

    [AllowAnonymous]
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
        // ViewBag.Kategoriler = _context.Kategoriler.ToList(); //yerine
        ViewBag.Kategoriler = new SelectList(_context.Kategoriler.ToList(), "Id", "KategoriAdi");
        // 2. SelectList nesnesi oluşturuyoruz:
            //    - Parametre 1: Veri kaynağı (liste)
            //    - Parametre 2: "DataValueField" -> Veritabanına gönderilecek değer (Id)
            //    - Parametre 3: "DataTextField" -> Kullanıcının ekranda göreceği metin (KategoriAdi)
        return View();
    }

    
    [HttpPost]
    public async Task<ActionResult> Create(UrunCreateModel model)
    {
        if (model.Resim == null || model.Resim.Length == 0)
        {
            ModelState.AddModelError("Resim", "Resim yükleyiniz");
        }
        // model validation
        //https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-9.0
        if (ModelState.IsValid)
        {
            // 1. Dosya için rastgele ve benzersiz bir isim oluşturur (Örn: "a2b3c4d5.jpg")
            // Path.GetRandomFileName() güvenli ve çakışma ihtimali düşük bir isim üretir.
            var fileName = Path.GetRandomFileName() + ".jpg";

            // 2. Dosyanın kaydedileceği tam yolu (full path) belirler.
            // Directory.GetCurrentDirectory(): Projenin ana klasörünü bulur.
            // Path.Combine: Farklı işletim sistemlerine (Windows/Linux) uyumlu şekilde yolları birleştirir.
            // Sonuç şuna benzer: "C:/Projem/wwwroot/img/rastgeleisim.jpg"
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);

            // 3. Belirlenen yolda boş bir dosya oluşturur ve bu dosyaya veri yazmak için bir 'akış' (stream) açar.
            // 'using' bloğu, işlem bitince dosyanın bilgisayar hafızasında kilitli kalmamasını sağlar.
            using (var stream = new FileStream(path, FileMode.Create))
            {
                // 4. Formdan gelen resmin içeriğini (baytlarını), oluşturduğumuz boş dosyaya kopyalar.
                // 'await' ve 'CopyToAsync' sayesinde büyük dosyalarda sistem donmadan işlem yapılır.
                await model.Resim!.CopyToAsync(stream);
            }
            var entitiy = new Urun()
            {
                UrunAdi = model.UrunAdi,
                Aciklama = model.Aciklama,
                Fiyat = model.Fiyat ?? 0,
                Aktif = model.Aktif,
                Anasayfa = model.Anasayfa,
                Resim = fileName,
                KategoriId =(int)model.KategoriId!,
            };

            _context.Urunler.Add(entitiy);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        ViewBag.Kategoriler = new SelectList(_context.Kategoriler.ToList(), "Id", "KategoriAdi");
        return View(model);
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
        if (ModelState.IsValid)
        {

            var entity = _context.Urunler.FirstOrDefault(i => i.Id == id);
            if (entity != null)
            {
                if (model.Resim != null)
                {
                    var fileName = Path.GetRandomFileName() + ".jpg";
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await model.Resim!.CopyToAsync(stream);

                    }
                    entity.Resim = fileName;

                }

                entity.Id = model.Id;
                entity.UrunAdi = model.UrunAdi;
                entity.Aciklama = model.Aciklama;
                entity.Fiyat = model.Fiyat ?? 0;
                entity.Aktif = model.Aktif;
                entity.Anasayfa = model.Anasayfa;
                entity.KategoriId = (int)model.KategoriId!;

                _context.SaveChanges();

                TempData["Mesaj"] = $"{entity.UrunAdi} urunu  güncellendi";

                return RedirectToAction("Index", "Urun");
            }
        }
         ViewBag.Kategoriler = new SelectList(_context.Kategoriler.ToList(), "Id", "KategoriAdi");
        return View(model);
    }
    
    
    public ActionResult Delete(int? id)
    {
        if (id == null)
        {
            return RedirectToAction("Index");
        }
        var entity = _context.Urunler.FirstOrDefault(i => i.Id == id);
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
        var entitiy = _context.Urunler.FirstOrDefault(i => i.Id == id);
        if (entitiy != null)
        {
            _context.Urunler.Remove(entitiy);
            _context.SaveChanges();
            TempData["Mesaj"] = $"{entitiy.UrunAdi} kategorisi güncellendi";
           
        }
        return RedirectToAction("Index");
    }

}