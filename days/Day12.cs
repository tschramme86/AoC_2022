using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2022.days
{
    internal class Day12
    {
        class MapLocation
        {
            public int X { get; set; }
            public int Y { get; set; }

            public char Height { get; set; }

            public List<MapLocation> Neighbors { get; } = new List<MapLocation>();

            public int Score { get; set; } = -1;

            public MapLocation? BestPredecessor { get; set; }
        }

        public static void Solve()
        {
            Console.WriteLine("*** 12th December ***");
            Console.WriteLine();

            Debug.Assert(FindShortestPath("data/d12-test.txt", false) == 31);
            FindShortestPath("data/d12.txt", false);

            Debug.Assert(FindShortestPath("data/d12-test.txt", true) == 29);
            FindShortestPath("data/d12.txt", true);
        }

        static int FindShortestPath(string heightMap, bool allowOtherStart)
        {
            var allLines = File.ReadAllLines(heightMap);
            var h = allLines.Length;
            var w = allLines[0].Length;

            Console.WriteLine($"Simulating HeightMap {heightMap} of {w}x{h} fields");

            // parse the heightmap and build a location grid
            var locationGrid = new MapLocation[h][];
            var allLocations = new List<MapLocation>();
            MapLocation? start = null;
            MapLocation? end = null;
            for (var y = 0; y < h; y++)
            {
                locationGrid[y] = new MapLocation[allLines[y].Length];
                Debug.Assert(w == allLines[y].Length);
                for (var x = 0; x < w; x++)
                {
                    var location = new MapLocation
                    {
                        X = x,
                        Y = y
                    };
                    var cH = allLines[y][x];
                    if (cH == 'S')
                    {
                        start = location;
                        cH = 'a';
                    }
                    if (cH == 'E')
                    {
                        end = location;
                        cH = 'z';
                    }
                    location.Height = cH;
                    locationGrid[y][x] = location;
                    allLocations.Add(location);
                }
            }

            Debug.Assert(start != null);
            Debug.Assert(end != null);

            // build neighborhood (only valid steps)
            for (var y = 0; y < h; y++)
            {
                for(var x=0; x < w; x++) 
                {
                    var l = locationGrid[y][x];
                    if (x > 0 && locationGrid[y][x - 1].Height <= l.Height + 1) l.Neighbors.Add(locationGrid[y][x - 1]);
                    if (x < w - 1 && locationGrid[y][x + 1].Height <= l.Height + 1) l.Neighbors.Add(locationGrid[y][x + 1]);
                    if (y > 0 && locationGrid[y - 1][x].Height <= l.Height + 1) l.Neighbors.Add(locationGrid[y - 1][x]);
                    if (y < h - 1 && locationGrid[y + 1][x].Height <= l.Height + 1) l.Neighbors.Add(locationGrid[y + 1][x]);
                }
            }

            // possible start positions - either just the defined start position or all lowest positions of height 'a'
            var allowedStartPositions = allowOtherStart ?
                allLocations.Where(l => l.Height == 'a').ToList() : new List<MapLocation> { start };

            // find shortest path
            var bestPathLength = int.MaxValue;
            MapLocation? bestPathStart = null;

            // iterate over all possible start positions
            foreach (var sLocation in allowedStartPositions)
            {
                // reset the grid for a new search
                allLocations.ForEach(l =>
                {
                    l.Score = -1;
                    l.BestPredecessor = null;
                });

                var score = sLocation.Score = 0;
                var searchList = new List<MapLocation> { sLocation };
                var foundPath = false;
                while (searchList.Any())
                {
                    score++;
                    var nextGenList = new List<MapLocation>();
                    foreach (var location in searchList)
                    {
                        foreach (var n in location.Neighbors)
                        {
                            if (n.Score < 0)
                            {
                                n.Score = score;
                                n.BestPredecessor = location;
                                nextGenList.Add(n);

                                if (n == end)
                                {
                                    foundPath = true;
                                    break;
                                }
                            }
                        }
                        if (foundPath) break;
                    }

                    if (foundPath) break;
                    searchList = nextGenList;
                }

                // did we found a path?
                if (foundPath)
                {
                    var path = new List<MapLocation>();
                    var cL = end;
                    while (cL != sLocation)
                    {
                        path.Add(cL);
                        cL = cL.BestPredecessor!;
                    }

                    path.Reverse();
                    var pathLength = path.Count;

                    if(pathLength < bestPathLength)
                    {
                        bestPathLength = pathLength;
                        bestPathStart = sLocation;
                    }
                }
            }

            if(bestPathLength < int.MaxValue && bestPathStart != null)
            {
                Console.WriteLine($"Found best path of length {bestPathLength}, starting at {bestPathStart.X},{bestPathStart.Y}");
            }
            else
            {
                Console.WriteLine("No valid path found");
            }

            Console.WriteLine();
            return bestPathLength;
        }
    }
}
