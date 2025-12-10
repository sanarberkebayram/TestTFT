using System;
using System.Collections.Generic;

namespace TestTFT.Scripts.Runtime.Systems.Board
{
    public class BoardGrid
    {
        public const int Width = 7;  // X: 0..6
        public const int Height = 8; // Y: 0..7

        private readonly int?[,] _cells;

        public BoardGrid()
        {
            _cells = new int?[Width, Height];
        }

        public bool InBounds(GridPos pos)
        {
            return pos.X >= 0 && pos.X < Width && pos.Y >= 0 && pos.Y < Height;
        }

        public bool IsOccupied(GridPos pos)
        {
            if (!InBounds(pos)) return false;
            return _cells[pos.X, pos.Y].HasValue;
        }

        public int? Get(GridPos pos)
        {
            if (!InBounds(pos)) return null;
            return _cells[pos.X, pos.Y];
        }

        public bool Place(int entityId, GridPos pos, out string error)
        {
            error = string.Empty;
            if (!InBounds(pos)) { error = "OutOfBounds"; return false; }
            if (IsOccupied(pos)) { error = "CellOccupied"; return false; }
            _cells[pos.X, pos.Y] = entityId;
            return true;
        }

        public bool Move(GridPos from, GridPos to, out string error)
        {
            error = string.Empty;
            if (!InBounds(from)) { error = "FromOutOfBounds"; return false; }
            if (!InBounds(to)) { error = "ToOutOfBounds"; return false; }
            if (!IsOccupied(from)) { error = "FromEmpty"; return false; }
            if (IsOccupied(to)) { error = "ToOccupied"; return false; }
            var id = _cells[from.X, from.Y];
            _cells[from.X, from.Y] = null;
            _cells[to.X, to.Y] = id;
            return true;
        }

        public bool Remove(GridPos pos, out int removedEntityId, out string error)
        {
            error = string.Empty;
            removedEntityId = -1;
            if (!InBounds(pos)) { error = "OutOfBounds"; return false; }
            if (!IsOccupied(pos)) { error = "CellEmpty"; return false; }
            removedEntityId = _cells[pos.X, pos.Y] ?? -1;
            _cells[pos.X, pos.Y] = null;
            return true;
        }

        public IEnumerable<GridPos> AllOccupied()
        {
            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
                if (_cells[x, y].HasValue)
                    yield return new GridPos(x, y);
            }
        }
    }
}

