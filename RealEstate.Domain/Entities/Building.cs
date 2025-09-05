namespace RealEstate.Domain.Entities
{
    public class Building
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PropertyId { get; set; }
        public string Type { get; set; } = "residential";
        public int YearBuilt { get; set; }
    }
}
