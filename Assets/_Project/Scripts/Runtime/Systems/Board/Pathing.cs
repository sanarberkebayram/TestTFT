using System.Collections.Generic;

namespace TestTFT.Scripts.Runtime.Systems.Board
{
    public static class Pathing
    {
        private static readonly (int dx, int dy)[] CardinalDirs =
        {
            (1, 0), (-1, 0), (0, 1), (0, -1)
        };

        public static int ManhattanDistance(GridPos a, GridPos b)
        {
            return System.Math.Abs(a.X - b.X) + System.Math.Abs(a.Y - b.Y);
        }

        public static IEnumerable<GridPos> NeighborsCardinal(GridPos pos, BoardGrid grid, HashSet<GridPos> blocked = null)
        {
            foreach (var (dx, dy) in CardinalDirs)
            {
                var n = new GridPos(pos.X + dx, pos.Y + dy);
                if (!grid.InBounds(n)) continue;
                if (blocked != null && blocked.Contains(n)) continue;
                yield return n;
            }
        }

        // Simple BFS for shortest path avoiding blocked cells; no diagonals.
        public static bool TryFindPath(BoardGrid grid, GridPos start, GridPos goal, out List<GridPos> path, HashSet<GridPos> blocked = null)
        {
            path = null;
            if (!grid.InBounds(start) || !grid.InBounds(goal)) return false;

            var cameFrom = new Dictionary<GridPos, GridPos>();
            var visited = new HashSet<GridPos> { start };
            var q = new Queue<GridPos>();
            q.Enqueue(start);

            while (q.Count > 0)
            {
                var cur = q.Dequeue();
                if (cur.Equals(goal))
                {
                    path = Reconstruct(cameFrom, start, goal);
                    return true;
                }

                foreach (var n in NeighborsCardinal(cur, grid, blocked))
                {
                    if (visited.Contains(n)) continue;
                    visited.Add(n);
                    cameFrom[n] = cur;
                    q.Enqueue(n);
                }
            }

            return false;
        }

        private static List<GridPos> Reconstruct(Dictionary<GridPos, GridPos> cameFrom, GridPos start, GridPos goal)
        {
            var result = new List<GridPos>();
            var cur = goal;
            result.Add(cur);
            while (!cur.Equals(start))
            {
                cur = cameFrom[cur];
                result.Add(cur);
            }
            result.Reverse();
            return result;
        }
    }
}

