using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Dynamic;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Xml;

namespace Day17
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
                var day = 17;
                data = await client.GetStringAsync($"https://adventofcode.com/2023/day/{day}/input");
                File.WriteAllText($"C:\\Source\\Aoc\\2023\\data{day}.txt", data);
            }
            else
            {
                Console.WriteLine("## RUNNING TEST DATA");
                data = testData.Substring(2).Replace("\r", "");
            }

            var grid = data.Split("\n").Select(x => x.ToArray().Select(y => int.Parse(y.ToString())).ToArray())
                .Where(x => x.Length > 0).ToArray();
            var sum = 0;

            var start = new Heading(new Vector(0, 0), new Vector(0, 0), 0);
            var costStates = new Dictionary<string, Tuple<int, string>>();
            costStates[GetKey(start)] = Tuple.Create(0, $"{start.Position.ToString()}:{0}");
            var queue = new Queue<Heading>();
            queue.Enqueue(start);
            var ix = 0;
            var maxRowCol = 0;
            var maxCost = 0;
            var curr = new Vector(0, 0);
            var steps = 0;
            while (true)
            {
                if ((steps/4) % 2 == 0 && grid.Length - curr.Row >= 4) 
                    curr.Add(Directions["E"]);
                else 
                    curr.Add(Directions["S"]);
                steps++;
                if (curr.IsWithinBounds(grid))
                    maxCost += grid[curr.Row][curr.Col];
                else
                    break;
            }

            Console.WriteLine($"MaxCost: {maxCost}");

            while (queue.Any())
            {
                var currPosition = queue.Dequeue();
                maxRowCol = Math.Max(maxRowCol, currPosition.Position.Col + currPosition.Position.Row);
                //Console.WriteLine($"Curr: {currPosition.Position.ToString()}");
                var newPositions = FindNextPositions2(currPosition, grid, costStates, maxCost);
                newPositions.ForEach(x => queue.Enqueue(x));
                //Console.WriteLine($"Stack costs: {string.Join(", ", queue.Select(x => GetSortCost(x, costStates).ToString()))}");
                if (ix % 1000 == 0) 
                {
                    //Console.WriteLine($"Stack costs: {string.Join(", ", stack.Select(x => GetSortCost(x, costStates).ToString()))}");
                    var newQueue = new Queue<Heading>(queue.OrderBy(x => GetSortCost(x, costStates)));
                    queue = newQueue;
                    //Console.WriteLine($"Stack costs: {string.Join(", ", stack.Select(x => GetSortCost(x, costStates).ToString()))}");
                    Console.Write($"\rStack: {costStates.Count} ({maxRowCol})  ");
                }
                ix++;
            }
            Console.WriteLine();
            //PrintGrid(grid, energized);

            var minState = costStates.Where(x => x.Key.StartsWith($"{new Vector(grid.Length - 1,grid[0].Length - 1).ToString()}|"))
                .OrderBy(x => x.Value.Item1).First();
            Console.WriteLine($"Part 1: {minState.Value.Item1}\n {minState.Value.Item2}");

        }

        static int GetSortCost(Heading heading, Dictionary<string, Tuple<int, string>> costStates)
        {
            var costState = GetCostState(heading, costStates);
            return costState.Item1 + 3 * (heading.Position.GetManhLen());
        }

        static int GetMin(int[][] grid, Dictionary<string, Tuple<int, string>> costStates)
        {
            return (costStates.Where(x => x.Key.StartsWith($"{new Vector(grid.Length - 1, grid[0].Length - 1).ToString()}|"))
                .OrderBy(x => x.Value.Item1).FirstOrDefault().Value ?? Tuple.Create(-1, "")).Item1;
        }

        static List<Heading> FindNextPositions2(Heading currHeading, int[][] grid, Dictionary<string, Tuple<int, string>> costStates, int maxCost)
        {
            var currCostState = GetCostState(currHeading, costStates);
            var newHeadings = new List<Heading>();

            foreach (var dir in Directions.Values)
            {
                var direction = new Vector(dir);
                var steps = 1;
                if (direction.Row == -currHeading.Direction.Row &&
                    direction.Col == -currHeading.Direction.Col) continue;

                if (!currHeading.Direction.Equals(direction))
                   steps = 4;

                var numStraight = currHeading.Direction.Equals(direction) ? currHeading.NumStraight + 1 : steps;
                if (numStraight > 10) continue;

                var nextCost = currCostState.Item1;
                var nextPos = new Vector(currHeading.Position);
                for (var i = 0; i < steps; i++) 
                { 
                    nextPos.Add(direction);
                    if (!nextPos.IsWithinBounds(grid)) break;
                    nextCost += grid[nextPos.Row][nextPos.Col];
                }
                if (!nextPos.IsWithinBounds(grid)) continue;

                var nextHeading = new Heading(nextPos, direction, numStraight);
                var existingNextCostState = GetCostState(nextHeading, costStates);

                if (nextCost < existingNextCostState.Item1)
                {
                    if (nextCost <= maxCost) newHeadings.Add(nextHeading);
                    costStates[GetKey(nextHeading)] =
                        Tuple.Create(nextCost, "" /*currCostState.Item2 + $"|{nextHeading.Position.ToString()}:{nextCost}"*/);
                }
            }

            return newHeadings.ToList();
        }

        static List<Heading> FindNextPositions(Heading currHeading, int[][] grid, Dictionary<string, Tuple<int, string>> costStates, int maxCost)
        {
            var currCostState = GetCostState(currHeading, costStates);
            var newHeadings = new List<Heading>();

            foreach (var direction in Directions.Values)
            {
                if (direction.Row == -currHeading.Direction.Row &&
                    direction.Col == -currHeading.Direction.Col) continue;

                var numStraight = currHeading.Direction.Equals(direction) ? currHeading.NumStraight + 1 : 1;
                if (numStraight > 3) continue;

                var nextPos = new Vector(currHeading.Position).Add(direction);
                if (!nextPos.IsWithinBounds(grid)) continue;

                var nextHeading = new Heading(nextPos, direction, numStraight);
                var existingNextCostState = GetCostState(nextHeading, costStates);
                var nextCost = currCostState.Item1 + grid[nextHeading.Position.Row][nextHeading.Position.Col];
                if (nextCost < existingNextCostState.Item1)
                {
                    if (nextCost <= maxCost) newHeadings.Add(nextHeading);
                    costStates[GetKey(nextHeading)] =
                        Tuple.Create(nextCost, ""/*currCostState.Item2 + $"|{nextHeading.Position.ToString()}:{nextHeading.NumStraight}"*/);
                }
            }

            return newHeadings.OrderByDescending(x => GetCostState(x, costStates).Item1).ToList();
        }

        static Tuple<int, string> GetCostState(Heading heading, Dictionary<string, Tuple<int, string>> costStates)
        {
            var key = GetKey(heading);
            return costStates.ContainsKey(key) ? costStates[key] : Tuple.Create(int.MaxValue, "");
        }

        static string GetKey(Heading heading)
        {
            return $"{heading.Position.ToString()}|{heading.Direction.ToString()}|{heading.NumStraight}";
        }

        static void IncreaseDict(Dictionary<string, int> dict, Vector key, int value)
        {
            if (dict.ContainsKey(key.ToString()))
            {
                dict[key.ToString()] += value;
            }
            else
            {
                dict[key.ToString()] = value;
            }
        }

        static void PrintGrid(int[][] grid, Vector curr)
        {
            if (!curr.IsWithinBounds(grid)) return;
            Console.WriteLine($"Curr: {grid[curr.Row][curr.Col]}");
            for (var row = 0; row < grid.Length; row++)
            {
                for (var col = 0; col < grid[0].Length; col++)
                {
                    var tmp = new Vector(row, col);
                    if (curr.Equals(tmp))
                    {
                        Console.Write("*");
                    }
                    else
                    {
                        Console.Write(grid[row][col]);
                    }
                }
                Console.WriteLine();
            }
        }

        static void PrintGrid(char[][] grid, Dictionary<string, int> states)
        {
            for (var row = 0; row < grid.Length; row++)
            {
                for (var col = 0; col < grid[0].Length; col++)
                {
                    var key = new Vector(row, col).ToString();
                    if (states.ContainsKey(key))
                    {
                        Console.Write(Math.Min(states[key], 9));
                    }
                    else
                    {
                        Console.Write(grid[row][col]);
                    }
                }
                Console.WriteLine();
            }
        }

        static Dictionary<string, Vector> Directions = new Dictionary<string, Vector>()
        {
            {"N", new Vector(-1, 0)},
            {"S", new Vector(1, 0)},
            { "E", new Vector(0, 1)},
            { "W", new Vector(0, -1)}
        };

        public class Heading
        {
            public Heading(Vector position, Vector direction, int numStraight)
            {
                Position = position;
                Direction = direction;
                NumStraight = numStraight;
            }

            public Vector Position { get; set; }
            public Vector Direction { get; set; }
            public int NumStraight { get; set; }
        }

        public class Vector
        {
            public int Row { get; set; }
            public int Col { get; set; }

            public Vector(int rows, int cols)
            {
                Row = rows;
                Col = cols;
            }

            public Vector(Vector v)
            {
                Row = v.Row;
                Col = v.Col;
            }

            public int GetManhLen()
            {
                return Math.Abs(Row) + Math.Abs(Col);
            }

            public Vector Add(Vector v)
            {
                Row += v.Row;
                Col += v.Col;
                return this;
            }

            public Vector Mul(int mul)
            {
                Row *= mul;
                Col *= mul;
                return this;
            }

            public bool IsWithinBounds(int[][] grid)
            {
                return Row >= 0 && Col >= 0 && Row < grid.Length && Col < grid[0].Length;
            }

            public override string ToString()
            {
                return $"({Row},{Col})";
            }

            public bool Equals(Vector v)
            {
                return v.Row == Row && v.Col == Col;
            }
        }

        private static string testData2 =
@"";

        private static string testData =
@"
2413432311323
3215453535623
3255245654254
3446585845452
4546657867536
1438598798454
4457876987766
3637877979653
4654967986887
4564679986453
1224686865563
2546548887735
4322674655533";
    }
}