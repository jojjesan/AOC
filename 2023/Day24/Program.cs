using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Day24
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            bool test = false;
            string data = "";
            var testAreaMin = new Vector(7, 7, 0);
            var testAreaMax = new Vector(27, 27, 0);

            if (!test)
            {
                testAreaMin = new Vector(200000000000000, 200000000000000, 0);
                testAreaMax = new Vector(400000000000000, 400000000000000, 0);
                string sessionKey = File.ReadAllText("C:\\Source\\Aoc\\sessionkey.txt");
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Cookie", $"session={sessionKey}");
                var day = 24;
                data = await client.GetStringAsync($"https://adventofcode.com/2023/day/{day}/input");
                File.WriteAllText($"C:\\Source\\Aoc\\2023\\data{day}.txt", data);
            }
            else
            {
                Console.WriteLine("## RUNNING TEST DATA");
                data = testData.Substring(2).Replace("\r", "");
            }
            data = data.Substring(0, data.Length-1);

            var hailstones = data.Split("\n").Select(x => x.Split(" @ ").Select(y => y.Split(", ").Select(z => long.Parse(z)).ToArray()).ToArray())
                .Select(x => new Heading(new Vector(x[1][0], x[1][1], x[1][2]), new Vector(x[0][0], x[0][1], x[0][2]))).ToList();

            //Part1(hailstones, testAreaMin, testAreaMax);

            Part2(hailstones, testAreaMin, testAreaMax);
        }

        static void Part1(List<Heading> hailstones, Vector testAreaMin, Vector testAreaMax)
        {
            var sum = 0;
            for (var i = 0; i < hailstones.Count(); i++)
            {
                var hailstoneA = hailstones[i];
                for (var j = i + 1; j < hailstones.Count(); j++)
                {
                    var hailstoneB = hailstones[j];
                    if (hailstoneA == hailstoneB)
                        continue;
                    var intersection = FindIntersection(hailstoneA.Position, hailstoneA.Direction,
                        hailstoneB.Position, hailstoneB.Direction, (0, 0));
                    //var intersectStr = intersection == null ? "||" : $"{intersection.Value.Item1};{intersection.Value.Item2}";
                    //if (intersection != null)
                    //    intersectStr += $" {hailstoneA.IsInPath(intersection.Value)} {hailstoneB.IsInPath(intersection.Value)}";
                    //if (InTestArea(intersection, testAreaMin, testAreaMax)) intersectStr += " *";
                    //Console.WriteLine($"{hailstoneA.Position} {hailstoneA.Direction} : {hailstoneB.Position} {hailstoneB.Direction} => {intersectStr}");
                    if (intersection != null && InTestArea((intersection.Value.Item1, intersection.Value.Item2), testAreaMin, testAreaMax))
                    {
                        if (hailstoneA.IsInPath(intersection.Value) && hailstoneB.IsInPath(intersection.Value))
                            sum++;
                    }
                }
            }

            Console.WriteLine($"Part1: {sum}");
        }

        static void Part2(List<Heading> hailstones, Vector testAreaMin, Vector testAreaMax)
        {
            long result2 = 0;
            for (var dx = -300; dx < 300; dx++)
            {
                for (var dy = -300; dy < 300; dy++)
                {
                    var intersection1 = FindIntersection(hailstones[1].Position, hailstones[1].Direction,
                        hailstones[0].Position, hailstones[0].Direction, (dx, dy));
                    var intersection2 = FindIntersection(hailstones[2].Position, hailstones[2].Direction,
                        hailstones[0].Position, hailstones[0].Direction, (dx, dy));
                    var intersection3 = FindIntersection(hailstones[3].Position, hailstones[3].Direction,
                        hailstones[0].Position, hailstones[0].Direction, (dx, dy));

                    if (intersection1 != null && intersection2 != null && intersection3 != null &&
                        intersection1.Value.Item1 == intersection2.Value.Item1 && intersection1.Value.Item1 == intersection3.Value.Item1 &&
                        intersection1.Value.Item2 == intersection2.Value.Item2 && intersection1.Value.Item2 == intersection3.Value.Item2)
                    {
                        for (var dz = -300; dz < 300; dz++)
                        {
                            var z1 = intersection1.Value.Item3 * (hailstones[1].Direction.Z - dz) + hailstones[1].Position.Z;
                            var z2 = intersection2.Value.Item3 * (hailstones[2].Direction.Z - dz) + hailstones[2].Position.Z;
                            var z3 = intersection3.Value.Item3 * (hailstones[3].Direction.Z - dz) + hailstones[3].Position.Z;
                            if (Math.Abs(z1 - z2) < 0.1 && Math.Abs(z2 - z3) < 0.1)
                            {
                                Console.WriteLine($"Found: {intersection1.Value.Item1};{intersection1.Value.Item2};{z1}");
                                result2 = (long)(intersection1.Value.Item1 + intersection1.Value.Item2 + z1);
                            }
                        }
                    }

                    //var intersectStr = intersection == null ? "||" : $"{intersection.Value.Item1};{intersection.Value.Item2}";
                    //if (intersection != null)
                    //    intersectStr += $" {hailstoneA.IsInPath(intersection.Value)} {hailstoneB.IsInPath(intersection.Value)}";
                    //if (InTestArea(intersection, testAreaMin, testAreaMax)) intersectStr += " *";
                    //Console.WriteLine($"{hailstoneA.Position} {hailstoneA.Direction} : {hailstoneB.Position} {hailstoneB.Direction} => {intersectStr}");
                }
            }

            Console.WriteLine($"Part2: {result2}");
            //-2147483648 (int truncated)
            // 618534564836937
        }

        static (double, double, double)? FindIntersection(Vector s1, Vector d1, Vector s2, Vector d2, (int, int) offset)
        {
            var (ox, oy) = offset;
            long a1 = d1.Y - oy;
            long b1 = d1.X - ox;

            long a2 = d2.Y - oy;
            long b2 = d2.X - ox;

            long delta = a1 * b2 - a2 * b1;
            if (delta == 0) return null;

            var t1a = (s1.X - s2.X) * a2 + (s2.Y - s1.Y) * b2;
            var t1 = (double)t1a / delta;
            //If lines are parallel, the result will be null.
            return (s1.X + b1 * t1, s1.Y + a1 * t1, t1);
        }

        static bool InTestArea((double, double)? intersection, Vector min, Vector max)
        {
            if (intersection == null) return false;
            var intersect = intersection.Value;
            return intersect.Item1 >= min.X && intersect.Item1 <= max.X &&
                   intersect.Item2 >= min.Y && intersect.Item2 <= max.Y;
        }

        public class Heading
        {
            public Heading(Vector direction, Vector pos)
            {
                Direction = direction;
                Position = pos;
            }

            public Vector Direction { get; set; }
            public Vector Position { get; set; }
            public bool IsInPath((double, double) point)
            {
                var (x, y) = point;
                var z = 0;
                if (Direction.X <= 0 && x > Position.X) return false;
                if (Direction.X >= 0 && x < Position.X) return false;
                if (Direction.Y <= 0 && y > Position.Y) return false;
                if (Direction.Y >= 0 && y < Position.Y) return false;
                if (Direction.Z <= 0 && z > Position.Z) return false;
                if (Direction.Z >= 0 && z < Position.Z) return false;
                return true;
            }

            public bool IsInPath((double, double, double) point)
            {
                var (x, y, z) = point;
                if (Direction.X <= 0 && x > Position.X) return false;
                if (Direction.X >= 0 && x < Position.X) return false;
                if (Direction.Y <= 0 && y > Position.Y) return false;
                if (Direction.Y >= 0 && y < Position.Y) return false;
                if (Direction.Z <= 0 && z > Position.Z) return false;
                if (Direction.Z >= 0 && z < Position.Z) return false;
                return true;
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

            public bool IsWithinBounds(Vector min, Vector max)
            {
                return X >= min.X && Y >= min.Y && Z >= min.Z && X <= max.X && Y <= max.Y && Z <= max.Z;
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
19, 13, 30 @ -2,  1, -2
18, 19, 22 @ -1, -1, -2
20, 25, 34 @ -2, -2, -4
12, 31, 28 @ -1, -2, -1
20, 19, 15 @  1, -5, -3
";
    }
}