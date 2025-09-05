namespace RealEstate.Domain.Entities
{
    public class PropertyTrace
    {
        public Guid IdPropertyTrace { get; set; } = Guid.NewGuid();
        public Guid IdProperty { get; set; }
        public Property Property { get; set; } = null!;

        public DateTime DateSale { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public decimal Tax { get; set; }
    }
}
