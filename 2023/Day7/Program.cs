using System.Collections.Immutable;
using System.Data;
using System.Reflection.Metadata;
using System.Xml;

namespace Day7
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
                data = await client.GetStringAsync("https://adventofcode.com/2023/day/7/input");
            }
            else
            {
                Console.WriteLine("## RUNNING TEST DATA");
                data = testData.Replace("\r", "");
            }

            // Parse indata
            var lines = data.Split("\n").Where(x => x != "").ToArray();
            var hands = new List<Hand>();
            foreach (var line in lines)
            {
                var hand = new Hand(line.Split(" ")[0].Select(x => GetCardInt(x)).ToList());
                hand.Bid = int.Parse(line.Split(" ")[1]);
                hand.Score = GetScore(hand.Cards);
                hands.Add(hand);
            }

            // Part 1
            //Part1(hands);

            // Part 2
            Part2(hands);
            // 255488666: too low
            // 255632664

        }
        static void Part2(List<Hand> hands)
        {
            foreach (var hand in hands)
            {
                // J is joker and ranks last
                hand.Cards = hand.Cards.Select(x => x == 11 ? 1 : x).ToList();
                hand.Score = GetScore2(hand.Cards);

            }
            hands.Sort((x, y) => CompareHands(x, y));

            Console.WriteLine($"Hands: {string.Join("\n", hands.Select(x => x.ToString()).ToArray())}");

            var sum = 0;
            for (var i = 0; i < hands.Count; i++)
            {
                sum += hands[i].Bid * (i + 1);
            }

            Console.WriteLine($"Part 2: {sum}");

        }

        static void Part1(List<Hand> hands)
        {
            hands.Sort((x, y) => CompareHands(x, y));

            Console.WriteLine($"Hands: {string.Join("\n", hands.Select(x => x.ToString()).ToArray())}");

            var sum = 0;
            for (var i = 0; i < hands.Count; i++)
            {
                sum += hands[i].Bid * (i + 1);
            }

            Console.WriteLine($"Part 1: {sum}");

        }

        static int CompareHands(Hand x, Hand y)
        {
            if (x.Score > y.Score)
                return 1;
            if (x.Score < y.Score)
                return -1;
            for (var i = 0; i < x.Cards.Count; i++)
            {
                if (x.Cards[i] > y.Cards[i])
                    return 1;
                if (x.Cards[i] < y.Cards[i])
                    return -1;
            }
            return 0;
        }

        static int GetScore2(List<int> cards)
        {
            var cardDict = new Dictionary<int, int>();
            var nbrOfJokers = cards.Where(x => x == 1).Count();
            if (nbrOfJokers >= 4)
            {
                return 6;
            }

            foreach (var card in cards)
            {
                if (card == 1) continue;

                if (cardDict.ContainsKey(card))
                    cardDict[card]++;
                else
                    cardDict.Add(card, 1);
            }

            // 3, 4 or 5 jokers will result in 4 or five of the same card
            if (cardDict.Values.Max() + nbrOfJokers == 5)
            {
                return 6;
            }
            if (cardDict.Values.Max() + nbrOfJokers == 4)
            {
                return 5;
            }

            // The rest of the cases can have maximum 2 jokers
            if (cardDict.Values.Max() == 3 && cardDict.Values.Min() == 2)
            {
                return 4;
            }
            if (cardDict.Values.Max() == 3)
            {
                // 1 or 2 jokers will behandled earlier (4 or 5 of a kind)
                return 3;
            }
            if (cardDict.Values.Where(x => x == 2).Count() == 2)
            {
                if (nbrOfJokers == 1)
                {
                    return 4;
                }
                return 2;
            }
            if (cardDict.Values.Where(x => x == 2).Count() == 1)
            {
                // 2 or 3 jokers will behandled earlier (4 or 5 of a kind)
                if (nbrOfJokers == 1)
                {
                    return 3;
                }
                return 1;
            }
            if (nbrOfJokers == 2)
            {
                return 3;
            }
            if (nbrOfJokers == 1)
            {
                return 1;
            }
            return 0;
        }

        static int GetScore(List<int> cards)
        {
            var cardDict = new Dictionary<int, int>();
            foreach (var card in cards)
            {
                if (cardDict.ContainsKey(card))
                    cardDict[card]++;
                else
                    cardDict.Add(card, 1);
            }
            if (cardDict.Values.Max() == 5)
            {
                return 6;
            }
            if (cardDict.Values.Max() == 4)
            {
                return 5;
            }
            if (cardDict.Values.Max() == 3 && cardDict.Values.Min() == 2)
            {
                return 4;
            }
            if (cardDict.Values.Max() == 3)
            {
                return 3;
            }
            if (cardDict.Values.Where(x => x == 2).Count() == 2)
            {
                return 2;
            }
            if (cardDict.Values.Where(x => x == 2).Count() == 1)
            {
                return 1;
            }
            return 0;
        }

        static int GetCardInt(char card)
        {
            switch (card)
            {
                case 'T':
                    return 10;
                case 'J':
                    return 11;
                case 'Q':
                    return 12;
                case 'K':
                    return 13;
                case 'A':
                    return 14;
                default:
                    return int.Parse(card.ToString());
            }
        }

        private static string testData = @"32T3K 765
T55J5 684
KK677 28
KTJJT 220
QQQJA 483";
    }
    public class Hand
    {
        public List<int> Cards { get; set; }
        public int Score { get; set; }
        public int Bid { get; set; }
        public Hand(List<int> cards)
        {
            Cards = cards;
        }

        public string ToString()
        {
            return $"{string.Join(" ", Cards.Select(x => x.ToString()).ToArray())} Score={Score} Bid={Bid}";
        }
    }

}