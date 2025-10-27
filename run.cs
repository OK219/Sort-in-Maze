using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

class Program
{
    private static readonly Dictionary<char, int> RoomMapChar = new Dictionary<char, int>()
    {
        { 'A', 2 }, { 'B', 4 }, { 'C', 6 }, { 'D', 8 }
    };

    private static readonly Dictionary<int, char> RoomMapInt = new Dictionary<int, char>()
    {
        { 2, 'A' }, { 4, 'B' }, { 6, 'C' }, { 8, 'D' }
    };

    private static readonly Dictionary<char, int> Weights = new Dictionary<char, int>()
    {
        { 'A', 1 }, { 'B', 10 }, { 'C', 100 }, { 'D', 1000 }
    };

    static int Solve(List<string> lines)
    {
        var initialState = GetInitialState(lines);
        //var result = BruteForce(initialState);
        var result = RealAStar(initialState);
        return result;
    }

    static State GetInitialState(List<string> lines)
    {
        lines = lines.Skip(2)
            .Take(lines.Count - 3)
            .ToList();
        var hall = "...........".ToCharArray();
        var rooms = new char[4].Select(x => new char[lines.Count]).ToArray();
        for (var i = 0; i < lines.Count; i++)
        {
            rooms[0][i] = lines[i][3];
            rooms[1][i] = lines[i][5];
            rooms[2][i] = lines[i][7];
            rooms[3][i] = lines[i][9];
        }

        return new State(hall, rooms);
    }

    static (State NextState, int Weight) TakeFromRoom(State state, int roomNum)
    {
        var (hall, rooms) = state;
        for (var i = 0; i < rooms[roomNum].Length; i++)
        {
            var character = rooms[roomNum][i];
            if (character == '.') continue;
            var weight = (i + 1) * Weights[character];
            hall[roomNum * 2 + 2] = character;
            rooms[roomNum][i] = '.';
            return (new State(hall, rooms), weight);
        }

        throw new Exception("Can't take from empty room");
    }

    static (State NextState, int Weight) MoveInHall(State state, int initialPos, int targetPos)
    {
        var start = Math.Min(initialPos, targetPos);
        var end = Math.Max(initialPos, targetPos);
        state.GetHall(out var hall);
        if (hall[initialPos] == '.') throw new Exception("Can't move nothing");
        for (var i = start; i <= end; i++)
        {
            if (hall[i] != '.' && i != initialPos)
            {
                throw new Exception("Can't reach target position");
            }
        }

        hall[targetPos] = hall[initialPos];
        hall[initialPos] = '.';

        return (new State(hall, state.Rooms), Math.Abs(targetPos - initialPos) * Weights[hall[targetPos]]);
    }

    static (State NextState, int Weight) PutInRoom(State state, int hallPosition)
    {
        var (hall, rooms) = state;
        var roomNum = (hallPosition - 2) / 2;
        var character = hall[hallPosition];
        if (character == '.') throw new Exception("Can't move nothing");
        if (RoomMapChar[character] != hallPosition) throw new Exception("Letter and room types incompatible");
        if (rooms[roomNum].Any(x => x != character && x != '.')) throw new Exception("Can't enter the room now");
        var deepestPlace = 0;
        for (var i = 0; i < rooms[roomNum].Length; i++)
        {
            if (rooms[roomNum][i] == '.') deepestPlace = i;
            else break;
        }

        hall[hallPosition] = '.';
        rooms[roomNum][deepestPlace] = character;
        return (new State(hall, rooms), (deepestPlace + 1) * Weights[character]);
    }

    static bool CanTake(State state, int roomNum)
    {
        return state.Rooms[roomNum].Any(x => x != RoomMapInt[roomNum * 2 + 2] && x != '.');
    }

    static bool CanPut(State state, int hallPos)
    {
        return state.Rooms[(hallPos - 2) / 2].All(x => x == '.' || x == RoomMapInt[hallPos]);
    }

    static bool CanMove(State state, int initialPos, int targetPos)
    {
        var start = Math.Min(initialPos, targetPos);
        var end = Math.Max(initialPos, targetPos);
        for (var i = start; i <= end; i++)
        {
            if (state.Hall[i] != '.' && i != initialPos)
            {
                return false;
            }
        }

        return true;
    }

    static bool IsCompleted(State state)
    {
        return !state.Rooms.Where((room, j) => room.Any(x => x != RoomMapInt[j * 2 + 2])).Any();
    }

    static (State state, int Weight) MoveAndPut(State state, int weight, int hallPos)
    {
        var roomIdx = RoomMapChar[state.Hall[hallPos]];
        var (nextState, moveWeight) = MoveInHall(state, hallPos, roomIdx);
        var (putState, putWeight) = PutInRoom(nextState, roomIdx);
        return (putState, weight + moveWeight + putWeight);
    }

    static int RealAStar(State initialState)
    {
        var q = new PriorityQueue<State, int>();
        var weights = new Dictionary<State, int>
        {
            [initialState] = 0
        };
        var initialWeight = H(initialState);
        q.Enqueue(initialState, initialWeight);
        while (q.Count > 0)
        {
            var state = q.Dequeue();
            if (IsCompleted(state))
                return weights[state];
            var states = GetPossibleStates(state);
            foreach (var (nextState, nextWeight) in states)
            {
                var newWeight = weights[state] + nextWeight;
                if (weights.TryGetValue(nextState, out var value) && newWeight >= value) continue;
                weights[nextState] = newWeight;
                q.Enqueue(nextState, newWeight + H(nextState));
            }
        }

        return -1;
    }

