using System.Security.Claims;
using System.Threading.Tasks;
using dotnet_store.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_store.Controllers;

// ASP.NET Identity, Kullanıcıyı güvenli şekilde yönetmek” için yazılmış hazır bir altyapıdır.
// Şunları senin yerine yapar: Kullanıcıyı veritabanında tutar, Şifreleri hash’ler, Login / Logout yönetir, Roller ve yetkiler
// Token üretimi (şifre sıfırlama, email doğrulama), Lockout (hesap kilitleme), Claims.
    // IdentityUser:	Veritabanındaki "Kullanıcı" tablosunun C# karşılığıdır.
    // UserManager:	kullanıcıyla ilgili HER ŞEYDEN sorumlu sınıftır. Kullanıcı oluşturur, Kullanıcıyı bulur,Şifreyi hash’ler,Token üretir, Roller atar, Lockout yönetir
    // SignInManager, kullanıcının sisteme girip çıkmasını yönetir.Login, Logout, Cookie oluşturma, Remember Me, Lockout kontrolü; “Bu kullanıcı şu an sistemde mi?
    // RoleManager, rollerin kendisini yönetir. Adminvb. rolü oluştur, Rol var mı kontrol et, Rol sil
    // Claim = Kullanıcı hakkında doğrulanmış bilgi Örnek: UserId Email Role = Admin
    // CreateAsync:	Kullanıcıyı şifreleyerek DB'ye kaydeden asenkron metod.
    // IdentityResult:	Kayıt işleminin başarılı olup olmadığını ve hataları tutan nesne.
public class AccountController : Controller
{
    // UserManager<IdentityUser> : Bu, Identity kütüphanesinin en önemli parçasıdır. 
    // Kullanıcılarla ilgili tüm veritabanı işlemlerini (Kullanıcı ekleme, silme, şifre değiştirme, rol kontrolü vb.) 
    // yapan hazır bir yardımcı sınıftır.
    
    // private UserManager<IdentityUser> _userManager;


