namespace RealEstate.Application.Dtos
{
    public record OwnerDto(
        Guid IdOwner,
        string Name,
        string? Address,
        string? Photo,
        DateTime? Birthday
    );
}
