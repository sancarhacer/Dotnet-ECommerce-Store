using System.ComponentModel.DataAnnotations;

namespace dotnet_store.Models;

public class UrunModel
{
    [Display(Name = "Ürün Adı")]
    [Required(ErrorMessage = "Urun Adı boş olamaz")]
    [StringLength(50, ErrorMessage = "Urun Adı {2} - {1} karekter olabilir", MinimumLength = 5)]
    public string UrunAdi { get; set; } = null!;
    
    [Display(Name = "Ürün Fiyat")]
    [Required(ErrorMessage = "{0} boş olamaz")]
    public double? Fiyat { get; set; }
    [Display(Name = "Ürün Resmi")]
    public IFormFile? Resim { get; set; }
    public string? Aciklama { get; set; }
    public bool Aktif { get; set; }
    public bool Anasayfa { get; set; }

    [Display(Name = "Kategori")]
    [Required(ErrorMessage = "{0} seçiniz")]
    public int? KategoriId { get; set; }
}

// IFormFile Resim	:Dosyayı kullanıcıdan teslim alır.
// İşlem (Controller)	-	-	Dosyayı sunucuya kaydeder, ismini üretir.
// Veritabanı	Urun (Entity)	string ResimYolu	Sadece dosya ismini saklar.