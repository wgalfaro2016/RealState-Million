namespace RealEstate.Domain.Entities
{
    public class Property
    {
        public Guid IdProperty { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string CodeInternal { get; set; } = string.Empty;
        public int Year { get; set; }

        public Guid IdOwner { get; set; }
        public Owner Owner { get; set; } = null!;

        public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
        public ICollection<PropertyTrace> Traces { get; set; } = new List<PropertyTrace>();
    }
}
