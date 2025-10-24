using System;
using System.Collections.Generic;

class Program
{
    private static readonly Dictionary<char, int> RoomMap = new Dictionary<char, int>()
    {
        { 'A', 2 }, { 'B', 4 }, { 'C', 6 }, { 'D', 8 }
    };

    static int Solve(List<string> lines)
    {
        var initialState = GetInitialState(lines);
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
    public char[] Hall = hall.ToArray();
    public char[][] Rooms = rooms.ToArray();
}