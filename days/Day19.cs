using Google.OrTools.LinearSolver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AoC2022.days
{
    internal class Day19
    {
        class Blueprint
        {
            public int Id { get; set; }
            public int OreRobotCostOre { get; set; }
            public int ClayRobotCostOre { get; set; }
            public int ObsidianRobotCostOre { get; set; }
            public int ObsidianRobotCostClay { get; set; }
            public int GeodeRobotCostOre { get; set; }
            public int GeodeRobotCostObsidian { get; set; }
        }

        static Regex rxBlueprint = new Regex(
              "Blueprint (?<bpId>\\d+): Each ore robot costs (?<oreRobotOre" +
              "C>\\d+) ore. Each clay robot costs (?<clayRobotOreC>\\d+) or" +
              "e. Each obsidian robot costs (?<obsRobotOreC>\\d+) ore and (" +
              "?<obsRobotClayC>\\d+) clay. Each geode robot costs (?<geoRob" +
              "otOreC>\\d+) ore and (?<geoRobotObsC>\\d+) obsidian.",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public static void Solve()
        {
            Console.WriteLine("*** 19th December ***");
            Console.WriteLine();

            Debug.Assert(CalcQualityLevel("data/d19-test.txt", 24, false) == 33);
            CalcQualityLevel("data/d19.txt", 24, false);

            Debug.Assert(CalcQualityLevel("data/d19-test.txt", 32, true) == 56 * 62);
            CalcQualityLevel("data/d19.txt", 32, true);
        }

        static int CalcQualityLevel(string blueprintFile, int time, bool limitToThree)
        {
            var blueprints = new List<Blueprint>();
            foreach(var line in File.ReadAllLines(blueprintFile))
            {
                var parsed = rxBlueprint.Match(line);
                if(parsed.Success)
                {
                    blueprints.Add(new Blueprint
                    {
                        Id = int.Parse(parsed.Groups["bpId"].Value),
                        OreRobotCostOre = int.Parse(parsed.Groups["oreRobotOreC"].Value),
                        ClayRobotCostOre = int.Parse(parsed.Groups["clayRobotOreC"].Value),
                        ObsidianRobotCostOre = int.Parse(parsed.Groups["obsRobotOreC"].Value),
                        ObsidianRobotCostClay = int.Parse(parsed.Groups["obsRobotClayC"].Value),
                        GeodeRobotCostOre = int.Parse(parsed.Groups["geoRobotOreC"].Value),
                        GeodeRobotCostObsidian = int.Parse(parsed.Groups["geoRobotObsC"].Value),
                    });
                }

                if (limitToThree && blueprints.Count == 3) break;
            }

            var qualitySum = 0;
            if (limitToThree)
            {
                qualitySum = 1;
                foreach (var blueprint in blueprints)
                {
                    var bpValue = CalcBlueprint(blueprint, time);
                    qualitySum *= bpValue;
                }
            }
            else
            {
                foreach (var blueprint in blueprints)
                {
                    var bpValue = CalcBlueprint(blueprint, time);
                    qualitySum += bpValue * blueprint.Id;
                }
            }

            if (limitToThree)
                Console.WriteLine($"Input = {blueprintFile} (only top 3), Time = {time}, Solution = {qualitySum}");
            else
                Console.WriteLine($"Input = {blueprintFile}, Time = {time}, Solution = {qualitySum}");

            return qualitySum;
        }

        static int CalcBlueprint(Blueprint blueprint, int minutes)
        {
            var solver = Solver.CreateSolver("SCIP");
            if (solver == null) throw new Exception("Could not create MIP solver");

            var buyOreRobot = new Dictionary<int, Variable>();
            var buyClayRobot = new Dictionary<int, Variable>();
            var buyObsidianRobot = new Dictionary<int, Variable>();
            var buyGeodeRobot = new Dictionary<int, Variable>();

            var oreCostHelper = new Dictionary<int, Variable>();

            // create variables + objective
            var objective = solver.Objective();
            buyOreRobot[0] = solver.MakeBoolVar("oR_0");
            for (var m=2; m<=minutes; m++)
            {
                buyOreRobot[m] = solver.MakeBoolVar($"oR_{m}");
                buyClayRobot[m] = solver.MakeBoolVar($"cR_{m}");
                buyObsidianRobot[m] = solver.MakeBoolVar($"obR_{m}");
                buyGeodeRobot[m] = solver.MakeBoolVar($"gR_{m}");
                oreCostHelper[m] = solver.MakeNumVar(double.NegativeInfinity, double.PositiveInfinity, $"oHlp_{m}");

                objective.SetCoefficient(buyGeodeRobot[m], minutes - m);
            }
            objective.SetMaximization();

            // constraint for robot factory - only one robot per minute
            for (var m = 2; m <= minutes; m++)
            {
                var constrBuildPerMinute = solver.MakeConstraint(0, 1, $"constr_build_{m}");
                constrBuildPerMinute.SetCoefficient(buyOreRobot[m], 1);
                constrBuildPerMinute.SetCoefficient(buyClayRobot[m], 1);
                constrBuildPerMinute.SetCoefficient(buyObsidianRobot[m], 1);
                constrBuildPerMinute.SetCoefficient(buyGeodeRobot[m], 1);
            }

            // constraint for costs
            for (var m = 2; m <= minutes; m++)
            {
                // create constraints for ore costs + collecting up to minute m
                var constrOre = solver.MakeConstraint(double.NegativeInfinity, 0, $"constr_ore_{m}");
                for (var mi = 2; mi <= m; mi++)
                {
                    // sum of ore costs up to period m...
                    constrOre.SetCoefficient(buyGeodeRobot[mi], blueprint.GeodeRobotCostOre);
                    constrOre.SetCoefficient(buyObsidianRobot[mi], blueprint.ObsidianRobotCostOre);
                    constrOre.SetCoefficient(buyClayRobot[mi], blueprint.ClayRobotCostOre);
                    constrOre.SetCoefficient(buyOreRobot[mi], blueprint.OreRobotCostOre);
                }
                constrOre.SetCoefficient(oreCostHelper[m], -1);

                // ...must be lower or equan than sum of ore collecting up to period m - 1
                var constrOre2 = solver.MakeConstraint(0, 0, $"constr_ore2_{m}");
                constrOre2.SetCoefficient(oreCostHelper[m], 1);
                constrOre2.SetCoefficient(buyOreRobot[0], -1 * (m - 1));
                for (var mi = 2; mi < m; mi++)
                {
                    var canCollectUnitsUpNow = Math.Max(0, (m - 1) - mi);
                    constrOre2.SetCoefficient(buyOreRobot[mi], -1 * canCollectUnitsUpNow);
                }

                // create constraints for obsidian costs + collecting up to minute m
                var constrObsidian = solver.MakeConstraint(double.NegativeInfinity, 0, $"constr_obsidian_{m}");
                for (var mi = 2; mi <= m; mi++)
                {
                    var canCollectUnitsUpNow = Math.Max(0, (m - 1) - mi);

                    constrObsidian.SetCoefficient(buyGeodeRobot[mi], blueprint.GeodeRobotCostObsidian);
                    if (mi < m - 1)
                    {
                        constrObsidian.SetCoefficient(buyObsidianRobot[mi], -1 * canCollectUnitsUpNow);
                    }
                }
                
                // create constraints for clay costs + collecting up to minute m
                var constrClay = solver.MakeConstraint(double.NegativeInfinity, 0, $"constr_clay_{m}");
                for (var mi = 2; mi <= m; mi++)
                {
                    var canCollectUnitsUpNow = Math.Max(0, (m - 1) - mi);

                    constrClay.SetCoefficient(buyObsidianRobot[mi], blueprint.ObsidianRobotCostClay);
                    if (mi < m - 1)
                    {
                        constrClay.SetCoefficient(buyClayRobot[mi], -1 * canCollectUnitsUpNow);
                    }
                }
            }

            // uncomment to export the whole model in LP format for other solvers
            // File.WriteAllText(@$"C:\temp\blueprint_{blueprint.Id}.lp", solver.ExportModelAsLpFormat(false));

            // solve the problem
            var solverStatus = solver.Solve();
            if(solverStatus != Solver.ResultStatus.OPTIMAL)
            {
                Console.WriteLine($"Could not solve blueprint {blueprint.Id}");
                return -1;
            }

            Console.WriteLine("========================================================================");
            Console.WriteLine($"Solution for blueprint {blueprint.Id}");
            var objectiveValue = (int)Math.Round(objective.Value());

            var oreRobots = 1;
            var totalOre = 0;
            var clayRobots = 0;
            var totalClay = 0;
            var obsidianRobots = 0;
            var totalObsidian = 0;
            var geodeRobots = 0;
            var totalGeodes = 0;
            for(var m=1; m<=minutes; m++)
            {
                // build new robots?
                Console.WriteLine($"Period {m}");
                if (buyOreRobot.ContainsKey(m) && buyOreRobot[m].SolutionValue() > 0.99) {
                    totalOre -= blueprint.OreRobotCostOre;
                    Console.WriteLine($" - Building Ore Robot ({totalOre} ore left)"); 
                }

                if (buyClayRobot.ContainsKey(m) && buyClayRobot[m].SolutionValue() > 0.99) 
                {
                    totalOre -= blueprint.ClayRobotCostOre;
                    Console.WriteLine($" - Building Clay Robot ({totalOre} ore left)");
                }
                if (buyObsidianRobot.ContainsKey(m) && buyObsidianRobot[m].SolutionValue() > 0.99) 
                {
                    totalOre -= blueprint.ObsidianRobotCostOre;
                    totalClay -= blueprint.ObsidianRobotCostClay;
                    Console.WriteLine($" - Building Obsidian Robot ({totalOre} ore + {totalClay} clay left)");
                }
                if (buyGeodeRobot.ContainsKey(m) && buyGeodeRobot[m].SolutionValue() > 0.99) 
                {
                    totalOre -= blueprint.GeodeRobotCostOre;
                    totalObsidian -= blueprint.GeodeRobotCostObsidian;
                    Console.WriteLine($" - Building Geode Robot ({totalOre} ore + {totalObsidian} obsidian left)");
                }

                // collecting!
                totalOre += oreRobots;
                totalClay += clayRobots;
                totalObsidian += obsidianRobots;
                totalGeodes += geodeRobots;
                Console.WriteLine($" - {oreRobots} ore robots collecting, total ore = {totalOre}");
                Console.WriteLine($" - {clayRobots} clay robots collecting, total clay = {totalClay}");
                Console.WriteLine($" - {obsidianRobots} obsidian robots collecting, total obsidian = {totalObsidian}");
                Console.WriteLine($" - {geodeRobots} geode robots cracking, total geodes = {totalGeodes}");

                // got new robots?
                if (buyOreRobot.ContainsKey(m) && buyOreRobot[m].SolutionValue() > 0.99) oreRobots++;
                if (buyClayRobot.ContainsKey(m) && buyClayRobot[m].SolutionValue() > 0.99) clayRobots++;
                if (buyObsidianRobot.ContainsKey(m) && buyObsidianRobot[m].SolutionValue() > 0.99) obsidianRobots++;
                if (buyGeodeRobot.ContainsKey(m) && buyGeodeRobot[m].SolutionValue() > 0.99) geodeRobots++;

                Console.WriteLine();
            }
            Console.WriteLine($"Will open {objectiveValue} geodes in total");
            Console.WriteLine();

            Debug.Assert(totalGeodes == objectiveValue);
            return objectiveValue;
        }
    }
}