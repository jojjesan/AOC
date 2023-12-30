using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Day12
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
                data = await client.GetStringAsync("https://adventofcode.com/2023/day/12/input");
                File.WriteAllText("C:\\Source\\Aoc\\2023\\data12.txt", data);
            }
            else
            {
                Console.WriteLine("## RUNNING TEST DATA");
                data = testData.Replace("\r", "");
            }

            Func<string, string> replaceChars = x => x.Replace("?", "X").Replace("#", "0").Replace(".", "1");

            var grid = data.Split("\n").Where(x => x != "").Select(x => x.Split(" "))
                .Select(x => new SpringRow {Springs = replaceChars(x[0]), BrokenSeqs = x[1].Split(",").Select(int.Parse).ToArray() })
                .ToArray();

            long sum = 0;
            foreach (var row in grid)
            {
                var rowSum = CheckOptions(row);
                sum += rowSum;
                Console.WriteLine($"Row sum: {rowSum}");
            }

            Console.WriteLine();
            // 9174 too high

            var grid2 = grid.Select((x, index) => new SpringRow { 
                Springs = $"{x.Springs}X{x.Springs}X{x.Springs}X{x.Springs}X{x.Springs}",
                BrokenSeqs = Duplicate(x.BrokenSeqs, 5),
                Nbr = index
            });
            long sum2 = 0;
            foreach (var row in grid2)
            {
                var rowSum = CheckOptions(row);
                sum2 += rowSum;
                Console.WriteLine($"Row sum: {rowSum}");
                //Console.WriteLine();
            }

            Console.WriteLine($"Part 1: {sum}");
            Console.WriteLine($"Part 2: {sum2}");
        }

        static int[] Duplicate(int[] arr, int times)
        {
            var res = new List<int>();
            for (var i = 0; i < times; i++)
            {
                res.AddRange(arr);
            }
            return res.ToArray();
        }

        static long CheckOptions(SpringRow row)
        {
            var start = new MatchStatus() { StartPos = 0, Length = 0, Level = 0, IsLast = false, Current = row.Springs };

            // Find matches
            var matches = FindAllMatches(row, start, 0);

            //Console.WriteLine($"All matches in '{row.Springs}' for {string.Join(",", row.BrokenSeqs)}: {string.Join($"\n{row.Springs}\n", matches.Where(x => x.IsLast))}");

            return matches;
        }

        static int CountBroken(string currSprings)
        {
            return currSprings.Count(x => x == '0');
        }   

        static Dictionary<string, long> _cache = new Dictionary<string, long>();

        static string GetCacheKey(SpringRow row, MatchStatus curr, int level)
        {
            var startPos = curr.StartPos + curr.Length;
            return $"{row.Nbr}-{level}-{curr.Length}-{curr.StartPos}-{CountBroken(curr.Current)}";
        }

        static long FindAllMatches(SpringRow row, MatchStatus curr, int level)
        {
            if (level >= row.BrokenSeqs.Length) return 0;
            var key = GetCacheKey(row, curr, level);
            if (_cache.ContainsKey(key)) return _cache[key];

            var matches = FindMatches(
                curr.Current, 
                curr.StartPos + curr.Length, 
                row.BrokenSeqs[level],
                level, row.BrokenSeqs.Length);

            long matchesSum = 0;
            if (level == row.BrokenSeqs.Length - 1)
                matchesSum = matches.Where(x => CountBroken(x.Current) == row.BrokenSeqs.Sum()).Count();
            else
                matchesSum = matches.Sum(x => FindAllMatches(row, x, level + 1));

            //if (_cache.ContainsKey(key)) Assert.AreEqual(_cache[key], matchesSum);
            _cache[key] = matchesSum;
            return matchesSum;
        }   

        static List<MatchStatus> FindMatches(string springs, int startPos, int length, int level, int noBreakIntervals)
        {
            if (startPos + length > springs.Length) return new List<MatchStatus>();

            var str = $"1{springs}1";
            var pattern = $"([1X])([0X]{{{length}}})([1X])";
            var re = new Regex(pattern);

            var matches = new List<MatchStatus>();
            var currPos = startPos == 0 ? 0 : startPos + 1;
            while (currPos < str.Length)
            {
                var match = re.Match(str.Substring(currPos));
                if (!match.Success) break;
                var startStr = springs.Substring(0, Math.Max(0, currPos + match.Index)).Replace("X", "1");
                startStr += match.Groups[2].Value.Replace("X", "0") + "1";
                var ms = new MatchStatus()
                {
                    StartPos = currPos + match.Groups[2].Index - 1,
                    Length = length,
                    Level = level,
                    Current = startStr + springs.Substring(Math.Min(startStr.Length, springs.Length))
                };
                ms.Current = ms.Current.Substring(0, springs.Length);
                if (level == noBreakIntervals-1)
                {
                    ms.Current = ms.Current.Replace("X", "1");
                }
                Assert.AreEqual(springs.Length, ms.Current.Length);
                matches.Add(ms);
                currPos += match.Index + 1;
            }
            if (matches.Any())
            {
                return matches;
            }

            return new List<MatchStatus>();
        }

        class MatchStatus
        {
            public int StartPos { get; set; }
            public int Length { get; set; }
            public bool IsLast { get; set; }
            public int Level { get; set; }
            public string Current { get; set; }

            public override string ToString()
            {
                return $"{Level}: {StartPos} {Length}";
            }
        }

        struct SpringRow
        {
            public int Nbr;
            public string Springs;
            public int[] BrokenSeqs;
            public override string ToString()
            {
                return $"({Springs}, {BrokenSeqs})";
            }
        }

        private static string testData2 =
@"";

        private static string testData =
@"???.### 1,1,3
.??..??...?##. 1,1,3
?#?#?#?#?#?#?#? 1,3,1,6
????.#...#... 4,1,1
????.######..#####. 1,6,5
?###???????? 3,2,1";
    }
}