using MediatR;
using RealEstate.Application.Interfaces;

namespace RealEstate.Application.Commands
{
    public record UploadPropertyImagesCommand(
        Guid PropertyId,
        IReadOnlyList<IFileResource> Files,
        int? CoverIndex
    ) : IRequest<int>;

    public class UploadPropertyImagesHandler
        : IRequestHandler<UploadPropertyImagesCommand, int>
    {
        private readonly IFileStorageService _storage;
        private readonly IMediator _mediator;

        public UploadPropertyImagesHandler(IFileStorageService storage, IMediator mediator) {
            _storage = storage;
            _mediator = mediator;
        }

        public async Task<int> Handle(UploadPropertyImagesCommand r, CancellationToken ct) {
            if (r.Files is null || r.Files.Count == 0) return 0;

            var processed = 0;
            for (int i = 0; i < r.Files.Count; i++) {
                var file = r.Files[i];
                if (file is null || file.Length <= 0) continue;

                var url = await _storage.SavePropertyImageAsync(r.PropertyId, file, ct);
                var isCover = r.CoverIndex.HasValue && r.CoverIndex.Value == i;

                await _mediator.Send(new AddPropertyImageCommand(r.PropertyId, url, isCover), ct);
                processed++;
            }
            return processed;
        }
    }
}
