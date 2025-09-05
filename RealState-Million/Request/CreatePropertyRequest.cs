namespace RealState_Million.Request
{
    public class CreatePropertyRequest
    {
        public string Name { get; set; } = default!;
        public string Address { get; set; } = default!;
        public decimal Price { get; set; }
        public string CodeInternal { get; set; } = default!;
        public int Year { get; set; }
        public Guid IdOwner { get; set; }

        public List<IFormFile>? Images { get; set; }    
        public int? CoverIndex { get; set; }     
    }
}
