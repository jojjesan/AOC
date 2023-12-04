using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Day3
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            bool test = false;
            string data = "";
            string sessionKey = File.ReadAllText("C:\\Source\\Aoc\\sessionkey.txt");
            if (!test)
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Cookie", $"session={sessionKey}");
                data = await client.GetStringAsync("https://adventofcode.com/2023/day/3/input");
            }
            else
            {
                Console.WriteLine("## RUNNING TEST DATA");
                data = testData.Replace("\r", "");
            }

            var lines = data.Split("\n").Where(x => x != "").ToArray();
            Part1(lines);
            Part2(lines);

            //Console.WriteLine(string.Join("\n", lines));
        }

        static void Part1(string[] lines)
        {
            var sum = 0;
            for (var row = 0; row < lines.Length; row++)
            {
                var line = lines[row];
                var col = 0;
                while (col < line.Length)
                {
                    var c = line[col];
                    if (Char.IsDigit(c))
                    {
                        var digits = GetDigits(line, col);
                        if (GearsNearPartNbr(lines, digits, row, col).Any())
                        {
                            sum += int.Parse(digits);
                        }
                        col += digits.Length;
                    }
                    else
                    {
                        col++;
                    }
                }
            }

            Console.WriteLine($"Part 1: {sum}");
        }

        static void Part2(string[] lines)
        {
            var sum = 0;
            var partGears = new Dictionary<string, List<GearInfo>> ();
            for (var row = 0; row < lines.Length; row++)
            {
                var line = lines[row];
                var col = 0;
                while (col < line.Length)
                {
                    var c = line[col];
                    if (Char.IsDigit(c))
                    {
                        var digits = GetDigits(line, col);
                        var gears = GearsNearPartNbr(lines, digits, row, col);
                        foreach (var g in gears)
                        {
                            var key = $"{g.Row}_{g.Col}";
                            if (partGears.TryGetValue(key, out List<GearInfo>? part))
                            {
                                part.Add(g);
                            }
                            else
                            {
                                partGears[key] = new List<GearInfo>() { g };
                            }
                        }
                        col += digits.Length;
                    }
                    else
                    {
                        col++;
                    }
                }
            }

            sum = partGears.Where(x => x.Value.Count == 2)
                .Sum(x => x.Value.ElementAt(0).Number * x.Value.ElementAt(1).Number);

            Console.WriteLine($"Part 2: {sum}");
        }

        /// <summary>
        /// Find gears/symbols next to the part number. Return one GearInfo per symbol found
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="digits"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns>List of all gears around number</returns>
        static List<GearInfo> GearsNearPartNbr(string[] lines, string digits, int row, int col)
        {
            var result = new List<GearInfo>();
            var len = digits.Length;
            var min_r = Math.Max(row - 1, 0);
            var max_r = Math.Min(row + 1, lines.Length - 1);
            var min_c = Math.Max(col - 1, 0);
            var max_c = Math.Min(col + len, lines[row].Length - 1);
            var block = "";

            for (var r = min_r; r <= max_r; r++)
            {
                for (var c = min_c; c <= max_c; c++)
                {
                    var ch = lines[r][c];
                    block += ch;
                    if (Char.IsDigit(ch)) continue;
                    if (ch == '.') continue;
                    result.Add(new GearInfo() { Number = int.Parse(digits), Col = c, Row = r, Part = ch});
                }
                block += "\n";
            }

            //Console.Write(block);
            //Console.WriteLine(result.Count());
            //Console.WriteLine();

            return result;  
        }

        /// <summary>
        /// Get consecutive digits from a string starting at ix
        /// </summary>
        /// <param name="line"></param>
        /// <param name="ix"></param>
        /// <returns>Digits concatenated to a string</returns>
        static string GetDigits(string line, int ix)
        {
            var m = Regex.Match(line.Substring(ix), "(\\d+)");
            if (m.Groups.Count == 2)
            {
                var num = int.Parse(m.Groups[1].Value);
                return m.Groups[1].Value;
            }
            return "";
        }

        public class GearInfo
        {
            public int Number { get; set; }
            public int Row { get; set; }
            public int Col { get; set; }    
            public char Part { get; set; }
        }

        private static string testData = @"467..114..
...*......
..35..633.
......#...
617*......
.....+.58.
..592.....
......755.
...$.*....
.664.598..";
    }
}