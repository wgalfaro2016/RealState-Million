namespace RealEstate.Application.Dtos
{
    public record PropertyListItemDto(
      Guid IdProperty,
      string Name,
      string Address,
      decimal Price,
      string CodeInternal,
      int Year,
      Guid IdOwner,
      string? CoverUrl,             
      List<string> ImageUrls         
  );


    public record PagedResultDto<T>(int Page, int PageSize, int Total, List<T> Items);
}
