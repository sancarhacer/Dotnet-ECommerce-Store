using dotnet_store.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddTransient<IEmailService, SmtpEmailService>();
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<DataContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlite(connectionString);
});
builder.Services.AddIdentity<AppUser, AppRole>().AddEntityFrameworkStores<DataContext>().AddDefaultTokenProviders();
// Configure identity  
// Identity (Kimlik) sisteminin çalışma kurallarını yapılandırıyoruz
builder.Services.Configure<IdentityOptions>(options =>
{
    // --- Şifre Kuralları ---
    options.Password.RequiredLength = 10;           // Şifre en az 10 karakter uzunluğunda olmalıdır.
    options.Password.RequireNonAlphanumeric = false; // Şifrede özel karakter (!, @, # vb.) zorunluluğunu kaldırır.
    options.Password.RequireUppercase = false;      // Şifrede en az bir büyük harf bulunma zorunluluğunu kaldırır.
    options.Password.RequireLowercase = false;      // Şifrede en az bir küçük harf bulunma zorunluluğunu kaldırır.
    options.Password.RequireDigit = false;          // Şifrede en az bir rakam (0-9) bulunma zorunluluğunu kaldırır.

    // --- Kullanıcı Kuralları ---
    options.User.RequireUniqueEmail = true;         // Aynı e-posta adresiyle birden fazla kayıt yapılmasını engeller.
    
    // Kullanıcı adında izin verilen karakterleri belirler. 
    // Boş bırakılırsa tüm karakterlere izin verir veya varsayılan kısıtlamaları kaldırabilir.
    options.User.AllowedUserNameCharacters = "";  

    // Kullanıcı kaç kere yanlış şifre girerse hesabı kilitlensin?
    options.Lockout.MaxFailedAccessAttempts = 5;

    // Hesap kilitlendiğinde ne kadar süre "Cezalı" kalsın?
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(2);
});

builder.Services.ConfigureApplicationCookie(options =>
{
    //Kullanıcı giriş yapmadan yasaklı bir yere girerse nereye gitsin? (Varsayılan: /Account/Login)
    options.LoginPath = "/Account/Login";

    // Kullanıcı giriş yapmış ama o sayfaya yetkisi yoksa (Örn: Müşteri, Admin paneline girmeye çalışırsa) nereye gitsin?
    options.AccessDeniedPath = "/Account/AccessDenied";

    //Bu çerez ne kadar süre geçerli olsun? (Örn: 30 gün boyunca bir daha şifre sorma).
    options.ExpireTimeSpan = TimeSpan.FromDays(30);

    //Eğer true ise, kullanıcı siteyi kullandıkça bu 30 günlük süre her seferinde baştan başlar. Yani kullanıcı aktifse oturumu hiç kapanmaz.
    options.SlidingExpiration = true;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

// app.MapStaticAssets();
//ASP.NET Core'da app.UseStaticFiles(); komutu, varsayılan olarak projenizdeki wwwroot klasörünü dışarıya açar. 
    // Buradaki resim, CSS ve JavaScript dosyalarına herkes erişebilir.
app.UseStaticFiles(); // Sadece wwwroot içindekileri internete aç
 
//roote dışarıya açmak istemezsek staticler için klaösor açıp içine yazabiliriz

//urunler/telefon
app.MapControllerRoute(
    name: "urunler_by_kategori",
    pattern: "urunler/{url?}",
    defaults: new { controller = "Urun", action = "List" }
    )
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

//Kullanıcı arama kutusuna Apple yazıp submit yaparsa:
// Tarayıcı şu URL’i oluşturur: /Urun/List?q=Apple
// Bu default route tarafından karşılanır ({controller=Home}/{action=Index}/{id?}), 
// çünkü özel route olan urunler/{url} URL’de /urunler/... yapısını bekliyor.
// url parametresi zorunlu olursa ({url}), route eşleşmezse form GET isteği özel route’u kullanamaz ve default route’a gider.


app.Run();
