using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2022.days
{
    internal class Day18
    {
        public static void Solve()
        {
            Console.WriteLine("*** 18th December ***");
            Console.WriteLine();

            Debug.Assert(CalcSurfaceArea("data/d18-test.txt", false) == 64);
            CalcSurfaceArea("data/d18.txt", false);

            Debug.Assert(CalcSurfaceArea("data/d18-test.txt", true) == 58);
            CalcSurfaceArea("data/d18.txt", true);
        }

        static int CalcSurfaceArea(string dropletFile, bool excludeInside)
        {
            var surfaceDefinition = File.ReadAllLines(dropletFile);
            
            var filledGrid = new HashSet<(int x, int y, int z)>();
            foreach(var line in surfaceDefinition)
            {
                var coords = line.Split(',');
                filledGrid.Add((int.Parse(coords[0]), int.Parse(coords[1]), int.Parse(coords[2])));
            }

            Console.WriteLine($"Calculating surface area for '{dropletFile}' ({filledGrid.Count} grid positions)");

            // calculate droplet size
            var minx = filledGrid.Min(a => a.x);
            var miny = filledGrid.Min(a => a.y);
            var minz = filledGrid.Min(a => a.z);
            var maxx = filledGrid.Max(a => a.x);
            var maxy = filledGrid.Max(a => a.y);
            var maxz = filledGrid.Max(a => a.z);

            // bfs search if coordinates are on the outside of the droplet
            var knownInsides = new HashSet<(int x, int y, int z)>();
            bool bfsIsOutside(int x, int y, int z)
            {
                if (!excludeInside) return true; // assume everything is outside
                if (knownInsides.Contains((x, y, z))) return false; // tested previously as inside?

                var nextTests = new Queue<(int x, int y, int z)>(new[] { (x, y, z) });
                var tested = new HashSet<(int x, int y, int z)>();

                while(nextTests.TryDequeue(out var c))
                {
                    if (tested.Contains(c)) continue; tested.Add(c);

                    if (!filledGrid.Contains((c.x - 1, c.y, c.z))) if (c.x - 1 < minx) return true; else nextTests.Enqueue((c.x - 1, c.y, c.z));
                    if (!filledGrid.Contains((c.x + 1, c.y, c.z))) if (c.x + 1 > maxx) return true; else nextTests.Enqueue((c.x + 1, c.y, c.z));
                    if (!filledGrid.Contains((c.x, c.y - 1, c.z))) if (c.y - 1 < miny) return true; else nextTests.Enqueue((c.x, c.y - 1, c.z));
                    if (!filledGrid.Contains((c.x, c.y + 1, c.z))) if (c.y + 1 > maxy) return true; else nextTests.Enqueue((c.x, c.y + 1, c.z));
                    if (!filledGrid.Contains((c.x, c.y, c.z - 1))) if (c.z - 1 < minz) return true; else nextTests.Enqueue((c.x, c.y, c.z - 1));
                    if (!filledGrid.Contains((c.x, c.y, c.z + 1))) if (c.z + 1 > maxz) return true; else nextTests.Enqueue((c.x, c.y, c.z + 1));
                }

                foreach(var t in tested) knownInsides.Add(t);   // speed up things by caching previous tested insides
                foreach (var t in nextTests) knownInsides.Add(t);

                return false;
            }

            var exposedSides = 0;
            foreach(var (x,y,z) in filledGrid)
            {
                if (!filledGrid.Contains((x - 1, y, z)) && bfsIsOutside(x - 1, y, z)) exposedSides++;
                if (!filledGrid.Contains((x + 1, y, z)) && bfsIsOutside(x + 1, y, z)) exposedSides++;
                if (!filledGrid.Contains((x, y - 1, z)) && bfsIsOutside(x, y - 1, z)) exposedSides++;
                if (!filledGrid.Contains((x, y + 1, z)) && bfsIsOutside(x, y + 1, z)) exposedSides++;
                if (!filledGrid.Contains((x, y, z - 1)) && bfsIsOutside(x, y, z - 1)) exposedSides++;
                if (!filledGrid.Contains((x, y, z + 1)) && bfsIsOutside(x, y, z + 1)) exposedSides++;
            }
            
            if(excludeInside)
                Console.WriteLine($"Found a total surface area of {exposedSides} (only outside surface)");
            else
                Console.WriteLine($"Found a total surface area of {exposedSides}");
            Console.WriteLine();

            return exposedSides;
        }
    }
}
