namespace RealEstate.Application.Dtos
{
    public record PropertyDto(
       Guid IdProperty,
       string Name,
       string Address,
       decimal Price,
       string CodeInternal,
       int Year,
       Guid IdOwner
   );
}
