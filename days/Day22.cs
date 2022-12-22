using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2022.days
{
    internal class Day22
    {
		enum MapTile 
		{
			Open,
			Wall
		}
        private static (int x, int y)[] directions = new[] { (1, 0), (0, 1), (-1, 0), (0, -1) };

        public static void Solve()
        {
            Console.WriteLine("*** 22nd December ***");
            Console.WriteLine();

            Debug.Assert(CalcFinalPassword("data/d22-test.txt", false) == 6032);
            CalcFinalPassword("data/d22.txt", false);

            Debug.Assert(CalcFinalPassword("data/d22-test.txt", true) == 5031);
            CalcFinalPassword("data/d22.txt", true);
        }

        static long CalcFinalPassword(string inputMap, bool isCubeMap)
        {			
			var map = new Dictionary<(int x, int y), MapTile>();
			var lines = File.ReadAllLines(inputMap);
			if (lines.Length == 0) return 0;

			var row = 1;
			foreach(var line in lines.Take(lines.Length - 2)) {
				for(var col=1;col<=line.Length;col++) {
					if(line[col-1] == '.') map.Add((col, row), MapTile.Open);
					if(line[col-1] == '#') map.Add((col, row), MapTile.Wall);
				}
				row++;
			}

			var minx = map.Keys.Min(k => k.x);
			var maxx = map.Keys.Max(k => k.x);
			var miny = map.Keys.Min(k => k.y);
			var maxy = map.Keys.Max(k => k.y);

			var instructions = lines[lines.Length - 1].Replace("R", " R ").Replace("L", " L ").Split(' ');
			cubeWrap.Clear();

            Console.WriteLine($"Calculating final password on map '{inputMap}' ({map.Count} tiles, {instructions.Length} instructions)");

			var d = 0;
			var p = map.Where(kvp => kvp.Key.y == miny && kvp.Value == MapTile.Open).OrderBy(kvp => kvp.Key.x).First().Key;
			foreach(var instr in instructions) {
				if(instr == "R") d = (d + 1) % directions.Length;
				else if(instr == "L") d = (d + (directions.Length - 1)) % directions.Length;
				else {
					for(var m=0; m<int.Parse(instr); m++)
					{
						var next = Add(p, directions[d]);
						var nextD = d;
						if (isCubeMap && !map.ContainsKey(next))
						{
							(next, nextD) = CubeWrap(next, d, map.Count > 100);
						}
						else
						{
							// wrap around
							while (!map.ContainsKey(next))
							{
								next = Add(next, directions[d]);
								if (next.x < minx) next.x = maxx;
								if (next.x > maxx) next.x = minx;
								if (next.y < miny) next.y = maxy;
								if (next.y > maxy) next.y = miny;
							}
						}
						if (map[next] == MapTile.Wall) break;

						Console.WriteLine($"{p} -> {next}");
						p = next;
						d = nextD;
                    }
				}
			}

			var finalPassword = 1000 * p.y + 4 * p.x + d;
			Console.WriteLine($"Final password is {finalPassword}");
			Console.WriteLine();

			return finalPassword;
        }


		private static Dictionary<((int x, int y), int d), ((int x, int y), int d)> cubeWrap = new();
		static ((int x, int y), int d) CubeWrap((int x, int y) p, int d, bool isLargeMap)
		{
            if (cubeWrap.Count == 0)
            {
				if (isLargeMap)
				{
                    for (var e = 0; e < 50; e++)
                    {
                        cubeWrap[((51 + e, 0), 3)] = ((1, 151 + e), 0);
                        cubeWrap[((101 + e, 0), 3)] = ((1 + e, 200), 3);
                        cubeWrap[((151, e + 1), 0)] = ((100, 150 - e), 2);
                        cubeWrap[((101 + e, 51), 1)] = ((100, 51 + e), 2);
                        cubeWrap[((51 + e, 151), 1)] = ((50, 151 + e), 2);
                        cubeWrap[((0, 101 + e), 2)] = ((51, 50 - e), 0);
                        cubeWrap[((1 + e, 100), 3)] = ((51, 51 + e), 0);
                    }
                }
				else
				{
					for (var e = 0; e < 4; e++)
					{
						cubeWrap[((8,  1 + e), 2)] = ((5 + e, 5), 1);
						cubeWrap[((9 + e, 0), 3)] = ((4 - e, 5), 1);
						cubeWrap[((13, e + 1), 0)] = ((16, 12 - e), 2);
						cubeWrap[((13, e + 5), 0)] = ((16 - e, 9), 1);
						cubeWrap[((13 + e, 13), 1)] = ((1, 8 - e), 0);
						cubeWrap[((9 + e, 13), 1)] = ((4 - e, 8), 3);
						cubeWrap[((8, 9 + e), 2)] = ((8 - e, 8), 3);
					}
				}

				// build reverse part of the map
				foreach(var wrapDef in cubeWrap.ToList())
				{
					var outgoingD = (wrapDef.Value.d + 2) % 4; // turn 180°
					var outgoingP = Add(wrapDef.Value.Item1, directions[outgoingD]);

					var incomingD = (wrapDef.Key.d + 2) % 4; // turn 180°
					var incomingP = Add(wrapDef.Key.Item1, directions[incomingD]);

					cubeWrap[(outgoingP, outgoingD)] = (incomingP, incomingD);
				}
            }

			var t = cubeWrap[(p, d)];
			Console.WriteLine($"({(p, d)} wrapping to {t}");

            return cubeWrap[(p, d)];
		}

        static (int x, int y) Add((int x, int y) a, (int x, int y) b)
        {
            return (a.x + b.x, a.y + b.y);
        }
    }
}
