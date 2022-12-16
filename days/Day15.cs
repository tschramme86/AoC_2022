using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AoC2022.days
{
    internal class Day15
    {
        public static Regex rxSensorBeacon = new Regex(".+x=(?<sx>-?\\d+).+y=(?<sy>-?\\d+).+x=(?<bx>-?\\d+).+y=(?<by>-?\\d+)",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        [DebuggerDisplay("({X},{Y})")]
        struct V
        {
            public V(int x, int y) { X = x; Y = y; }

            public int X { get; set; }
            public int Y { get; set; }

            public override int GetHashCode()
            {
                return this.X + 100 * this.Y;
            }

            public override bool Equals(object? obj)
            {
                if(obj is V v2)
                    return this.X == v2.X && this.Y == v2.Y;
                return false;
            }

            public static int MDist(V v1, V v2)
            {
                return Math.Abs(v1.X - v2.X) + Math.Abs(v1.Y - v2.Y);
            }
        }

        [DebuggerDisplay("S = {Sensor}, B = {Beacon} (d = {SBDist})")]
        class SensorBeacon
        {
            public SensorBeacon(V sensor, V beacon)
            {
                this.Sensor = sensor;
                this.Beacon = beacon;
                this.SBDist = V.MDist(this.Sensor, this.Beacon);
            }

            public V Sensor { get; set; }

            public V Beacon { get; set; }

            public int SBDist { get; }

            public bool CanOtherBeaconExists(V beacon)
            {
                return this.SBDist < V.MDist(this.Sensor!, beacon);
            }

            public IEnumerable<int> GetNoBeaconsInRow(int row)
            {
                if(row < this.Sensor.Y - this.SBDist || row > this.Sensor.Y + this.SBDist) yield break;

                var yd = Math.Abs(this.Sensor.Y - row);
                var xd = this.SBDist - yd;
                yield return this.Sensor.X;
                for (var offsetx = 1; offsetx <= xd; offsetx++)
                {
                    yield return this.Sensor.X + offsetx;
                    yield return this.Sensor.X - offsetx;
                }
            }

            public IEnumerable<V> GetOuterCircle()
            {
                for(var y=this.Sensor.Y - this.SBDist - 1; y <= this.Sensor.Y + this.SBDist + 1; y++)
                {
                    var yd = Math.Abs(this.Sensor.Y - y);
                    var xd = (this.SBDist + 1) - yd;
                    if(xd == 0)
                    {
                        yield return new V(this.Sensor.X, y);
                    } else
                    {
                        yield return new V(this.Sensor.X - xd, y);
                        yield return new V(this.Sensor.X + xd, y);
                    }
                }
            }
        }

        public static void Solve()
        {
            Console.WriteLine("*** 15th December ***");
            Console.WriteLine();

            Debug.Assert(PositionsWithoutBeacon("data/d15-test.txt", 10) == 26);
            PositionsWithoutBeacon("data/d15.txt", 2000000);

            Debug.Assert(FindDistressBeacon("data/d15-test.txt", true) == 56000011);
            FindDistressBeacon("data/d15.txt", false);
        }

        static int PositionsWithoutBeacon(string sensorBeaconFile, int row)
        {
            var sensorBeacons = ReadInput(sensorBeaconFile);
            Console.WriteLine($"Checking row {row} for Sensor-Beacon file {sensorBeaconFile} ({sensorBeacons.Count} entries)");

            var beaconsInRow = new HashSet<int>(sensorBeacons.Where(sb => sb.Beacon!.Y == row).Select(sb => sb.Beacon!.X));
            var noBeaconPositions = new HashSet<int>();

            foreach (var beacon in sensorBeacons)
            {
                foreach(var x in beacon.GetNoBeaconsInRow(row))
                {
                    if(!beaconsInRow.Contains(x)) noBeaconPositions.Add(x);
                }
            }

            Console.WriteLine($"Found {noBeaconPositions.Count} possible positions without beacon in row {row}");
            Console.WriteLine();

            return noBeaconPositions.Count;
        }

        static long FindDistressBeacon(string sensorBeaconFile, bool limitedSearch)
        {
            var sensorBeacons = ReadInput(sensorBeaconFile);
            Console.WriteLine($"Searching distress beacon for Sensor-Beacon file {sensorBeaconFile} ({sensorBeacons.Count} entries)");

            // story part two: find distress beacon
            var maxC = limitedSearch ? 20 : 4000000;

            var circles = sensorBeacons.Select(sb => sb.GetOuterCircle()).ToList();
            var positionsNotVisibleByAnySensor = circles.AsParallel()
                .SelectMany(circle => 
                    circle.Where(v => v.X >= 0 && v.X <= maxC && v.Y >= 0 && v.Y <= maxC && sensorBeacons.All(sb => sb.CanOtherBeaconExists(v))))
                .Distinct().ToList();

            var freq = 0L;
            if(positionsNotVisibleByAnySensor.Any())
            {
                var dBeacon = positionsNotVisibleByAnySensor.First();
                freq = dBeacon.X * 4000000L + dBeacon.Y;
                Console.WriteLine($"Found distress beacon at ({dBeacon.X},{dBeacon.Y}) with frequency {freq}");
            } else
            {
                Console.WriteLine("Distress beacon not found");
            }
            Console.WriteLine();
            return freq;
        }

        static List<SensorBeacon> ReadInput(string sensorBeaconFile)
        {
            var sbLines = File.ReadAllLines(sensorBeaconFile);
            var sensorBeacons = new List<SensorBeacon>();

            foreach (var line in sbLines)
            {
                var sbMatch = rxSensorBeacon.Match(line);
                if (sbMatch.Success)
                {
                    sensorBeacons.Add(new SensorBeacon(
                     new V(int.Parse(sbMatch.Groups["sx"].Value), int.Parse(sbMatch.Groups["sy"].Value)),
                     new V(int.Parse(sbMatch.Groups["bx"].Value), int.Parse(sbMatch.Groups["by"].Value))
                    ));
                }
            }

            return sensorBeacons;
        }
    }
}