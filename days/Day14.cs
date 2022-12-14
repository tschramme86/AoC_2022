using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AoC2022.days
{
    internal class Day14
    {
        [DebuggerDisplay("V({X},{Y})")]
        struct V
        {
            public int X { get; set; }
            public int Y { get; set; }
        }
        enum CaveTile
        {
            Air,
            Void,
            Rock,
            Sand
        }

        static Regex rxRockLineCoordiate = new Regex("(?<x>\\d+),(?<y>\\d+)", RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public static void Solve()
        {
            Console.WriteLine("*** 14th December ***");
            Console.WriteLine();

            Debug.Assert(SimulateSand("data/d14-test.txt", false) == 24);
            SimulateSand("data/d14.txt", false);

            Debug.Assert(SimulateSand("data/d14-test.txt", true) == 93);
            SimulateSand("data/d14.txt", true);
        }

        static int SimulateSand(string caveFile, bool withFloor)
        {
            // read + parse the input file
            var lines = File.ReadLines(caveFile);
            var rockLines = new List<List<V>>();
            foreach(var line in lines) 
            {
                var l = new List<V>();
                var c = rxRockLineCoordiate.Match(line);
                while(c.Success)
                {
                    l.Add(new V { X = int.Parse(c.Groups["x"].Value), Y = int.Parse(c.Groups["y"].Value) });
                    c = c.NextMatch();
                }
                rockLines.Add(l);
            }

            // dimensions of cave
            var seed = new V { X = 500, Y = 0 };
            var minX = Math.Min(rockLines.Min(l => l.Min(v => v.X)), seed.X);
            var maxX = Math.Max(rockLines.Max(l => l.Max(v => v.X)), seed.X);
            var minY = Math.Min(rockLines.Min(l => l.Min(v => v.Y)), seed.Y);
            var maxY = Math.Max(rockLines.Max(l => l.Max(v => v.Y)), seed.Y);
            if(withFloor)
            {
                minX = 0;
                maxX *= 2;
                maxY += 2;
            }

            var w = maxX - minX + 2;
            var h = maxY - minY + 2;

            var cave = new CaveTile[h][];
            for(var y=0; y<h; y++)
            {
                cave[y] = new CaveTile[w];
            }

            CaveTile GetCaveTile(int x, int y)
            {
                if (y > maxY) return CaveTile.Void;
                return cave![y - minY][x - minX + 1];
            }
            void SetCaveTile(int x, int y, CaveTile tile)
            {
                cave![y - minY][x - minX + 1] = tile;
            }

            // place rock lines
            foreach (var rl in rockLines)
            {
                for(var i=0; i<rl.Count - 1; i++)
                {
                    var c0 = rl[i];
                    var c1 = rl[i + 1];
                    if(c0.X == c1.X)
                    {
                        foreach (var y in Range(c0.Y, c1.Y))
                            SetCaveTile(c0.X, y, CaveTile.Rock);
                    } else
                    {
                        foreach(var x in Range(c0.X, c1.X))
                            SetCaveTile(x, c0.Y, CaveTile.Rock);
                    }
                }
            }
            if(withFloor)
            {
                foreach (var x in Range(minX, maxX))
                    SetCaveTile(x, maxY, CaveTile.Rock);
            }

            Console.WriteLine($"Simulating cave '{caveFile}' from X = {minX} to {maxX}, Y = {minY} to {maxY}...");
            if (withFloor) Console.WriteLine("The cave has a floor!");

            var sandUnitsAtRest = 0;
            while(true)
            {
                var sand = seed;
                var reachedVoid = false;
                var reachedTop = false;
                while(true)
                {
                    if(GetCaveTile(sand.X, sand.Y + 1) <= CaveTile.Void)
                        sand = new V { X = sand.X, Y = sand.Y + 1 };
                    else if(GetCaveTile(sand.X - 1, sand.Y + 1) <= CaveTile.Void)
                        sand = new V { X = sand.X - 1, Y = sand.Y + 1 };
                    else if(GetCaveTile(sand.X + 1, sand.Y + 1) <= CaveTile.Void)
                        sand = new V { X = sand.X + 1, Y = sand.Y + 1 };
                    else
                    {
                        SetCaveTile(sand.X, sand.Y, CaveTile.Sand);
                        if (sand.X == seed.X && sand.Y == seed.Y)
                        {
                            sandUnitsAtRest++;
                            reachedTop = true;
                        }
                        break;
                    }

                    if (GetCaveTile(sand.X, sand.Y) == CaveTile.Void)
                    {
                        reachedVoid = true;
                        break;
                    }
                }
                if (reachedVoid || reachedTop) break;
                sandUnitsAtRest++;
            }

            Console.WriteLine($"There came {sandUnitsAtRest} sand packages to rest in the cave");
            Console.WriteLine();

            return sandUnitsAtRest;
        }

        static IEnumerable<int> Range(int i, int j)
        {
            if(i < j) for(var c=i; c<=j; c++) yield return c;
            else for(var c=j; c<=i; c++) yield return c;
        }
    }
}
