namespace RealEstate.Domain.Entities
{
    public class Owner
    {
        public Guid IdOwner { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Photo { get; set; }      
        public DateTime? Birthday { get; set; }
        public ICollection<Property> Properties { get; set; } = new List<Property>();
    }
}
