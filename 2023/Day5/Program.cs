using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Data;

namespace Day5
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
                data = await client.GetStringAsync("https://adventofcode.com/2023/day/5/input");
            }
            else
            {
                Console.WriteLine("## RUNNING TEST DATA");
                data = testData.Replace("\r", "");
            }

            // Parse indata
            var lines = data.Split("\n").ToArray();

            var seeds = lines[0].Split(": ")[1].Split(" ").Select(x => long.Parse(x)).ToArray();
            var ix = 1;
            var maps = new Map[7];
            for (var i = 0; i < 7; i++)
            {
                maps[i] = GetNextMap(lines, ref ix);
                ix++;
            }

            //Part1(seeds, maps);

            // Part 2
            var max = 1000;
            var seeds2 = new List<ValueRange>();
            for (var i = 0; i < seeds.Length; i += 2)
            {
                seeds2.Add(new ValueRange { Start = seeds[i], Range = seeds[i + 1] });
            }

            Part2(seeds2, maps);

            //for (var i = 0; i < 7; i++)
            //{
            //    Console.WriteLine($"Map {i}");
            //    Console.WriteLine(string.Join("\n", maps[i].Items.Select(x => x.ToString()).ToArray()));
            //}
        }

        static void Part2(List<ValueRange> seeds, Map[] maps)
        {
            // Part 2
            var results = new List<ValueRange>();
            foreach (var seed in seeds)
            {
                var res = MapRangeValue(seed, maps);
                results.AddRange(res);
            }

            if (results.Any())
            {
                Console.WriteLine($"Part 2: {results.Min(x => x.Start)}");
            }
            else
            {
                Console.WriteLine($"Part 2: No results");
            }

            foreach (var res in results)
            {
                Console.WriteLine($"{res.Start} {res.Range}");
            }
        }

        static ValueRange[] MapRangeValue(ValueRange valRange, Map[] maps)
        {
            var valRanges = new List<ValueRange>();
            valRanges.Add(valRange);

            for (var i = 0; i < 7; i++)
            {
                var nextValRanges = new List<ValueRange>();
                foreach (var vRange in valRanges)
                {
                    var mappedRanges = GetMappedRanges(vRange, maps[i]);
                    var allValueRanges = FillGaps(vRange, mappedRanges);
                    nextValRanges.AddRange(allValueRanges);
                }
                Console.WriteLine($"Map ({valRange.ToString()}) {i} {string.Join(";", nextValRanges.Select(x => x.ToString()).ToArray())}");
                valRanges = nextValRanges;
            }

            return valRanges.ToArray();
        }

        static List<MappedValueRange> GetMappedRanges(ValueRange valRange, Map map)
        {
            var mappedRanges = new List<MappedValueRange>();
            foreach (var item in map.Items)
            {
                var intersect = IntersectMap(item, valRange);
                mappedRanges.Add(intersect);
            }
            return mappedRanges.Where(x => x.Source.Range > 0).ToList();
        }

        static MappedValueRange IntersectMap(MapItem mapItem, ValueRange valRange)
        {
            var offset = mapItem.Dest - mapItem.Source;
            var start = Math.Max(valRange.Start, mapItem.Source);
            var end = Math.Min(valRange.Start + valRange.Range, mapItem.Source + mapItem.Range);
            MappedValueRange res;
            if (start < end)
            {
                res = new MappedValueRange(){
                    Dest = new ValueRange { Start = start + offset, Range = end - start },
                    Source = new ValueRange { Start = start, Range = end - start }};
            }
            else
            {
                // No intersection, just return empty range
                res = new MappedValueRange();
            }
            return res;
        }

        static List<ValueRange> FillGaps(ValueRange valRange, IEnumerable<MappedValueRange> mappedRanges)
        {
            var res = new List<ValueRange>(mappedRanges.Select(x => x.Dest));
            var start = valRange.Start;
            foreach (var mappedRange in mappedRanges.OrderBy(x => x.Source.Start))
            { 
                if (mappedRange.Source.Start > start)
                {
                    res.Add(new ValueRange { Start = start, Range = mappedRange.Source.Start - start });
                }
                start = mappedRange.Source.Start + mappedRange.Source.Range;
            }
            if (start < valRange.Start + valRange.Range)
            {
                res.Add(new ValueRange { Start = start, Range = valRange.Start + valRange.Range - start });
            }
            return res;
        }

        static void Part1(long[] seeds, Map[] maps)
        {
            // Part 1
            var results = new List<long>();
            foreach (var seed in seeds)
            {
                var res = MapValue(seed, maps);
                results.Add(res);
            }

            Console.WriteLine($"Part 1: {results.Min()}");
        }

        static long MapValue(long seed, Map[] maps)
        {
            var res = seed;
            for (var i = 0; i < 7; i++)
            {
                var resMap = maps[i].Items.Where(x => x.Source <= res && x.Source + x.Range > res).FirstOrDefault();
                if (resMap != null)
                {
                    res = resMap.Dest + (res - resMap.Source);
                }
            }
            return res;
        }

        static Map GetNextMap(string[] lines, ref int ix)
        {
            ix++;
            var data = new List<string>();
            while (ix < lines.Length && lines[ix] != "")
            {
                data.Add(lines[ix]);
                ix++;
            }
            var res = new Map();
            res.Parse(data.ToArray());
            return res;
        }

        private static string testData = @"seeds: 79 14 55 13

seed-to-soil map:
50 98 2
52 50 48

soil-to-fertilizer map:
0 15 37
37 52 2
39 0 15

fertilizer-to-water map:
49 53 8
0 11 42
42 0 7
57 7 4

water-to-light map:
88 18 7
18 25 70

light-to-temperature map:
45 77 23
81 45 19
68 64 13

temperature-to-humidity map:
0 69 1
1 0 69

humidity-to-location map:
60 56 37
56 93 4";
    }

    public class Map
    {
        public Map()
        {
            Items = new List<MapItem>();
        }

        public void Parse(string[] rows)
        {
            foreach (var row in rows)
            {
                if (row.Contains(":")) continue;

                var parts = row.Split(" ");
                Items.Add(new MapItem
                {
                    Dest = long.Parse(parts[0]),
                    Source = long.Parse(parts[1]),
                    Range = long.Parse(parts[2])
                });
            }
        }

        public List<MapItem> Items { get; set; }
    }

    public class MapItem
    {
        public long Dest { get; set; }
        public long Source { get; set; }
        public long Range { get; set; }
        public  override string ToString()
        {
            return $"{Dest} {Source} {Range}";
        }
    }

    public class ValueRange
    {
        public long Start { get; set; }
        public long Range { get; set; }

        public override string ToString()
        {
            return $"{Start} {Range}";
        }
    }
    public class MappedValueRange
    {
        public ValueRange Source { get; set; }
        public ValueRange Dest { get; set; }
        public MappedValueRange()
        {
            Source = new ValueRange();
            Dest = new ValueRange();
        }
        public override string ToString()
        {
            return $"{Source.Start} {Source.Range} => {Dest.Start} {Dest.Range}";
        }
    }
}