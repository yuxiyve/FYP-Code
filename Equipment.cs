namespace FYP
{
    internal class Equipment
    {

        public int EIndex { get; set; }
        public List<Location>? GetLocations { get; set; } = null;
        public int OperationCost { get; set; }
        public int TotalVolume { get; set; }
        public int UsedVolume { get; set; }
        public int TransportTime { get; set; }


    }
}
