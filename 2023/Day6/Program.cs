using System.Collections.Immutable;
using System.Data;
using System.Xml;

namespace Day6
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
                data = await client.GetStringAsync("https://adventofcode.com/2023/day/6/input");
            }
            else
            {
                Console.WriteLine("## RUNNING TEST DATA");
                data = testData.Replace("\r", "");
            }

            // Parse indata
            var lines = data.Split("\n").ToArray();
            var raceDurations = lines[0].Split(": ")[1].Split(" ").Where(x => x.Trim() != "")
                .Select(x => int.Parse(x.Trim())).ToArray();
            var recordDistances = lines[1].Split(": ")[1].Split(" ").Where(x => x.Trim() != "")
                .Select(x => int.Parse(x.Trim())).ToArray();

            var res = 1;
            for (var i = 0; i < raceDurations.Length; i++)
            {
                var holdTime = 0;
                var ix = 0;
                while (holdTime < raceDurations[i])
                {
                    if (GetDistance(holdTime, raceDurations[i]) > recordDistances[i])
                        ix++;
                    holdTime++;
                }
                Console.WriteLine($"Wins: {ix}");
                res = res * ix;
            }

            Console.WriteLine($"Part 1: {res}");

            // Part 2

            var raceDuration = long.Parse(lines[0].Split(":")[1].Replace(" ", ""));
            var recordDistance = long.Parse(lines[1].Split(":")[1].Replace(" ", ""));

            Console.WriteLine($"Part 2: Duration={raceDuration} Record={recordDistance}");
            var roots = GetRoots(-raceDuration, recordDistance);
            Console.WriteLine($"Part 2: Root 1={roots.Item1} Root 2={roots.Item2}");

            var firstRecord = (long)Math.Floor(roots.Item1);
            var lastRecord = (long)Math.Ceiling(roots.Item2);

            Console.WriteLine($"Part 2: First start={GetDistance(firstRecord, raceDuration)} Last start={GetDistance(lastRecord, raceDuration)}");
            while (GetDistance(firstRecord, raceDuration) <= recordDistance)
                firstRecord++;
            while (GetDistance(lastRecord, raceDuration) <= recordDistance)
                lastRecord--;
            Console.WriteLine($"Part 2: First end={GetDistance(firstRecord, raceDuration)} Last end={GetDistance(lastRecord, raceDuration)}");

            Console.WriteLine($"Part 2: Solution={lastRecord-firstRecord+1}");

            // h * (r-h) = rd
            // h*r - h*h = rd
            // h*h - r*h + rd = 0

        }

        static Tuple<double, double> GetRoots(double p, double q)
        {
            var root = Math.Sqrt((p/2)*(p/2) - q);
            return Tuple.Create(-p/2 - root, -p / 2 + root);
        }

        static long GetDistance(long holdTime, long raceDuration)
        {
            return holdTime * (raceDuration - holdTime);
        }


        private static string testData = @"Time:      7  15   30
Distance:  9  40  200";
    }

    
}