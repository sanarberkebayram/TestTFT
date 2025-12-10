using System;

namespace TestTFT.Scripts.Runtime.Systems.Board
{
    public class Bench
    {
        public const int Slots = 9;
        private readonly int?[] _slots = new int?[Slots];

        public bool IsOccupied(int index)
        {
            return InBounds(index) && _slots[index].HasValue;
        }

        public bool InBounds(int index) => index >= 0 && index < Slots;

        public int? Get(int index)
        {
            if (!InBounds(index)) return null;
            return _slots[index];
        }

        public bool Place(int entityId, int index, out string error)
        {
            error = string.Empty;
            if (!InBounds(index)) { error = "BenchIndexOutOfBounds"; return false; }
            if (IsOccupied(index)) { error = "BenchSlotOccupied"; return false; }
            _slots[index] = entityId;
            return true;
        }

        public bool Move(int from, int to, out string error)
        {
            error = string.Empty;
            if (!InBounds(from)) { error = "FromBenchIndexOutOfBounds"; return false; }
            if (!InBounds(to)) { error = "ToBenchIndexOutOfBounds"; return false; }
            if (!IsOccupied(from)) { error = "FromBenchEmpty"; return false; }
            if (IsOccupied(to)) { error = "ToBenchOccupied"; return false; }
            var id = _slots[from];
            _slots[from] = null;
            _slots[to] = id;
            return true;
        }

        public bool Remove(int index, out int removedEntityId, out string error)
        {
            error = string.Empty;
            removedEntityId = -1;
            if (!InBounds(index)) { error = "BenchIndexOutOfBounds"; return false; }
            if (!IsOccupied(index)) { error = "BenchSlotEmpty"; return false; }
            removedEntityId = _slots[index] ?? -1;
            _slots[index] = null;
            return true;
        }
    }
}

