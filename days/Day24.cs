using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2022.days
{
    internal class Day24
    {
        class Blizzard
        {
            public (int x, int y) Position { get; set; }

            public (int x, int y) Direction { get; set; }
        }

        public static void Solve()
        {
            Console.WriteLine("*** 24th December ***");
            Console.WriteLine();

            Debug.Assert(FindShortestPath("data/d24-test.txt", false) == 18);
            FindShortestPath("data/d24.txt", false);

            Debug.Assert(FindShortestPath("data/d24-test.txt", true) == 54);
            FindShortestPath("data/d24.txt", true);
        }

        static int FindShortestPath(string valleyMap, bool doubleCross)
        {
            var lines = File.ReadAllLines(valleyMap);
            
            var map = new Dictionary<(int x, int y), bool>();
            var blizzards = new List<Blizzard>();

            var y = 0;
            foreach (var line in lines)
            {
                for(var x=0; x<line.Length; x++)
                {
                    switch(line[x])
                    {
                        case '#':
                            map[(x,y)] = true; break;
                        case '.':
                            map[(x, y)] = false; break;
                        case '>':
                            map[(x, y)] = false;
                            blizzards.Add(new Blizzard
                            {
                                Position = (x, y),
                                Direction = (1, 0)
                            });
                            break;
                        case '<':
                            map[(x, y)] = false;
                            blizzards.Add(new Blizzard
                            {
                                Position = (x, y),
                                Direction = (-1, 0)
                            });
                            break;
                        case '^':
                            map[(x, y)] = false;
                            blizzards.Add(new Blizzard
                            {
                                Position = (x, y),
                                Direction = (0, -1)
                            });
                            break;
                        case 'v':
                            map[(x, y)] = false;
                            blizzards.Add(new Blizzard
                            {
                                Position = (x, y),
                                Direction = (0, 1)
                            });
                            break;
                    }
                }
                y++;
            }
            var startPos = map.Where(kvp => kvp.Key.y == 0).First(kvp => kvp.Value == false).Key;
            var endPos = map.Where(kvp => kvp.Key.y == y - 1).First(kvp => kvp.Value == false).Key;
            var targets = doubleCross ? new[] { endPos, startPos, endPos } : new[] { endPos };

            Console.WriteLine($"Simulating valley '{valleyMap}' with {blizzards.Count} blizzards...");

            var step = 1;
            var nextTarget = 0;
            var proposedMoves = new (int x, int y)[] { (0, 0), (1, 0), (-1, 0), (0, 1), (0, -1) };
            var nextPosCandidates = new HashSet<(int x, int y)>(new[] { startPos });
            do
            {
                // move blizzards
                foreach(var b in blizzards)
                {
                    b.Position = Add(b.Position, b.Direction);

                    // hit the wall?
                    if (map[b.Position])
                    {
                        do
                        {
                            b.Position = Sub(b.Position, b.Direction);
                        } while (!map[b.Position]);
                        b.Position = Add(b.Position, b.Direction);
                    }
                }

                // compute possible next positions
                var blizzardPos = new HashSet<(int x, int y)> (blizzards.Select(b => b.Position));
                var newCandidateList = new HashSet<(int x, int y)>();
                foreach(var p in nextPosCandidates)
                {
                    foreach(var move in proposedMoves)
                    {
                        var newPos = Add(p, move);
                        if (!map.ContainsKey(newPos) || map[newPos]) continue;
                        if (blizzardPos.Contains(newPos)) continue;
                        newCandidateList.Add(newPos);
                    }
                }

                // is one of the possible next position our target position?
                if (newCandidateList.Contains(targets[nextTarget]))
                {
                    var p = targets[nextTarget];
                    nextTarget++;
                    if (nextTarget >= targets.Length) break;
                    Console.WriteLine(" turning around...");

                    newCandidateList.Clear();
                    newCandidateList.Add(p);
                }
                nextPosCandidates = newCandidateList;

                step++;
            } while (true);

            Console.WriteLine($"Reached end of valley after {step} steps.");
            Console.WriteLine();
            return step;
        }

        static (int x, int y) Add((int x, int y) a, (int x, int y) b)
        {
            return (a.x + b.x, a.y + b.y);
        }
        static (int x, int y) Sub((int x, int y) a, (int x, int y) b)
        {
            return (a.x - b.x, a.y - b.y);
        }
    }
}
