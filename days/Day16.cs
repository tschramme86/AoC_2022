using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AoC2022.days
{
    internal class Day16
    {
        [DebuggerDisplay("({Source} -> {Target}")]
        class Edge
        {
            public Edge(Node source, Node target)
            {
                Source = source;
                Target = target;
            }

            public Node Source { get; }

            public Node Target { get; }
        }
        [DebuggerDisplay("{Name} ({Id})")]
        class Node
        {
            public string Name { get; set; } = string.Empty;
            public int Id { get; set; }
            public int FlowRate { get; set; }
            public List<Edge> OutgoingEdges { get; } = new List<Edge>();
            public List<Edge> IncomingEdges { get; } = new List<Edge>();
        }

        static Regex rxFlows = new Regex(
            "Valve (?<source>[A-Z]+) has flow rate=(?<rate>\\d+); tunnels? leads? to valves? (?<targets>[A-Z, ]+)",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public static void Solve()
        {
            Console.WriteLine("*** 16th December ***");
            Console.WriteLine();

            // story part one - only one runner
            Debug.Assert(SimulateFlow("data/d16-test.txt", 30, false) == 1651);
            SimulateFlow("data/d16.txt", 30, false);

            // story part two - two runners
            Debug.Assert(SimulateFlow("data/d16-test.txt", 26, true) == 1707);
            SimulateFlow("data/d16.txt", 26, true);
        }

        static int SimulateFlow(string flowDefinitionFile, int time, bool twoRunners)
        {
            var (nodes, startNode) = ReadNetwork(flowDefinitionFile);
            if (startNode == null) Debug.Assert(false);

            var relevantNodes = (new[] { startNode }).Union(nodes.Where(n => n.FlowRate > 0)).ToArray();

            Console.WriteLine($"Simulating '{flowDefinitionFile}', {relevantNodes.Length} sources for pressure");

            var distances = CalculateDistances(relevantNodes);
            var allPaths = new List<List<Node>>();
            CreatePossiblePaths(startNode, new List<Node> { startNode }, relevantNodes, time, distances, allPaths);

            var pathFlows = allPaths.Select(p => Tuple.Create(p, GetPathFlow(p, distances, time))).OrderByDescending(pf => pf.Item2).ToList();

            if (twoRunners)
            {
                var bestFlow = 0;
                var lockObject = new object();
                var runner1Path = new List<Node>();
                var runner2Path = new List<Node>();

                Parallel.ForEach(pathFlows, (firstPath) =>
                {
                    foreach (var secondPath in pathFlows)
                    {
                        if (firstPath.Item1.Skip(1).Union(secondPath.Item1.Skip(1))
                            .All(n => !(firstPath.Item1.Contains(n) && secondPath.Item1.Contains(n))))
                        {
                            var flow = firstPath.Item2 + secondPath.Item2;
                            lock (lockObject)
                            {
                                if (flow > bestFlow)
                                {
                                    bestFlow = flow;
                                    runner1Path = firstPath.Item1;
                                    runner2Path = secondPath.Item1;
                                    break;
                                }
                            }
                        }
                    }
                });

                Console.WriteLine($"Most pressure in {time} minutes = {bestFlow} (running with elephant)");
                Console.WriteLine();

                return bestFlow;
            }
            else
            {
                var bestPath = pathFlows[0].Item1;
                var bestFlow = pathFlows[0].Item2;

                Console.WriteLine($"Most pressure in {time} minutes = {bestFlow}");
                Console.WriteLine();

                return bestFlow;
            }
        }

        static void CreatePossiblePaths(Node currentNode, List<Node> currentPath, Node[] nodes, int maxPathLength, int[,] distances, List<List<Node>> allPaths)
        {
            if(currentPath.Count == nodes.Length)
            {
                allPaths.Add(currentPath);
                return;
            }

            foreach(var node in nodes)
            {
                if(currentPath.Contains(node)) continue;
                
                var newPath = currentPath.Append(node).ToList();
                if(GetPathLength(newPath, distances) <= maxPathLength)
                    CreatePossiblePaths(node, newPath, nodes, maxPathLength, distances, allPaths);
            }

            allPaths.Add(currentPath);
        }

        static int GetPathFlow(List<Node> path, int[,] distances, int maxTime)
        {
            var flow = 0;
            var remainingTime = maxTime;
            for (var i = 1; i < path.Count; i++)
            {
                remainingTime -= distances[path[i - 1].Id, path[i].Id]; // time to go to this node
                remainingTime--; // time to open this valve
                flow += remainingTime * path[i].FlowRate;
            }
            return flow;
        }

        static int GetPathLength(List<Node> path, int[,] distances)
        {
            var length = path.Count(n => n.FlowRate > 0);
            for(int i=0; i< path.Count - 1; i++)
            {
                length += distances[path[i].Id, path[i + 1].Id];
            }
            return length;
        }

        static int[,] CalculateDistances(Node[] nodes)
        {
            var distMatrix = new int[nodes.Length, nodes.Length];
            for(var i=0; i<nodes.Length; i++) nodes[i].Id = i;

            foreach(var source in nodes)
            {
                foreach(var target in nodes)
                {
                    if(source == target) continue;

                    var d = 0;
                    var visited = new HashSet<Node>();
                    var visitList = new List<Node> { source };
                    var found = false;
                    while(true)
                    {
                        d++;
                        var newVisitList = new List<Node>();
                        foreach(var me in visitList)
                        {
                            foreach(var next in me.OutgoingEdges)
                            {
                                if (visited.Contains(next.Target)) continue;
                                if(next.Target == target)
                                {
                                    distMatrix[source.Id, target.Id] = d;
                                    found = true;
                                    break;
                                } else
                                {
                                    visited.Add(next.Target);
                                    newVisitList.Add(next.Target);
                                }
                            }
                            if (found) break;
                        }

                        if (found || newVisitList.Count == 0) break;
                        visitList = newVisitList;
                    }
                }
            }

            return distMatrix;
        }

        static (Node[], Node) ReadNetwork(string flowDefinitionFile)
        {
            var nodeDict = new Dictionary<string, Node>();
            Node getNode(string name, bool createWhenNotFound)
            {
                if (nodeDict!.TryGetValue(name, out var n))
                    return n;

                if (createWhenNotFound)
                {
                    Node newNode = new Node { Name = name, Id = -1 };
                    nodeDict[name] = newNode;
                    return newNode;
                }
                throw new InvalidOperationException();
            }
            void connectNodes(Node source, Node target)
            {
                var e1 = new Edge(source, target);
                source.OutgoingEdges.Add(e1);
                target.IncomingEdges.Add(e1);
            }

            var lines = File.ReadAllLines(flowDefinitionFile);

            // create sources
            foreach (var line in lines)
            {
                var vDef = rxFlows.Match(line);
                if(vDef.Success)
                {
                    var flow = int.Parse(vDef.Groups["rate"].Value);
                    var node = getNode(vDef.Groups["source"].Value, true);
                    node.FlowRate= flow;

                    var targetValves = vDef.Groups["targets"].Value.Split(',');
                    foreach (var tV in targetValves)
                    {
                        var target = getNode(tV.Trim(), true);
                        connectNodes(node, target);
                    }
                }
            }

            return (nodeDict.Values.ToArray(), getNode("AA", false));
        }
    }
}