    static List<(State state, int Weight)> GetPossibleStates(State state)
    {
        var states = new List<(State, int)>();
        for (var i = 0; i < state.Hall.Length; i++)
        {
            if (state.Hall[i] == '.') continue;
            var targetPos = RoomMapChar[state.Hall[i]];
            if (!(CanMove(state, i, targetPos) && CanPut(state, targetPos))) continue;
            var (putState, nextWeight) = MoveAndPut(state, 0, i);
            states.Add((putState, nextWeight));
        }

        for (var i = 0; i < state.Rooms.Length; i++)
        {
            if (!CanTake(state, i)) continue;
            var (takeState, takeWeight) = TakeFromRoom(state, i);
            var hallPos = i * 2 + 2;
            if (CanMove(state, hallPos, RoomMapChar[takeState.Hall[hallPos]]) &&
                CanPut(state, RoomMapChar[takeState.Hall[hallPos]]))
            {
                var (putState, nextWeight) = MoveAndPut(takeState, 0 + takeWeight, hallPos);
                states.Add((putState, nextWeight));
            }
            else
            {
                for (var j = 0; j < takeState.Hall.Length; j++)
                {
                    if (RoomMapInt.ContainsKey(j) || !CanMove(takeState, hallPos, j)) continue;
                    var (moveState, moveWeight) = MoveInHall(takeState, hallPos, j);
                    states.Add((moveState, 0 + takeWeight + moveWeight));
                }
            }
        }

        return states;
    }

    static int H(State state)
    {
        var weight = 0;
        for (var i = 0; i < state.Hall.Length; i++)
        {
            if (state.Hall[i] == '.') continue;
            var roomPos = RoomMapChar[state.Hall[i]];
            weight += Math.Abs(roomPos - i) * Weights[state.Hall[i]];
        }

        for (var i = 0; i < state.Rooms.Length; i++)
        {
            for (var j = 0; j < state.Rooms[i].Length; j++)
            {
                var character = state.Rooms[i][j];
                if (character == '.') continue;
                var roomPos = RoomMapChar[character];
                weight += (Math.Abs(roomPos - (i * 2 + 2)) + i + 1) * Weights[character];
            }
        }

        return weight;
    }

    static int BruteForce(State initialState)
    {
        var q = new PriorityQueue<(State State, int Weight), int>();
        var visited = new HashSet<State>();
        q.Enqueue((initialState, 0), 0);
        while (q.Count > 0)
        {
            var shouldBreak = false;
            var (state, weight) = q.Dequeue();
            if (!visited.Add(state)) continue;
            if (IsCompleted(state)) return weight;
            for (var i = 0; i < state.Hall.Length; i++)
            {
                if (state.Hall[i] == '.') continue;
                var targetPos = RoomMapChar[state.Hall[i]];
                if (!(CanMove(state, i, targetPos) && CanPut(state, targetPos))) continue;
                var (putState, nextWeight) = MoveAndPut(state, weight, i);
                q.Enqueue((putState, nextWeight), nextWeight);
                shouldBreak = true;
                break;
            }

            if (shouldBreak) continue;
            for (var i = 0; i < state.Rooms.Length; i++)
            {
                if (!CanTake(state, i)) continue;
                var (takeState, takeWeight) = TakeFromRoom(state, i);
                var hallPos = i * 2 + 2;
                if (CanMove(state, hallPos, RoomMapChar[takeState.Hall[hallPos]]) &&
                    CanPut(state, RoomMapChar[takeState.Hall[hallPos]]))
                {
                    var (putState, nextWeight) = MoveAndPut(takeState, weight + takeWeight, hallPos);
                    if (visited.Contains(putState)) continue;
                    q.Enqueue((putState, nextWeight), nextWeight);
                }
                else
                {
                    for (var j = 0; j < takeState.Hall.Length; j++)
                    {
                        if (RoomMapInt.ContainsKey(j) || !CanMove(takeState, hallPos, j)) continue;
                        var (moveState, moveWeight) = MoveInHall(takeState, hallPos, j);
                        if (visited.Contains(moveState)) continue;
                        q.Enqueue((moveState, weight + takeWeight + moveWeight),
                            weight + takeWeight + moveWeight);
                    }
                }
            }
        }

        return -1;
    }

    static void Main()
    {
        var lines = new List<string>();
        string line;
        while (!string.IsNullOrEmpty(line = Console.ReadLine()))
        {
            lines.Add(line);
        }

        var result = Solve(lines);
        Console.WriteLine(result);
    }
}

public class State(char[] hall, char[][] rooms)
{
    protected bool Equals(State other)
    {
        for (var i = 0; i < Hall.Length; i++)
        {
            if (Hall[i] != other.Hall[i]) return false;
        }

        for (var i = 0; i < Rooms.Length; i++)
        {
            for (var j = 0; j < Rooms[i].Length; j++)
            {
                if (Rooms[i][j] != other.Rooms[i][j]) return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((State)obj);
    }

    public static bool operator ==(State? left, State? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(State? left, State? right)
    {
        return !Equals(left, right);
    }

    public readonly char[] Hall = hall;
    public readonly char[][] Rooms = rooms;

    public void Deconstruct(out char[] hall, out char[][] rooms)
    {
        hall = Hall.ToArray();
        rooms = Rooms.Select(x => x.ToArray()).ToArray();
    }

    public void GetHall(out char[] hall)
    {
        hall = Hall.ToArray();
    }

    public override int GetHashCode()
    {
        var hash = Hall.Aggregate(0, HashCode.Combine);
        hash = Rooms.Aggregate(hash, (cur, room) => room.Aggregate(cur, HashCode.Combine));
        return hash;
    }
}