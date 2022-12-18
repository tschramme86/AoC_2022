using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2022.days
{
    internal class Day17
    {
        static bool[][,] rocks = new[] {
            new bool[,] { { true, true, true, true } },
            new bool[,] { { false, true, false }, { true, true, true }, { false, true, false } },
            new bool[,] { { false, false, true }, { false, false, true }, { true, true, true } },
            new bool[,] { { true }, { true }, { true }, { true } },
            new bool[,] { { true, true }, { true, true } }
        };

        public static void Solve()
        {
            Console.WriteLine("*** 17th December ***");
            Console.WriteLine();

            Debug.Assert(GetRockTowerHeight("data/d17-test.txt", 2022) == 3068);
            GetRockTowerHeight("data/d17.txt", 2022);

            Debug.Assert(GetRockTowerHeight("data/d17-test.txt", 1000000000000) == 1514285714288);
            GetRockTowerHeight("data/d17.txt", 1000000000000);
        }

        static long GetRockTowerHeight(string jetstreamFile, long steps)
        {
            Console.WriteLine($"Simulating falling rocks with jetstream file {jetstreamFile}");
            var jets = File.ReadAllText(jetstreamFile);

            var iJ = 0;

            var towerHeight = 0;
            var cave = new bool[1][];
            cave[0] = new bool[] { true, true, true, true, true, true, true, true, true };

            var cycleTracker = new Dictionary<(int, int), (long, int)>(); // (rockIndex, jetIndex) -> (iteration, caveHeight)
            var withCycleDetection = steps > Int32.MaxValue;

            for (var i = 0L; i < steps; i++)
            {
                var rockIndex = (int)(i % rocks.Length);
                var rock = rocks[rockIndex];
                var rH = rock.GetLength(0);
                var rW = rock.GetLength(1);

                var rx = 3;
                var ry = towerHeight + 4;

                // resize cave if necessary
                var neededHeight = ry + rH;
                var linesToAdd = neededHeight - cave.Length;
                if (linesToAdd > 0)
                {
                    Array.Resize(ref cave, neededHeight);
                    for (var y = 1; y <= linesToAdd; y++)
                    {
                        cave[cave.Length - y] = new bool[9];
                        cave[cave.Length - y][0] = cave[cave.Length - y][8] = true;
                    }
                }

                while (true)
                {
                    var jetIndex = iJ++ % jets.Length;

                    if (withCycleDetection && cycleTracker.ContainsKey((rockIndex, jetIndex)))
                    {
                        var (pIteration, pHeight) = cycleTracker[(rockIndex, jetIndex)];
                        var cycleLength = i - pIteration;
                        if (i % cycleLength == steps % cycleLength)
                        {
                            var cycleHeight = towerHeight - pHeight;
                            var rocksRemaining = steps - i;
                            var cyclesRemaining = (rocksRemaining / cycleLength) + 1;

                            var towerHeightAtEnd = pHeight + (cycleHeight * cyclesRemaining);

                            Console.WriteLine($"Found cycle after {i} steps, predict final tower height of {towerHeightAtEnd} units");
                            Console.WriteLine();

                            return towerHeightAtEnd;
                        }
                    }
                    else
                        cycleTracker[(rockIndex, jetIndex)] = (i, towerHeight);

                    // try apply jet
                    var nx = rx + (jets[jetIndex] == '<' ? -1 : 1);
                    if (CanPlaceRock(rock, nx, ry, cave, false))
                        rx = nx;

                    // try fall
                    var ny = ry - 1;
                    if (CanPlaceRock(rock, rx, ny, cave, false))
                        ry = ny;
                    else
                    {
                        // place rock and start with next rock
                        CanPlaceRock(rock, rx, ry, cave, true);
                        break;
                    }
                }
                for (var y = cave.Length - 1; y > 0; y--)
                {
                    var foundTop = false;
                    for (var x = 1; x < 8; x++)
                    {
                        if (cave[y][x])
                        {
                            towerHeight = y;
                            foundTop = true;
                            break;
                        }
                    }
                    if (foundTop) break;
                }
            }

            Console.WriteLine($"After {steps} steps (full simulation) the tower has a height of {towerHeight} units");
            Console.WriteLine();

            return towerHeight;
        }

        static bool CanPlaceRock(bool[,] rock, int rx, int ry, bool[][] cave, bool doPlacement)
        {
            var rH = rock.GetLength(0);
            var rW = rock.GetLength(1);

            for (var iy=0; iy < rH; iy++)
            {
                for(var ix=0; ix < rW; ix++)
                {
                    if (doPlacement)
                        cave[ry + iy][rx + ix] |= rock[rH - 1 - iy, ix];
                    else if (rock[rH - 1 - iy, ix] && cave[ry + iy][rx + ix])
                        return false;
                }
            }

            return true;
        }
    }
}
