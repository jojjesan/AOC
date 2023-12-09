using System.Collections;
using System.Data;
using System.Numerics;

namespace Day9
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
                data = await client.GetStringAsync("https://adventofcode.com/2023/day/9/input");
            }
            else
            {
                Console.WriteLine("## RUNNING TEST DATA");
                data = testData.Replace("\r", "");
            }

            // Parse indata
            var lines = data.Split("\n").Where(x => x != "").ToArray();

            var histData = lines.Select(x => new List<List<int>>() { x.Split(" ").Select(int.Parse).ToList() }).ToList();
            PrintMatrix(histData);


            Console.WriteLine("## Diffs");
            CalcDiffs(histData);
            PrintMatrix(histData);

            Console.WriteLine("## Predict right");
            DoRightPrediction(histData);
            PrintMatrix(histData);

            Console.WriteLine($"Part 1: {histData.Sum(x => x[0].Last())}");

            Console.WriteLine("## Predict left");
            DoLeftPrediction(histData);
            PrintMatrix(histData);

            Console.WriteLine($"Part 2: {histData.Sum(x => x[0].First())}");
        }

        static void DoRightPrediction(List<List<List<int>>> matrices)
        {
            foreach (var matrix in matrices)
            {
                matrix.Last().Add(0);
                for (var r = matrix.Count() - 2; r >= 0; r--)
                {
                    matrix[r].Add(matrix[r].Last() + matrix[r + 1].Last());
                }
            }
        }

        static void DoLeftPrediction(List<List<List<int>>> matrices)
        {
            foreach (var matrix in matrices)
            {
                matrix.Last().Insert(0, 0);
                for (var r = matrix.Count() - 2; r >= 0; r--)
                {
                    matrix[r].Insert(0, matrix[r].First() - matrix[r + 1].First());
                }
            }
        }

        static void CalcDiffs(List<List<List<int>>> matrices)
        {
            foreach (var matrix in matrices)
            {
                var ix = 0;
                while (true)
                {
                    var row = new List<int>();
                    matrix.Add(row);
                    var onlyZeroes = true;
                    for (var i = 0; i < matrix[ix].Count() - 1; i++)
                    {
                        var diff = matrix[ix][i + 1] - matrix[ix][i];
                        row.Add(diff);
                        if (diff != 0) onlyZeroes = false;
                    }

                    if (onlyZeroes || row.Count() == 1) break;
                    ix++;
                }
            }
        }

        static void PrintMatrix(List<List<List<int>>> matrices)
        {
            var num = 0;
            foreach (var matrix in matrices)
            {
                Console.WriteLine($"## Matrix {num++}");
                foreach (var row in matrix)
                    Console.WriteLine(string.Join(", ", row));
            }
        }

        private static string testData = @"0 3 6 9 12 15
1 3 6 10 15 21
10 13 16 21 30 45";
    }
}