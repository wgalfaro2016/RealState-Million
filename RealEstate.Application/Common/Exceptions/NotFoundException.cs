namespace RealEstate.Application.Common.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string resource, object key)
            : base($"{resource} '{key}' was not found.") { }
    }
}
