using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

class Program
{
    static List<string> Solve(Dictionary<char, HashSet<char>> graph, int gateWayEdgesCount)
    {
        var result = new List<string>();
        var startPos = 'a';
        while (gateWayEdgesCount-- > 0)
        {
            var distances = BFS(startPos, graph);
            var priority = new Dictionary<int, List<(char Prev, char GateWay, char VirusNextStep)>>();

            foreach (var pair in distances)
            {
                foreach (var chars in pair.Value)
                {
                    var neighboursCount = graph[chars.Prev].Count(x => x <= 'Z');
                    var weight = pair.Key - neighboursCount;
                    priority.TryAdd(weight, []);
                    priority[weight].Add(chars);
                }
            }

            var mostPriority = priority
                .OrderBy(x => x.Key)
                .First()
                .Value
                .OrderBy(x => x.GateWay)
                .ThenBy(x => x.Prev)
                .First();
            var mostNearest = distances
                .OrderBy(x => x.Key)
                .First(x => x.Value.Any(y => y.GateWay == mostPriority.GateWay))
                .Value;
            var toRemove = mostNearest
                .FirstOrDefault(x => x.Prev == mostPriority.Prev);
            if (toRemove is ('\0', '\0', '\0'))
                toRemove = mostNearest
                    .Where(x => x.GateWay == mostPriority.GateWay)
                    .OrderBy(x => x.GateWay)
                    .ThenBy(x => x.Prev)
                    .First();

            graph[toRemove.Prev].Remove(toRemove.GateWay);
            graph[toRemove.GateWay].Remove(toRemove.Prev);
            result.Add($"{toRemove.GateWay}-{toRemove.Prev}");

            if (gateWayEdgesCount == 0) break;

            var nextMove = distances
                .OrderBy(x => x.Key)
                .First(x => (x.Value.Count > 1 && x.Value.Contains(toRemove)) || !x.Value.Contains(toRemove))
                .Value
                .OrderBy(x => x.GateWay)
                .ThenBy(x => x.Prev)
                .First(x => x != toRemove);

            startPos = nextMove.VirusNextStep;
        }

        return result;
    }

    static void Main()
    {
        var graph = new Dictionary<char, HashSet<char>>();
        var gateWayEdgesCount = 0;
        string line;

        while (!string.IsNullOrEmpty(line = Console.ReadLine()))
        {
            line = line.Trim();
            if (string.IsNullOrEmpty(line)) continue;
            var parts = line.Split('-');
            if (parts.Length != 2) continue;
            var first = parts[0][0];
            var second = parts[1][0];
            graph.TryAdd(first, []);
            graph[first].Add(second);

            graph.TryAdd(second, []);
            graph[second].Add(first);

            if (second <= 'Z') gateWayEdgesCount++;
            if (first <= 'Z') gateWayEdgesCount++;
        }

        var result = Solve(graph, gateWayEdgesCount);
        foreach (var edge in result)
        {
            Console.WriteLine(edge);
        }
    }

    static Dictionary<int, List<(char Prev, char GateWay, char VirusNextStep)>> BFS(char startPos,
        Dictionary<char, HashSet<char>> graph)
    {
        var distances = new Dictionary<int, List<(char, char, char)>>();
        var q = new Queue<(char Previous, char GateWay, char VirusNextStep, int Distance)>();
        var visited = new HashSet<char>();
        q.Enqueue((startPos, startPos, startPos, 0));
        while (q.Count > 0)
        {
            var (previous, current, virusNextStep, distance) = q.Dequeue();
            if (current <= 'Z')
            {
                distances.TryAdd(distance, []);
                distances[distance].Add((previous, current, virusNextStep));
            }

            else
            {
                if (!visited.Add(current)) continue;
            }

            foreach (var neighbour in graph[current].Where(x => !visited.Contains(x)))
            {
                q.Enqueue((current, neighbour, virusNextStep == startPos ? neighbour : virusNextStep, distance + 1));
            }
        }

        return distances;
    }
}