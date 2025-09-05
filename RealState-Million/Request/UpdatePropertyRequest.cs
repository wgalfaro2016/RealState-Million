namespace RealState_Million.Request
{
    public class UpdatePropertyRequest
    {
        public string? Name { get; init; }
        public string? Address { get; init; }
        public decimal? Price { get; init; }
        public string? CodeInternal { get; init; }
        public int? Year { get; init; }
        public Guid? IdOwner { get; init; }
    }
}
