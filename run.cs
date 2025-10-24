using System;
using System.Collections.Generic;

class Program
{
    private static readonly Dictionary<char, int> RoomMap = new Dictionary<char, int>()
    {
        { 'A', 2 }, { 'B', 4 }, { 'C', 6 }, { 'D', 8 }
    };

    private static readonly Dictionary<char, int> Weights = new Dictionary<char, int>()
    {
        { 'A', 1 }, { 'B', 10 }, { 'C', 100 }, { 'D', 1000 }
    };

    static int Solve(List<string> lines)
    {
        var initialState = GetInitialState(lines);
        var z = TakeFromRoom(initialState, 0);
        return 0;
    }

    static State GetInitialState(List<string> lines)
    {
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
            hall[RoomMap[character]] = character;
            rooms[roomNum][i] = '.';
            return (new State(hall, rooms), weight);
        }

        throw new Exception("Can't take from empty room");
    }

    static (State NextState, int Weight) MoveInHall(State state, int initialPos, int targetPos)
    {
        state.Deconstruct(out var hall);
        if (hall[initialPos] == '.') throw new Exception("Can't move nothing");
        for (var i = initialPos + 1; i <= targetPos; i++)
        {
            if (hall[i] != '.') throw new Exception("Can't reach target position");
        }

        hall[targetPos] = hall[initialPos];
        hall[initialPos] = '.';

        return (new State(hall, state.Rooms), (targetPos - initialPos) * Weights[hall[targetPos]]);
    }

    static (State NextState, int Weight) PutInRoom(State state, int hallPosition)
    {
        var (hall, rooms) = state;
        var roomNum = (hallPosition - 2) / 2;
        var character = hall[hallPosition];
        if (character == '.') throw new Exception("Can't move nothing");
        if (RoomMap[character] != roomNum) throw new Exception("Letter and room types incompatible");
        if (rooms[roomNum].Any(x => x != character && x != '.')) throw new Exception("Can't enter the room now");
        var deepestPlace = 0;
        for (var i = 0; i < rooms[roomNum].Length; i++)
        {
            if (rooms[roomNum][i] == '.') deepestPlace = i;
            else break;
        }

        hall[hallPosition] = '.';
        rooms[roomNum][deepestPlace] = character;
        return (new State(hall, rooms), deepestPlace * Weights[character]);
    }

    static void Main()
    {
        var lines = new List<string>();
        string line;

        while (!string.IsNullOrEmpty(line = Console.ReadLine()))
        {
            lines.Add(line);
        }

        var result = Solve(lines
            .Skip(2)
            .Take(lines.Count - 3)
            .ToList());
        Console.WriteLine(result);
    }
}

public class State(char[] hall, char[][] rooms)
{
    public char[] Hall = hall;
    public char[][] Rooms = rooms;

    public void Deconstruct(out char[] hall, out char[][] rooms)
    {
        hall = Hall.ToArray();
        rooms = Rooms.Select(x => x.ToArray()).ToArray();
    }

    public void Deconstruct(out char[] hall)
    {
        hall = Hall.ToArray();
    }
}