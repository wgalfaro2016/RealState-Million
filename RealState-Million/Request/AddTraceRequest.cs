namespace RealState_Million.Request
{
    public sealed class AddTraceRequest
    {
        public string Name { get; init; } = default!;
        public decimal Value { get; init; }
        public decimal Tax { get; init; }
        public DateTime? DateSale { get; init; } 
    }
}
