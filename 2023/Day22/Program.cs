using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Day22
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
                var day = 22;
                data = await client.GetStringAsync($"https://adventofcode.com/2023/day/{day}/input");
                File.WriteAllText($"C:\\Source\\Aoc\\2023\\data{day}.txt", data);
            }
            else
            {
                Console.WriteLine("## RUNNING TEST DATA");
                data = testData.Substring(2).Replace("\r", "");
            }

            var bricks = data.Split("\n").Where(x => x.Length > 0)
                .Select(x => x.Split("~").Select(y => y.Split(",").Select(z => int.Parse(z)).ToArray()).ToArray())
                .Select((x, i) => new Heading(i,
                    new Vector(x[1][0]- x[0][0], x[1][1]- x[0][1], x[1][2]- x[0][2]), 
                    new Vector(x[0][0], x[0][1], x[0][2])))
                .ToDictionary(k => k.Id, v => v);

            Part1(bricks);

            Part2(bricks);
        }

        static void Part1(Dictionary<int, Heading> bricks)
        {
            DoFall(bricks);
            Console.WriteLine($"Part1: {bricks.Values.Where(x => !x.Supports.Any(y => bricks[y].SupportedBy.Count() == 1)).Count()}");
        }

        static void Part2(Dictionary<int, Heading> bricks)
        {
            var sum = 0;
            var copy = Copy(bricks);
            foreach (var brick in copy.Values)
            {
                copy.Remove(brick.Id);
                var falls = DoFall(copy);
                sum += falls;
                //Console.WriteLine($"Removing {brick.Id}: {falls}");
                copy = Copy(bricks);
            }
            Console.WriteLine($"Part2: {sum}");
        }

        static Dictionary<int, Heading> Copy(Dictionary<int, Heading> bricks)
        {
            var copy = bricks.ToDictionary(k => k.Key, v => new Heading(v.Value.Id, new Vector(v.Value.Direction), new Vector(v.Value.Position)));
            return copy;
        }

        static int DoFall(Dictionary<int, Heading> bricks)
        {
            var zSorted = bricks.Values.OrderBy(x => x.Position.Z).ToArray();
            var occupied = new Dictionary<string, int>();
            var falling = new Dictionary<int, bool>();
            foreach (var v in zSorted)
            {
                // Move until stop
                while (v.Position.Z > 1)
                {
                    var collisions = v.GetPath().Where(x => occupied.ContainsKey(GetKey(x.X, x.Y, x.Z - 1)))
                        .Select(x => occupied[GetKey(x.X, x.Y, x.Z - 1)]).Distinct().ToList();
                    if (collisions.Any())
                    {
                        //Console.WriteLine($"{v.Id} collides at {v.Position} with {string.Join(",", collisions)}");
                        v.SupportedBy.AddRange(collisions);
                        collisions.ForEach(x => bricks[x].Supports.Add(v.Id));
                        break;
                    }
                    v.Position.Z--;
                    falling[v.Id] = true;
                }

                // Mark as occupied
                v.GetPath().ForEach(x => occupied[x.ToString()] = v.Id);
            }

            return falling.Count();
        }

        static string GetKey(long x, long y, long z)
        {
            return $"{x},{y},{z}";
        }


        static void PrintGrid(Dictionary<string, int> grid, Vector min, Vector max)
        {
            for (var row = min.X; row <= max.X; row++)
            {
                for (var col = min.Y; col <= max.Y; col++)
                {
                    var tmp = new Vector(row, col, 0);
                    if (grid.ContainsKey(tmp.ToString()) && grid[tmp.ToString()] == -1)
                    {
                        Console.Write(".");
                    }
                    else
                    {
                        Console.Write("#");
                    }
                }
                Console.WriteLine();
            }
        }

        public class Heading
        {
            public Heading(int id, Vector direction, Vector pos)
            {
                Direction = direction;
                Position = pos;
                Id = id;
            }

            public int Id { get; set; }
            public Vector Direction { get; set; }
            public Vector Position { get; set; }
            public List<int> SupportedBy { get; set; } = new List<int>();
            public List<int> Supports { get; set; } = new List<int>();
            public List<Vector> GetPath()
            {
                var res = new List<Vector>();
                var p = new Vector(Position);
                var end = new Vector(Position).Add(Direction);
                var d = new Vector(Direction.X > 0 ? 1 : 0, Direction.Y > 0 ? 1 : 0, Direction.Z > 0 ? 1 : 0);
                res.Add(p);
                while (!p.Equals(end))
                {
                    p = new Vector(p).Add(d);
                    res.Add(p);
                }
                return res;
            }
        }

        public class Vector
        {
            public long X { get; set; }
            public long Y { get; set; }
            public long Z { get; set; }

            public Vector(long x, long y, long z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public Vector(Vector v)
            {
                X = v.X;
                Y = v.Y;
                Z = v.Z;
            }

            public long GetManhLen()
            {
                return Math.Abs(X) + Math.Abs(Y) + Math.Abs(Z);
            }

            public Vector Sub(Vector v)
            {
                X -= v.X;
                Y -= v.Y;
                Z -= v.Z;
                return this;
            }

            public Vector Add(Vector v)
            {
                X += v.X;
                Y += v.Y;
                Z += v.Z;
                return this;
            }

            public Vector Mul(int mul)
            {
                X *= mul;
                Y *= mul;
                Z *= mul;
                return this;
            }

            public override string ToString()
            {
                return $"{X},{Y},{Z}";
            }

            public bool Equals(Vector v)
            {
                return v.X == X && v.Y == Y && v.Z == Z;
            }
        }

        private static string testData2 =
@"";

        private static string testData =
@"
1,0,1~1,2,1
0,0,2~2,0,2
0,2,3~2,2,3
0,0,4~0,2,4
2,0,5~2,2,5
0,1,6~2,1,6
1,1,8~1,1,9
";
    }
}