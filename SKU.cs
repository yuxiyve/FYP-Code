namespace FYP
{
    internal class SKU
    {
        public int Index { get; set; }
        public int Volume { get; set; }
        public List<Location>? Locations { get; set; }
        public int Quantity { get; set; }
        public int Mean { get; set; }
        public int StandardDeviation { get; set; }
        public int IntervalSalesTime { get; set; }
        public int FutureIST { get; set; }

    }
}
