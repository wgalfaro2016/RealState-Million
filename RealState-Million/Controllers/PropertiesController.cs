using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Application.Commands;
using RealEstate.Application.Dtos;
using RealEstate.Application.Interfaces;
using RealEstate.Application.Queries;
using RealState_Million.Adapters;
using RealState_Million.Request;

namespace RealState_Million.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropertiesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IWebHostEnvironment _env;

        public PropertiesController(IMediator mediator, IWebHostEnvironment env) 
        {
            _mediator = mediator;
            _env = env;
        }


        [HttpPost("CreateProperties")]
        [Authorize(Policy = "properties:write")]
        [ProducesResponseType(typeof(PropertyDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<PropertyDto>> Create([FromForm] CreatePropertyRequest form,
                                                            CancellationToken ct) {

            var created = await _mediator.Send(new CreatePropertyCommand(form.Name,
                                                                         form.Address,
                                                                         form.Price,
                                                                         form.CodeInternal,
                                                                         form.Year,
                                                                         form.IdOwner),
                                                                         ct);

            if (form.Images is { Count: > 0 }) {
                List<IFileResource> files = form.Images
                    .Where(f => f is { Length: > 0 })
                    .Select(f => (IFileResource)new FormFileResource(f))
                    .ToList();

                await _mediator.Send(new UploadPropertyImagesCommand(created.IdProperty, files, form.CoverIndex), ct);
            }

            return Ok(created);
        }

        [HttpPatch("{id:guid}/ChangePropertyPrice")]
        [Authorize(Policy = "properties:price")]
        [ProducesResponseType(typeof(PropertyDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PropertyDto>> ChangePrice(Guid id,
                                                                 [FromBody] ChangePriceRequest body,
                                                                 CancellationToken ct) 
        {
            var result = await _mediator.Send(
                new ChangePropertyPriceCommand(id, body.NewPrice), ct);

            return Ok(result);
        }

        [HttpPatch("{id:guid}/UpdateProperty")]
        [Authorize(Policy = "properties:write")]
        [ProducesResponseType(typeof(PropertyDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PropertyDto>> Update(Guid id,
                                                            [FromBody] UpdatePropertyRequest body,
                                                            CancellationToken ct) 
        {
            var result = await _mediator.Send(new UpdatePropertyCommand(
                id,
                body.Name,
                body.Address,
                body.Price,
                body.CodeInternal,
                body.Year,
                body.IdOwner
            ), ct);

            return Ok(result);
        }

        [HttpGet("GetProperties")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedResultDto<PropertyListItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<PropertyListItemDto>>> Get(
                [FromQuery] string? name,
                [FromQuery] string? address,
                [FromQuery] string? codeInternal,
                [FromQuery] Guid? ownerId,
                [FromQuery] decimal? minPrice,
                [FromQuery] decimal? maxPrice,
                [FromQuery] int? minYear,
                [FromQuery] int? maxYear,
                [FromQuery] bool onlyWithImages = false,
                [FromQuery] bool includeAllImages = true,
                [FromQuery] string? sortBy = "price",
                [FromQuery] bool desc = false,
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 20,
                CancellationToken ct = default) 
        {
            var result = await _mediator.Send(new ListOfPropertiesQuery(
                Name: name,
                Address: address,
                CodeInternal: codeInternal,
                OwnerId: ownerId,
                MinPrice: minPrice,
                MaxPrice: maxPrice,
                MinYear: minYear,
                MaxYear: maxYear,
                OnlyWithImages: onlyWithImages,
                IncludeAllImages: includeAllImages,
                SortBy: sortBy,
                Desc: desc,
                Page: page,
                PageSize: pageSize
            ), ct);

            return Ok(result);
        }

        [HttpPost("{id:guid}/AddTrace")]
        [Authorize(Policy = "properties:trace")]
        [ProducesResponseType(typeof(PropertyTraceDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PropertyTraceDto>> AddTrace(Guid id,
                                                                   [FromBody] AddTraceRequest body,
                                                                   CancellationToken ct) 
        {
            var result = await _mediator.Send(new AddPropertyTraceCommand(
                PropertyId: id,
                Name: body.Name,
                Value: body.Value,
                Tax: body.Tax,
                DateSale: body.DateSale
            ), ct);

            return Ok(result);
        }
    }
}
