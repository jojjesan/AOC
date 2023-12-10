using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Security.Cryptography;
using System.Xml;

namespace Day10
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            bool test = false;
            string data = "";
            if (!test)
            {
                string sessionKey = File.ReadAllText("C:\\Source\\Aoc\\sessionkey.txt");
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Cookie", $"session={sessionKey}");
                data = await client.GetStringAsync("https://adventofcode.com/2023/day/10/input");
            }
            else
            {
                Console.WriteLine("## RUNNING TEST DATA");
                data = testData2.Replace("\r", "");
            }

            // Parse indata
            var lines = data.Split("\n").Where(x => x != "").ToArray();
            var connPoints = lines.Select(x => x.ToCharArray().Select(GetConnectingPoints).ToList()).ToList();

            var start = new Point() { Row = 0, Col = 0 };
            var ix = 0;
            foreach (var connPointRow in connPoints)
            {
                //WriteColored(string.Join("", connPointRow), "XX");
                //Console.WriteLine();
                if (connPointRow.Contains("NSEW")) {
                    start.Row = ix;
                    start.Col = connPointRow.IndexOf("NSEW");
                };
                ix++;
            }

            //Part1(start, connPoints);

            // Part 2
            Part2(start, connPoints);

        }

        static void Part2(Point start, List<List<string>> connPoints)
        {
            var forward = GetNextPoint(start, start, connPoints);
            var steps = 1;

            Console.WriteLine($"Start: {start}");
            Console.WriteLine($"Forward: {forward}");

            var map = new Dictionary<Point, char>();
            var prevForward = start;
            while (!forward.Equals(start))
            {
                map[forward] = '#';
                var savedForward = forward;
                forward = GetNextPoint(forward, prevForward, connPoints);
                prevForward = savedForward;
                //Console.WriteLine($"Curr forward: {forward}");
                steps++;
            }
            map[start] = '#';

            PrintMap(map, connPoints.Count(), connPoints[0].Count());
            Console.WriteLine();

            var max = new Point() { Row = connPoints.Count(), Col = connPoints[0].Count() };
            forward = start;
            prevForward = forward;
            forward = GetNextPoint(forward, prevForward, connPoints);
            MarkLeftAndRight(map, prevForward, forward, max);
            while (!forward.Equals(start))
            {
                var savedForward = forward;
                forward = GetNextPoint(forward, prevForward, connPoints);
                prevForward = savedForward;
                MarkLeftAndRight(map, prevForward, forward, max);
                //PrintMap(map, max.Row, max.Col);
                //Console.WriteLine();
            }


            PrintMap(map, max.Row, max.Col);

            //Console.WriteLine($"Part 2: {map[new Point() { Row=0, Col=0}]}");
            Console.WriteLine($"Part 2 L: {map.Values.Where(x => x == 'L').Count()}");
            Console.WriteLine($"Part 2 R: {map.Values.Where(x => x == 'R').Count()}");
            Console.WriteLine($"Part 2 #: {map.Values.Where(x => x == '#').Count()}");
            Console.WriteLine($"Part 2 #: {map.Values.Count()}");
            Console.WriteLine($"Part 2 #: {max.Row * max.Row}");
        }

        static void MarkLeftAndRight(Dictionary<Point, char> map, Point prev, Point curr, Point max)
        {
            var diff = curr.Subtract(prev);
            switch (diff.Row, diff.Col)
            {
                case (1, 0):
                    MarkArea(map, new Point { Row = curr.Row, Col = curr.Col + 1 }, 'L', max);
                    MarkArea(map, new Point { Row = curr.Row, Col = curr.Col - 1 }, 'R', max);
                    break;
                case (-1, 0):
                    MarkArea(map, new Point { Row = curr.Row, Col = curr.Col + 1 }, 'R', max);
                    MarkArea(map, new Point { Row = curr.Row, Col = curr.Col - 1 }, 'L', max);
                    break;
                case (0, 1):
                    MarkArea(map, new Point { Row = curr.Row - 1, Col = curr.Col }, 'L', max);
                    MarkArea(map, new Point { Row = curr.Row + 1, Col = curr.Col }, 'R', max);
                    break;
                case (0, -1):
                    MarkArea(map, new Point { Row = curr.Row - 1, Col = curr.Col }, 'R', max);
                    MarkArea(map, new Point { Row = curr.Row + 1, Col = curr.Col }, 'L', max);
                    break;
                default:
                    throw new Exception("Unknown direction");

            }

            var points = new List<Point> { new Point(0, 1), new Point(1, 1), new Point(1, 0), new Point(1, -1), 
                new Point(0, -1), new Point(-1, -1), new Point(-1, 0), new Point(-1, 1)};
            points.AddRange(points);
            var currCh = '#';
            foreach (var d in points)
            {
                if (map.TryGetValue(curr.Add(d), out char ch))
                {
                    currCh = ch;
                }
                else if (currCh != '#')
                {
                    MarkArea(map, curr.Add(d), currCh, max);
                }
            }

            points.Reverse();
            foreach (var d in points)
            {
                if (map.TryGetValue(curr.Add(d), out char ch))
                {
                    currCh = ch;
                }
                else if (currCh != '#')
                {
                    map[curr.Add(d)] = currCh;
                }
            }

        }

        static void MarkArea(Dictionary<Point, char> map, Point curr, char ch, Point max)
        {
            if (curr.Row < 0 || curr.Row >= max.Row || curr.Col < 0 || curr.Col >= max.Col)
                return;
            if (map.ContainsKey(curr) && map[curr] == '#')
                return;
            var points = new Point[] { 
                new Point() { Row = curr.Row + 1, Col = curr.Col },
                new Point() { Row = curr.Row - 1, Col = curr.Col },
                new Point() { Row = curr.Row, Col = curr.Col + 1 },
                new Point() { Row = curr.Row, Col = curr.Col - 1}
            };
            if (!map.ContainsKey(curr))
            {
                map[curr] = ch;
            }

            foreach (var p in points)
            {
                if (!map.ContainsKey(p))
                    MarkArea(map, p, ch, max);
            }
        }

        static void PrintMap(Dictionary<Point, char> map, int rowCount, int colCount)
        {
            for (var r = 0; r < rowCount; r++)
            {
                for (var c = 0; c < colCount; c++)
                {
                    var p = new Point() { Row = r, Col = c };
                    if (map.ContainsKey(p))
                    {
                        Console.Write(map[p]);
                    }
                    else
                    {
                        Console.Write(".");
                    }
                }
                Console.WriteLine();
            }
        }

        static void Part1(Point start, List<List<string>> connPoints)
        {
            var forward = GetNextPoint(start, start, connPoints);
            var reverse = GetNextPoint(start, start, connPoints, true);
            var steps = 1;

            Console.WriteLine($"Start: {start}");
            Console.WriteLine($"Forward: {forward}");
            Console.WriteLine($"Reverse: {reverse}");

            var prevForward = start;
            var prevReverse = start;
            while (!forward.Equals(reverse))
            {
                var savedForward = forward;
                var savedReverse = reverse;
                forward = GetNextPoint(forward, prevForward, connPoints);
                reverse = GetNextPoint(reverse, prevReverse, connPoints);
                prevForward = savedForward;
                prevReverse = savedReverse;
                Console.WriteLine($"Curr forward: {forward}");
                Console.WriteLine($"Curr reverse: {reverse}");
                steps++;
            }

            Console.WriteLine($"Part 1: {steps}");
        }

        static Point GetNextPoint(Point current, Point previous, List<List<string>> connPoints, bool reverse = false)
        {
            var directions = new Point[] {
                new Point() { Row = 0, Col = 1},
                new Point() { Row = 0, Col = -1},
                new Point() { Row = 1, Col = 0},
                new Point() { Row = -1, Col = 0}
            };
            if (reverse)
            {
                directions = directions.Reverse().ToArray();
            }

            foreach (var direction in directions)
            {
                if (current.Add(direction).Equals(previous)) continue;
                if (IsConnected(current, current.Add(direction), connPoints)) 
                    return current.Add(direction);
            }

            throw new Exception("No next point found");
        }

        static bool IsConnected(Point from, Point to, List<List<string>> connPoints)
        {
            if (to.Row < 0 || to.Row >= connPoints.Count()) return false;
            if (to.Col < 0 || to.Col >= connPoints[to.Row].Count()) return false;

            var fromConn = connPoints[from.Row][from.Col];
            var toConn = connPoints[to.Row][to.Col];
            var direction = GetDirection(to.Subtract(from));

            return fromConn.Contains(direction[0]) && toConn.Contains(direction[1]);
        }

        static string GetDirection(Point direction)
        {
            switch (direction.Row, direction.Col)
            {
                case (0, 1):
                    return "EW";
                case (0, -1):
                    return "WE";
                case (1, 0):
                    return "SN";
                case (-1, 0):
                    return "NS";
                default:
                    throw new Exception("Unknown direction");
            }
        }

        static string GetConnectingPoints(char input)
        {
            switch (input)
            {
                case '|':
                    return "NS";
                case '-':
                    return "EW";
                case 'L':
                    return "NE";
                case 'J':
                    return "NW";
                case '7':
                    return "SW";
                case 'F':
                    return "SE";
                case 'S':
                    return "NSEW";
                case '.':
                    return "..";
                default:
                    throw new Exception("Unknown input");
            }

        }

        static void WriteColored(string text, string coloredWord, ConsoleColor color = ConsoleColor.Green)
        {
            string[] normalParts = text.Split(new string[] { coloredWord }, StringSplitOptions.None);
            for (int i = 0; i < normalParts.Length; i++)
            {
                Console.ResetColor();
                Console.Write(normalParts[i]);
                if (i != normalParts.Length - 1)
                {
                    Console.BackgroundColor = color;
                    Console.Write(coloredWord);
                }
            }
        }

        struct Point
        {
            public int Row;
            public int Col;
            public Point(int r, int c)
            {
                Row = r;
                Col = c;
            }
            public Point Add(Point other)
            {
                return new Point() { Row = Row + other.Row, Col = Col + other.Col };
            }
            public Point Subtract(Point other)
            {
                return new Point() { Row = Row - other.Row, Col = Col - other.Col };
            }
            public bool Equals(Point other)
            {
                return Row == other.Row && Col == other.Col;
            }
            public override string ToString()
            {
                return $"({Row}, {Col})";
            }
        }

        private static string testData2 =
@"...........
.S-------7.
.|F-----7|.
.||.....||.
.||.....||.
.|L-7.F-J|.
.|..|.|..|.
.L--J.L--J.
...........";

        private static string testData = @"7-F7-
.FJ|7
SJLL7
|F--J
LJ.LJ";
    }
}