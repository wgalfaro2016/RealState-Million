namespace RealEstate.Application.Dtos
{
    public record PropertyImageDto
    {
        public Guid Id { get; init; }
        public Guid PropertyId { get; init; }
        public string Url { get; init; } = string.Empty;
        public bool IsCover { get; init; }
    }
}
