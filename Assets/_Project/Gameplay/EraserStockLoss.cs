namespace EraserWars.Gameplay
{
    public readonly struct EraserStockLoss
    {
        public EraserStockLoss(int victimSeat, int knockerSeat)
        {
            VictimSeat = victimSeat;
            KnockerSeat = knockerSeat;
        }

        public int VictimSeat { get; }
        public int KnockerSeat { get; }

        public static bool TryNotice(
            int stocksBefore,
            int stocksAfter,
            int victimSeat,
            int knockerSeat,
            out EraserStockLoss loss)
        {
            if (stocksAfter < stocksBefore)
            {
                loss = new EraserStockLoss(victimSeat, knockerSeat);
                return true;
            }

            loss = default;
            return false;
        }
    }
}
