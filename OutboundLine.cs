namespace FYP
{
    internal class OutboundLine
    {
        public int OIndex { get; set; }
        public List<SKUfill>? Fill { get; set; }
        public int PackingTime { get; set; }
        public int PackingCost { get; set; }
    }
}
