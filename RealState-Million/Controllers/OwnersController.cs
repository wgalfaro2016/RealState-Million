using MediatR;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Application.Commands;
using RealEstate.Application.Dtos;
using RealState_Million.Request;

namespace RealState_Million.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OwnersController : ControllerBase
    {
        private readonly IMediator _mediator;
        public OwnersController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        [ProducesResponseType(typeof(OwnerDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<OwnerDto>> Create([FromBody] CreateOwnerRequest body, CancellationToken ct) {
            var cmd = new CreateOwnerCommand(body.Name, body.Address, body.Photo, body.Birthday);
            var result = await _mediator.Send(cmd, ct);
            return Ok(result);
        }
    }
}
