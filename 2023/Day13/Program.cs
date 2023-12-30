using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Numerics;
using System.Text.RegularExpressions;
using NumSharp;

namespace Day13
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
                data = await client.GetStringAsync("https://adventofcode.com/2023/day/13/input");
                File.WriteAllText("C:\\Source\\Aoc\\2023\\data13.txt", data);
            }
            else
            {
                Console.WriteLine("## RUNNING TEST DATA");
                data = testData.Substring(2).Replace("\r", "");
            }

            var records = data.Split("\n\n");
            var matrix = records.Select(x => x.Split("\n").Select(y => y.ToArray()
                .Select(z => z == '#' ? 1:0).ToArray()).Where(a => a.Length > 0).ToArray()).ToArray();

            var sum = 0;
            var ix = 0;

            foreach (var part in matrix)
            {
                var orig = new NDArray(part);

                // Column reflection
                var cr = FindColReflection(orig);
                sum += cr;

                if (cr == 0)
                {
                    // Row reflection
                    var rr = FindRowReflection(orig);
                    sum += rr*100;
                }

                ix++;
            }

            Console.WriteLine($"Part 1: {sum}");
            //Console.WriteLine($"Part 2: {sum2}");

            var sum2 = 0;
            foreach (var part in matrix)
            {
                var orig = new NDArray(part);

                // Column reflection
                var cr = FindColReflection(orig, 1);
                sum2 += cr;

                if (cr == 0)
                {
                    // Row reflection
                    var rr = FindRowReflection(orig, 1);
                    sum2 += rr * 100;
                }

                ix++;
            }

            Console.WriteLine($"Part 2: {sum2}");
        }

        static int FindColReflection(NDArray orig, int unmatched = 0)
        {
            for (var col = 1; col < orig.Shape[1]; col++)
            {
                var len = Math.Min(col, orig.Shape[1] - col);
                var first = orig[$":,{col - len}:{col}"];
                var second = orig[$":,{col}:{col + len}"];
                var secondFlippedCols = second[":,::-1"];
                var s = (first == secondFlippedCols);
                if (s.flatten().ToArray<bool>().Where(x => !x).Count() == unmatched)
                {
                    return col;
                }
            }
            return 0;
        }

        static int FindRowReflection(NDArray orig, int unmatched = 0)
        {
            for (var row = 1; row < orig.Shape[0]; row++)
            {
                var len = Math.Min(row, orig.Shape[0] - row);
                var first = orig[$"{row - len}:{row},:"];
                var second = orig[$"{row}:{row + len},:"];
                var secondFlippedRows = second["::-1"];
                var s = (first == secondFlippedRows);
                if (s.flatten().ToArray<bool>().Where(x => !x).Count() == unmatched)
                {
                    return row;
                }
            }

            return 0;
        }


        private static string testData2 =
@"";

        private static string testData =
@"
#.##..##.
..#.##.#.
##......#
##......#
..#.##.#.
..##..##.
#.#.##.#.

#...##..#
#....#..#
..##..###
#####.##.
#####.##.
..##..###
#....#..#";
    }
}