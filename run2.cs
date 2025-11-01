using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class State
{
    public readonly char VirusPosition;
    public readonly HashSet<(char GateWay, char PrevNode)> GateWays;
    public readonly List<string> Result;

    public State(char virusPosition, HashSet<(char GateWay, char PrevNode)> gateWays, List<string> result)
    {
        VirusPosition = virusPosition;
        GateWays = gateWays.ToHashSet();
        Result = result.ToList();
    }

    protected bool Equals(State other)
    {
        return VirusPosition == other.VirusPosition && GateWays.SetEquals(other.GateWays);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((State)obj);
    }

    public override int GetHashCode()
    {
        var hash = (int)VirusPosition;
        return GateWays
            .OrderBy(x => x.GateWay)
            .ThenBy(x => x.PrevNode)
            .Aggregate(hash, (current, pair) => HashCode.Combine(pair, current));
    }
}

class Program
{
    static List<string> Solve(Dictionary<char, HashSet<char>> graph, HashSet<(char GateWay, char PrevNode)> gateWays)
    {
        var initialState = new State('a', gateWays, []);
        var q = new Queue<State>();
        q.Enqueue(initialState);
        var visited = new HashSet<State>();
        while (q.Count > 0)
        {
            var curState = q.Dequeue();
            if (!visited.Add(curState) || curState.VirusPosition <= 'Z') continue;
            var distances = BFS(curState, graph);
            foreach (var pair in distances.OrderBy(x => x.Key))
            {
                foreach (var toRemove in pair.Value.OrderBy(x => x.GateWay).ThenBy(x => x.Prev))
                {
                    if (curState.GateWays.Count == 1)
                    {
                        curState.Result.Add($"{toRemove.GateWay}-{toRemove.Prev}");
                        return curState.Result;
                    }

                    var virusNextStep = distances
                        .OrderBy(x => x.Key)
                        .First(x => (x.Value.Count > 1 && x.Value.Contains(toRemove)) || !x.Value.Contains(toRemove))
                        .Value
                        .OrderBy(x => x.GateWay)
                        .ThenBy(x => x.VirusNextStep)
                        .First(x => x != toRemove)
                        .VirusNextStep;

                    var nextState = new State(virusNextStep, curState.GateWays, curState.Result);
                    nextState.GateWays.Remove((toRemove.GateWay, toRemove.Prev));
                    nextState.Result.Add($"{toRemove.GateWay}-{toRemove.Prev}");
                    if (!visited.Contains(nextState)) q.Enqueue(nextState);
                }
            }
        }

        return [];
    }

    static void Main()
    {
        var graph = new Dictionary<char, HashSet<char>>();
        var gateWays = new HashSet<(char, char)>();
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

            if (second <= 'Z') gateWays.Add((second, first));
            if (first <= 'Z') gateWays.Add((first, second));
        }

        // var s = Stopwatch.StartNew();
        var result = Solve(graph, gateWays);
        foreach (var edge in result)
        {
            Console.WriteLine(edge);
        }

        // s.Stop();
        // Console.WriteLine(s.ElapsedMilliseconds);
    }

    static Dictionary<int, List<(char Prev, char GateWay, char VirusNextStep)>> BFS(State state,
        Dictionary<char, HashSet<char>> graph)
    {
        var movesByDistance = new Dictionary<int, List<(char, char, char)>>();
        var distances = new Dictionary<char, int>();
        var q = new Queue<(char Previous, char GateWay, char VirusNextStep, int Distance)>();
        var visited = new HashSet<char>();
        q.Enqueue((state.VirusPosition, state.VirusPosition, state.VirusPosition, 0));
        while (q.Count > 0)
        {
            var (previous, current, virusNextStep, distance) = q.Dequeue();
            if (current <= 'Z')
            {
                if (state.GateWays.Contains((current, previous)))
                {
                    movesByDistance.TryAdd(distance, []);
                    movesByDistance[distance].Add((previous, current, virusNextStep));
                }

                continue;
            }

            else
            {
                if (!visited.Add(current) && distances[current] != distance) continue;
                distances[current] = distance;
            }

            foreach (var neighbour in graph[current].Where(x => !visited.Contains(x)))
            {
                q.Enqueue((current, neighbour, virusNextStep == state.VirusPosition ? neighbour : virusNextStep,
                    distance + 1));
            }
        }

        return movesByDistance;
    }
}