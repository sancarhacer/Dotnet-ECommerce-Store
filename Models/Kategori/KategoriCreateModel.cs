// Veri taşımak için model tanımladık
using System.ComponentModel.DataAnnotations;

namespace dotnet_store.Models;

public class KategoriCreateModel
{
    [Display(Name = "Kategori Adı")]
    [Required(ErrorMessage = "Kategori adı boş olamaz")]
    [StringLength(30,ErrorMessage ="Kategori Adı maksimum 30 karekter olabilir")]
    public string KategoriAdi { get; set; } = null!;
    
    [Display(Name = "URL")]
    [Required(ErrorMessage ="Url boş olamaz")]
    [StringLength(30,ErrorMessage ="Url maksimum 30 karekter olabilir")]
    public string Url { get; set; } = null!;
}