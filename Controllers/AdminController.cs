using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_store.Controllers;


// . Authentication:
// Kullanıcının sisteme giriş yaparken sunduğu kanıtların (Email, Parola) doğrulanmasıdır.
// Cookie Based (Çerez Tabanlı): Tarayıcıya bir "giriş bileti" (çerez) bırakılır. Genellikle standart web sitelerinde kullanılır.
// Token Based (JWT): Tarayıcı dışında (Mobil uygulamalar, farklı platformlar) çalışırken kullanılan dijital anahtardır.
// External Provider (Dış Sağlayıcılar): "Google ile Giriş Yap" gibi, kimlik doğrulama yükünü güvenilir devlere devretme yöntemidir.

// Cookie Based Authentication Akışı
// Talep (Request): Kullanıcı yetki gerektiren bir sayfaya (örneğin /Admin) girmek ister.
// Engel: Sistem bakar, kullanıcı giriş yapmamışsa onu hemen Login sayfasına yönlendirir.
// Giriş: Kullanıcı bilgilerini girer. Bilgiler doğruysa sunucu kullanıcıya özel bir Cookie (Çerez) oluşturur ve tarayıcıya gönderir.
// Tanıma: Kullanıcı bir sonraki sayfaya gittiğinde, tarayıcı bu çerezi otomatik olarak sunucuya sunar. Sunucu "Tamam, seni tanıyorum, biletin geçerli" der ve kapıyı a
//ConfigureApplicationCookie program.cs de yapılır
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    public ActionResult Index()
    {
        return View();
    }
}