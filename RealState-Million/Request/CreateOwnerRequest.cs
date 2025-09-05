namespace RealState_Million.Request
{
    public record CreateOwnerRequest(
    string Name,
    string? Address,
    string? Photo,
    DateTime? Birthday
    );

}
