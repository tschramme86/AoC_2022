using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2022.days
{
    internal class Day23
    {
        private static (int x, int y) N = (0, -1);
        private static (int x, int y) NW = (-1, -1);
        private static (int x, int y) NE = (1, -1);
        private static (int x, int y) E = (1, 0);
        private static (int x, int y) W = (-1, 0);
        private static (int x, int y) SE = (1, 1);
        private static (int x, int y) SW = (-1, 1);
        private static (int x, int y) S = (0, 1);

        public static void Solve()
        {
            Console.WriteLine("*** 23rd December ***");
            Console.WriteLine();

            Debug.Assert(SimulateMovingElves("data/d23-test.txt", 10) == 110);
            SimulateMovingElves("data/d23.txt", 10);

            Debug.Assert(SimulateMovingElves("data/d23-test.txt") == 20);
            SimulateMovingElves("data/d23.txt");
        }

        static int SimulateMovingElves(string inputMapFile, int? rounds = null)
        {
            var map = new Dictionary<(int x, int y), object>();

            var y = 1;
            foreach(var line in File.ReadAllLines(inputMapFile))
            {
                for(var x=0; x<line.Length; x++)
                {
                    if (line[x] == '#') map[(x, y)] = new object();
                }
                y++;
            }

            Console.WriteLine($"Simulating '{inputMapFile}' with {map.Count} elves...");
            if (map.Count == 0) return 0;

            var consideredMoves = new[] { N, S, W, E };
            var allDirections = new[] { NE, N, NW, E, W, SE, S, SW };
            var directionsToCheck = new Dictionary<(int x, int y), (int x, int y)[]>
            {
                { N, new[] { NE, N, NW } },
                { S, new[] { SE, S, SW } },
                { W, new[] { NW, W, SW } },
                { E, new[] { NE, E, SE } },
            };

            var maxRounds = rounds ?? int.MaxValue;
            var exitRound = -1;
            for (var r = 0; r < maxRounds; r++)
            {
                // fist part of the round: each propose a move
                var proposed = new List<(object e, (int x, int y) pNew, (int x, int y) pOld)>();
                foreach (var elve in map)
                {
                    // don't move if no elve is around
                    if (allDirections.All(d => !map.ContainsKey(Add(elve.Key, d))))
                        continue;

                    // find valid move
                    for (var c = r; c < r + consideredMoves.Length; c++)
                    {
                        var direction = consideredMoves[c % consideredMoves.Length];
                        if (directionsToCheck[direction].All(d => !map.ContainsKey(Add(elve.Key, d))))
                        {
                            proposed.Add((elve.Value, Add(elve.Key, direction), elve.Key));
                            break;
                        }
                    }
                }

                // second part of round: do the move if no other elve wants on this position
                var movesToField = proposed.ToLookup(prop => prop.pNew);
                var didMove = false;
                foreach (var validMove in movesToField.Where(m => m.Count() == 1))
                {
                    var elve = validMove.Single();
                    map.Remove(elve.pOld);
                    map.Add(elve.pNew, elve.e);
                    
                    didMove = true;
                }

                if(!didMove)
                {
                    exitRound = r + 1;
                    break;
                }

                // Console.WriteLine($"Map after round {r + 1}:");
                // PrintMap(map);
                // Console.WriteLine();
            }

            if(rounds == null)
            {
                Console.WriteLine($"Simulation stopped in round {exitRound} because of no move.");
                Console.WriteLine();
                return exitRound;
            }

            var minx = map.Keys.Min(k => k.x);
            var maxx = map.Keys.Max(k => k.x);
            var miny = map.Keys.Min(k => k.y);
            var maxy = map.Keys.Max(k => k.y);
            var mapSize = (maxx - minx + 1) * (maxy - miny + 1);
            var emptyTiles = mapSize - map.Count;

            Console.WriteLine($"Simulation found {emptyTiles} empty tiles after {rounds} rounds.");
            Console.WriteLine();

            return emptyTiles;
        }

        static (int x, int y) Add((int x, int y) a, (int x, int y) b)
        {
            return (a.x + b.x, a.y + b.y);
        }

        static void PrintMap(Dictionary<(int x, int y), object> map)
        {
            var minx = map.Keys.Min(k => k.x);
            var maxx = map.Keys.Max(k => k.x);
            var miny = map.Keys.Min(k => k.y);
            var maxy = map.Keys.Max(k => k.y);

            for (var y = miny; y <= maxy; y++)
            {
                for(var x=minx; x <= maxx; x++)
                {
                    if(map.ContainsKey((x,y)))
                        Console.Write("#");
                    else
                        Console.Write(".");
                }
                Console.WriteLine();
            }
        }
    }
}
