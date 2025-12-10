using System.Collections.Generic;

namespace TestTFT.Scripts.Runtime.Systems.Board
{
    // High-level API for board + bench operations with validation.
    public class BoardService
    {
        public BoardGrid Board { get; }
        public Bench Bench { get; }

        // Optional: track occupied cells as blocked for path searches.
        private readonly HashSet<GridPos> _blocked = new HashSet<GridPos>();

        public BoardService()
        {
            Board = new BoardGrid();
            Bench = new Bench();
        }

        // Board operations
        public bool PlaceOnBoard(int entityId, GridPos pos, out string error)
        {
            var ok = Board.Place(entityId, pos, out error);
            if (ok) _blocked.Add(pos);
            return ok;
        }

        public bool MoveOnBoard(GridPos from, GridPos to, out string error)
        {
            // Basic validation: no diagonals enforced by grid steps; path optional.
            // If pathing check is desired, ensure a clear route excluding from cell.
            _blocked.Remove(from);
            var canPath = Pathing.TryFindPath(Board, from, to, out _, _blocked);
            _blocked.Add(from);
            if (!canPath) { error = "NoPath"; return false; }

            var ok = Board.Move(from, to, out error);
            if (ok)
            {
                _blocked.Remove(from);
                _blocked.Add(to);
            }
            return ok;
        }

        public bool RemoveFromBoard(GridPos pos, out int removedEntityId, out string error)
        {
            var ok = Board.Remove(pos, out removedEntityId, out error);
            if (ok) _blocked.Remove(pos);
            return ok;
        }

        // Bench operations
        public bool PlaceOnBench(int entityId, int benchIndex, out string error)
        {
            return Bench.Place(entityId, benchIndex, out error);
        }

        public bool MoveOnBench(int fromIndex, int toIndex, out string error)
        {
            return Bench.Move(fromIndex, toIndex, out error);
        }

        public bool RemoveFromBench(int benchIndex, out int removedEntityId, out string error)
        {
            return Bench.Remove(benchIndex, out removedEntityId, out error);
        }

        // Cross operations
        public bool MoveBenchToBoard(int benchIndex, GridPos to, out string error)
        {
            error = string.Empty;
            if (!Bench.IsOccupied(benchIndex)) { error = "FromBenchEmpty"; return false; }

            var id = Bench.Get(benchIndex)!.Value;
            if (!Board.InBounds(to)) { error = "ToOutOfBounds"; return false; }
            if (Board.IsOccupied(to)) { error = "ToOccupied"; return false; }

            if (!Bench.Remove(benchIndex, out var removed, out error)) return false;
            if (!PlaceOnBoard(id, to, out error))
            {
                // rollback bench remove if place fails
                Bench.Place(removed, benchIndex, out _);
                return false;
            }
            return true;
        }

        public bool MoveBoardToBench(GridPos from, int benchIndex, out string error)
        {
            error = string.Empty;
            if (!Board.InBounds(from)) { error = "FromOutOfBounds"; return false; }
            if (!Board.IsOccupied(from)) { error = "FromEmpty"; return false; }
            if (!Bench.InBounds(benchIndex)) { error = "BenchIndexOutOfBounds"; return false; }
            if (Bench.IsOccupied(benchIndex)) { error = "BenchSlotOccupied"; return false; }

            if (!Board.Remove(from, out var id, out error)) return false;
            if (!Bench.Place(id, benchIndex, out error))
            {
                // rollback board remove if place fails
                Board.Place(id, from, out _);
                return false;
            }
            _blocked.Remove(from);
            return true;
        }
    }
}

