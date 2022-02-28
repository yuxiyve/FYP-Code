using O2DESNet;
using O2DESNet.Distributions;

namespace FYP
{
    internal class Warehouse : Sandbox
    {
        //Variables
        //Inbound
        public List<int> order { get; set; }
        public int A { get; set; }
        //Outbound
        public int O { get; set; }
        public int R { get; set; }
        public int Number { get; set; }
        //Popularity
        public int K1 { get; set; }

        //Inbound Section
        SKU InitialSKU(int index, List<Equipment> EquipmentList)
        {
            SKU sKU = new SKU();
            sKU.Index = index;
            List<Location> SKUlocations = new();
            sKU.Locations = SKUlocations;
            Location L1 = new()
            {
                SKU = sKU,
                Equipment = EquipmentList[0],
                ReservedQuantity = 0,
                OccupiedQuantity = 0,
                Fill = new List<SKUfill>(),
            };
            Location L2 = new()
            {
                SKU = sKU,
                Equipment = EquipmentList[1],
                ReservedQuantity = 0,
                OccupiedQuantity = 0,
                Fill = new List<SKUfill>(),
            };
            Location L3 = new()
            {
                SKU = sKU,
                Equipment = EquipmentList[2],
                ReservedQuantity = 0,
                OccupiedQuantity = 0,
                Fill = new List<SKUfill>(),
            };
            SKUlocations.Add(L1);
            SKUlocations.Add(L2);
            SKUlocations.Add(L3);
            sKU.Quantity = 0;
            Random rnd = new();
            sKU.Volume = rnd.Next(5, 20);
            sKU.Mean = rnd.Next(2, 8);
            sKU.StandardDeviation = rnd.Next(1, 3);
            sKU.IntervalSalesTime = Math.Abs((int)Normal.Sample(rnd, sKU.Mean, sKU.StandardDeviation)) + 2;
            sKU.FutureIST = Math.Abs((int)Normal.Sample(rnd, sKU.Mean, sKU.StandardDeviation)) + 2;
            return sKU;
        }

        OutboundLine InitialOL(int index)
        {
            OutboundLine outboundLine = new()
            {
                OIndex = index,
                Fill = new List<SKUfill>(),
                PackingCost = 0,
                PackingTime = 0,
            };
            return outboundLine;
        }

        void InboundArrival(SKU sku, List<SKU> SKUs, List<Equipment> EquipmentList, List<OutboundLine> OutboundLines)
        {
            //update sku quantity
            sku.Quantity += A;
            //choose next schedule new arrival event, check if it is new sku
            Random rnd = new();
            int length = SKUs.Count;
            int NextA = rnd.Next(length + 1);
            if (NextA >= length)
            {
                NextA = length;
                SKUs.Add(InitialSKU(NextA, EquipmentList));
                length++;
                Schedule(() => OutboundOrderGenerate(SKUs[NextA], OutboundLines), TimeSpan.FromDays(SKUs[NextA].IntervalSalesTime));
            }
            Schedule(() => InboundArrival(SKUs[NextA], SKUs, EquipmentList, OutboundLines), TimeSpan.FromHours(2));

            //schedule sku location
            Schedule(() => SKULocate(SKUs[sku.Index], EquipmentList));


            //output
            Console.WriteLine($"{ ClockTime }\tInboound Arrival, Item index = { sku.Index }, Quantity = { sku.Quantity },IST = {sku.IntervalSalesTime}");
        }
        void SKULocate(SKU sku, List<Equipment> EquipmentList)
        {
            void AssignSKU(Location location)
            {
                if (location.ReservedQuantity >= A)
                {
                    location.ReservedQuantity -= A;
                    location.OccupiedQuantity += A;
                }
                else
                {
                    location.Equipment.UsedVolume += (A - location.ReservedQuantity) * location.SKU.Volume;
                    location.ReservedQuantity = 0;
                    location.OccupiedQuantity += A;
                }
            }

            //assign into equipment
            int assign;
            
            if (sku.IntervalSalesTime < K1 ) assign = 0;
            else if (sku.IntervalSalesTime < 10 && sku.IntervalSalesTime >= K1) assign = 1;
            else assign = 2;
            if (assign == 0 && EquipmentList[order[0]].TotalVolume - EquipmentList[order[0]].UsedVolume < A * sku.Volume) assign = 1;
            if (assign == 1 && EquipmentList[order[1]].TotalVolume - EquipmentList[order[1]].UsedVolume < A * sku.Volume) assign = 2;
            AssignSKU(sku.Locations[order[assign]]);
            int remain = EquipmentList[order[assign]].TotalVolume - EquipmentList[order[assign]].UsedVolume;

            //output
            Console.WriteLine($"{ ClockTime }\tSKU Located, Item index = { sku.Index }, Eqiuipment = { sku.Locations[order[assign]].Equipment.EIndex }, remain = { remain }");
        }

