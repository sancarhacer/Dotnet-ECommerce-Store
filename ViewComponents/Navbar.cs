//viewcompanent: controllerdan bağımsız olarak kendisi gidip veri tabanına bağlanan viewe gönderen cs dosyasıdır
//@await Component.InvokeAsync("Navbar") ile view tarafında istenilen yerde kullanılır

using dotnet_store.Models;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_store.ViewComponents;

public class Navbar : ViewComponent
{
    private readonly DataContext _context;

    public Navbar(DataContext context)
    {
        _context = context;
    }

    public IViewComponentResult Invoke()
    {
        return View(_context.Kategoriler.ToList());
    }
}