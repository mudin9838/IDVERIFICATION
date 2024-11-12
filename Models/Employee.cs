using System.ComponentModel.DataAnnotations;

namespace IDVERIFICATION.Models;

public class Employee
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Position is required")]
    public string Position { get; set; }

    [Required(ErrorMessage = "Tax Center is required")]
    public string TaxCenter { get; set; }

    public string? QRCodeUrl { get; set; }
}