        //Outbound Section
        void OutboundOrderGenerate(SKU sku, List<OutboundLine> OutboundLines)
        {
            OutboundLines.Add(InitialOL(Number));
            int need = O;
            if (sku.Quantity >= O)
            {
                int i = 0;
                while ( i <3 && need >0)
                {
                    SKUfill fill = new()
                    {
                        Location = sku.Locations[order[i]],
                        OutboundLine = OutboundLines[Number],
                        Quanity = Math.Min(sku.Locations[order[i]].OccupiedQuantity, need),
                    };
                    OutboundLines[Number].Fill.Add(fill);
                    //quantity
                    need -= fill.Quanity;
                    sku.Locations[order[i]].OccupiedQuantity -= fill.Quanity;
                    sku.Locations[order[i]].Equipment.UsedVolume -= fill.Quanity * sku.Volume;
                    //Cost
                    OutboundLines[Number].PackingCost += fill.Location.Equipment.OperationCost;
                    OutboundLines[Number].PackingTime += fill.Location.Equipment.TransportTime;
                    i++;
                }
                sku.Quantity -= O;
            }

            Schedule(() => OutboundOrderGenerate(sku, OutboundLines), TimeSpan.FromDays(sku.IntervalSalesTime));

            //output
            Console.WriteLine($"{ ClockTime }\tOrder Generated, Item index = { sku.Index },Packing cost = { OutboundLines[Number].PackingCost}");
            Number++;
        }

        //Popularity Section
        void PopularityUpdate(SKU sku, List<SKU> SKUs)
        {
            // update and set the future IST
            Random rnd = new();
            sku.IntervalSalesTime = sku.FutureIST;
            sku.FutureIST = Math.Abs((int)Normal.Sample(rnd, sku.Mean, sku.StandardDeviation)) + 2;

            if (sku.FutureIST <= K1)  Schedule(() => VolumeReserve(sku));

            //schedule next update
            Schedule(() => PopularityUpdate(sku, SKUs), TimeSpan.FromDays(14));
            Console.WriteLine($"{ ClockTime }\tPopularity Update, Item index = { sku.Index }, Next Interval Sales time = { sku.FutureIST }");
        }
        void VolumeReserve(SKU sku)
        {
            if (sku.Locations[0].ReservedQuantity < R)
            {
                sku.Locations[0].ReservedQuantity = R;
                sku.Locations[0].Equipment.UsedVolume += R * sku.Volume;
            }
        }

        void Evaluation(List<OutboundLine> outboundLines)
        {
            int time = 0;
            int cost = 0;
            int count = 0;
            foreach (OutboundLine p in outboundLines)
            {
                if (p.PackingTime != 0)
                {
                    count++;
                    time += p.PackingTime;
                    cost += p.PackingCost;
                }
            }
            int cycletime = time / count;
            int averagecost = cost / count;
            //output
            Console.WriteLine($"{ ClockTime }\tSimulation evaluated, Cycle Time = { cycletime }, Average Cost = { averagecost }");
        }
        public Warehouse(int seed) : base(seed)
        {
            int CurrentA = 1;
            order = new List<int>() { 1, 2, 0 };
            A = 20;
            O = 50;
            R = 50;
            Number = 0;
            K1 = 5;
            //Initialize Equipment
            List<Equipment> EquipmentList = new();
            Equipment E1 = new()
            {
                EIndex = 0,
                GetLocations = new List<Location>(),
                OperationCost = 5,
                TotalVolume = 50000,
                UsedVolume = 0,
                TransportTime = 20,

            };
            Equipment E2 = new()
            {
                EIndex = 1,
                GetLocations = new List<Location>(),
                OperationCost = 10,
                TotalVolume = 10000,
                UsedVolume = 0,
                TransportTime = 10,
            };
            Equipment E3 = new()
            {
                EIndex = 2,
                GetLocations = new List<Location>(),
                OperationCost = 15,
                TotalVolume = 5000,
                UsedVolume = 0,
                TransportTime = 5,
            };
            EquipmentList.Add(E1);
            EquipmentList.Add(E2);
            EquipmentList.Add(E3);

            //initialize exist 30 SKUs
            List<SKU> SKUs = new();
            for (int i = 0; i < 30; i++) SKUs.Add(InitialSKU(i, EquipmentList));

            // initial outboundline list
            List<OutboundLine> OutboundLines = new();
            
            //schedule events
            Schedule(() => InboundArrival(SKUs[CurrentA], SKUs, EquipmentList, OutboundLines));
            foreach (SKU p in SKUs) Schedule(() => PopularityUpdate(p, SKUs),TimeSpan.FromDays(14));
            foreach (SKU p in SKUs) Schedule(() => OutboundOrderGenerate(p, OutboundLines), TimeSpan.FromDays(p.IntervalSalesTime));
            Schedule(() => Evaluation(OutboundLines), TimeSpan.FromDays(60));
        }
    }
}
