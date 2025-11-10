// Veri taşımak için model tanımladık
using System.ComponentModel.DataAnnotations;

namespace dotnet_store.Models;

public class KategoriCreateModel
{
    [Display(Name = "Kategori Adı")]
    public string KategoriAdi { get; set; } = null!;
    [Display(Name = "URL")]
    public string Url { get; set; } = null!;
}