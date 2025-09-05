using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RealEstate.Domain.Entities
{
    [Table("PropertyImage")]
    public class PropertyImage
    {
        [Key]
        [Column("IdPropertyImage")]
        public Guid IdPropertyImage { get; set; }

        [Column("IdProperty")]
        public Guid PropertyId { get; set; }

        [Column("File")]
        [MaxLength(300)]
        public string Url { get; set; } = string.Empty;

        [Column("Enabled")]
        public bool IsCover { get; set; }
    }
}