    private UserManager<AppUser> _userManager;
    private SignInManager<AppUser> _signInManager;
    private IEmailService _emailService;
    public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailService emailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
    }
    public ActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<ActionResult> Create(AccountCreateModel model)
    {
        if (ModelState.IsValid)
        {
            // Burada henüz veritabanına bir şey yazılmadı. Sadece Identity'nin anlayacağı cinsten bir kullanıcı taslağı 
            // oluşturdun. Dikkat edersen şifreyi buraya yazmadın; çünkü şifre güvenlik nedeniyle farklı işlenir.
            //username olması şarttır
            // var user = new IdentityUser { UserName = model.Username, Email = model.Email };
             var user = new AppUser { UserName = model.Email, Email = model.Email, AdSoyad = model.AdSoyad };
    
            // Şifreyi otomatik olarak hash'ler
            // Identity kurallarına bakarak kullanıcıyı veritabanına kaydeder
                //.NET (Identity) başlangıçta bazı "varsayılan" kurallarla gelir, ama yazılımcı olarak sen bu kuralları istediğin gibi esnetebilir veya sıkılaştırabilirsin. program.cs de
            // Kullanıcı adının benzersiz olup olmadığını kontrol eder
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                // Eğer şifre çok kısaysa veya bu kullanıcı adı zaten varsa, Identity sana result.Errors içinde hata listesi döner.
                //  Sen de bu hataları ModelState'e ekleyerek sayfada gösterirsin
                ModelState.AddModelError("", error.Description);
            }
        }
        return View(model);
    }

    public ActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<ActionResult> Login(AccountLoginModel model,string? returnUrl)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                // Cookie silinir,Kullanıcı artık authenticated değil
                await _signInManager.SignOutAsync();

                // Şifre doğru mu?,Hesap kilitli mi? Hatalı giriş sayısı arttırılsın mı? Cookie oluşturulsun mu?
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.BeniHatirla, true);

                
                    // Login başarılı
                if (result.Succeeded)
                {
                    // Yanlış giriş sayısını sıfırlar, Kilidi kaldırır
                    await _userManager.ResetAccessFailedCountAsync(user);
                    await _userManager.SetLockoutEndDateAsync(user,null);

                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    

                    
                }
                // result.IsLockedOut    // Hesap kilitli
                // Lockout ayarları Program.cs’tedir.
                else if(result.IsLockedOut) {
                    var lockoutDate = await _userManager.GetLockoutEndDateAsync(user);
                    var  timeleft = lockoutDate.Value - DateTime.UtcNow;
                    ModelState.AddModelError("",$"Hesabınız kitlendi. Lütfen {timeleft.Minutes +1} dakika bekleyiniz");
                }
                else
                {
                    ModelState.AddModelError("", "Hatalı parola");
                }
            }
            else
            {
                ModelState.AddModelError("", "Hatalı email");
            }
        }
        return View(model);
    }


    // [Authorize] Kullanıcının login olup olmadığını kontrol eder,Cookie içindeki Claims’e bakar, Yetkisizse → AccessDenied
    //Login sırasında cookie içine şunlar yazılır:UserId,Email,Username, Roller. Bunlara Claim denir
     [Authorize]
    public async Task<ActionResult> LogOut()
    {
        await _signInManager.SignOutAsync();

        return RedirectToAction("Login", "Account");
    }

    [Authorize]
    public async Task<ActionResult> EditUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId!);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        return View(new AccountEditUserModel
        {
            AdSoyad = user.AdSoyad,
            Email = user.Email!
        });
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> EditUser(AccountEditUserModel model)
    {
        if (ModelState.IsValid)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user != null)
            {
                user.Email = model.Email;
                user.AdSoyad = model.AdSoyad;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    TempData["Mesaj"] = "Bilgileriniz güncellendi";
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
        }
        return View(model);
    }

    public ActionResult AccessDenied()
    {
        return View();
    }


    [Authorize]
    public ActionResult ChangePassword()
    {
        return View();
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult> ChangePassword(AccountChangePasswordModel model)
    {
        if (ModelState.IsValid)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user != null)
            {
                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.Password);

                if (result.Succeeded)
                {
                    TempData["Mesaj"] = "Parolanız güncellendi";
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
        }
        return View(model);
    }

    
    public ActionResult ForgotPassword()
    {
        return View();
    }
    // Token:
    // “Bu işlemi yapmaya gerçekten yetkilisin” demek için üretilen tek kullanımlık, süreli ve kriptografik olarak güvenli bir anahtartır.
    // Şifre sıfırlama senaryosunda token şunu temsil eder: “Bu kullanıcı, mail adresine gerçekten erişebildiğini kanıtladı.
    // Token sayesinde;  Link tek seferlik, Süreli, Kullanıcıya özel, Tahmin edilemez
    [HttpPost]
    public async Task<ActionResult> ForgotPassword(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            TempData["Mesaj"] = "Eposta adresinizi giriniz";
            return View();
        }
        // Kullanıcı var mı kontrol ediliyor
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            TempData["Mesaj"] = "Bu eposta adresi kayıtlı değil";
            return View();
        }

        // Rastgele bir string üretir, Ama sadece rastgele değil, Kullanıcıya özel ,Güvenli (Data Protection API kullanır) ,Süresi vardır (default: genelde 1–24 saat)
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        // Token + UserId linke gömülür
        var url = Url.Action("ResetPassword", "Account", new { userId = user.Id, token });

        // Link mail ile gönderilir; Bu linke sadece mail hesabına erişebilen kişi tıklayabilir
        var link = $"<a href='http://localhost:5101{url}'>Şifre Yenile</a>";

        await _emailService.SendEmailAsync(user.Email!, "Parola Sıfırlama", link);

        TempData["Mesaj"] = "Eposta adresine gönderilen link ile şifreni sıfırlayabilirsin.";

        return RedirectToAction("Login");
    }

    // Kullanıcı linke tıklar → ResetPassword sayfası açılır
    public async Task<ActionResult> ResetPassword(string userId, string token)
    {
        if (userId == null || token == null)
        {
            return RedirectToAction("Login");
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return RedirectToAction("Login");
        }

        var model = new AccountResetPasswordModel
        {
            Token = token,
            Email = user.Email!
        };

        return View(model);
    }

    [HttpPost]
    public async Task<ActionResult> ResetPassword(AccountResetPasswordModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // Token doğrulanır ve şifre değiştirilir Bu method:Token doğru mu?, Token süresi dolmuş mu?, Token kullanılmış mı?, Token user’a mı ait?
            // Hepsi uygunsa → şifre değişir
            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

            if (result.Succeeded)
            {
                TempData["Mesaj"] = "Şifreniz güncellendi";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
        return View(model);
        // “Token  manuel olarak DB’ye kaydetmiyorsun.
        // ASP.NET Identity:
        // Token’ı hash’leyerek, Kendi Data Protection altyapısında doğrular, Token stateless çalışır (çoğu senaryoda)
        // Yani:“Ben bu token’ı ben mi üretmiştim?” sorusunu sistem kendi algoritmasıyla cevaplar
    }

}