namespace FYP
{
    internal class Location
    {
        public SKU? SKU { get; set; } = null;
        public Equipment? Equipment { get; set; } = null;
        public int ReservedQuantity { get; set; }
        public int OccupiedQuantity { get; set; }
        public List<SKUfill>? Fill { get; set; }


    }
}
