using RealEstate.Application.Interfaces;

namespace RealState_Million.Adapters
{
    public class FormFileResource : IFileResource
    {
        private readonly IFormFile _file;

        public FormFileResource(IFormFile file) => _file = file;

        public string FileName => _file.FileName;
        public long Length => _file.Length;
        public Stream OpenReadStream() => _file.OpenReadStream();
    }
}
