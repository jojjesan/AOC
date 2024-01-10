using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Day20
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
                var day = 20;
                data = await client.GetStringAsync($"https://adventofcode.com/2023/day/{day}/input");
                File.WriteAllText($"C:\\Source\\Aoc\\2023\\data{day}.txt", data);
            }
            else
            {
                Console.WriteLine("## RUNNING TEST DATA");
                data = testData2.Substring(2).Replace("\r", "");
            }

            var config = data.Split("\n").Where(x => x != "").Select(x => x.Split(" -> "))
                .Select(y => new {Comp = y[0], Receivers = y[1].Split(", ") }).ToArray();

            var components = new Dictionary<string, IComponent>();
            components.Add("output", new Output { Name = "Output", Receivers = new string[] { } });
            var allReceivers = new List<Tuple<string, string>>();
            foreach (var item in config)
            {
                var name = item.Comp.Substring(1);
                if (item.Comp == "broadcaster")
                {
                    components.Add(item.Comp, new Broadcaster { Name = item.Comp, Receivers = item.Receivers });
                    item.Receivers.ToList().ForEach(x => allReceivers.Add(Tuple.Create(item.Comp, x)));
                }
                else if (item.Comp.StartsWith("%"))
                {
                    components.Add(name, new FlipFlop { Name = name, Receivers = item.Receivers });
                    item.Receivers.ToList().ForEach(x => allReceivers.Add(Tuple.Create(name, x)));
                }
                else if (item.Comp.StartsWith("&"))
                {
                    components.Add(name, new Conjunction { Name = name, Receivers = item.Receivers });
                    item.Receivers.ToList().ForEach(x => allReceivers.Add(Tuple.Create(name, x)));
                }
            }
            allReceivers.Where(x => !components.ContainsKey(x.Item2))
                .ToList().ForEach(x => components.Add(x.Item2, new Output() { Name = x.Item2, Receivers = new string[] { } }));
            allReceivers.Where(x => components[x.Item2] is Conjunction).ToList()
                .ForEach(x => (components[x.Item2] as Conjunction).ConnectInput(x.Item1));

            // We can only run one part each time
            //Part1(components);

            var conj = FindLevel(components, 1);
            Part2(components, 10000);
        }

        static IComponent[] FindLevel(Dictionary<string, IComponent> components, int levels)
        {
            var comps = new List<IComponent>() { components["rx"] };
            var listed = new Dictionary<string, bool>();
            var ix = 0;
            while (comps.Any() && ix < levels)
            {
                if (comps.Any(x => listed.ContainsKey(x.Name)))
                    throw new Exception("Looping");
                comps.ForEach(x => listed[x.Name] = true);
                comps = comps.SelectMany(x => components.Values.Where(y => y.Receivers.ToList().Contains(x.Name))).ToList();
                Console.WriteLine($"{string.Join(";", comps.Select(x => x.ToString()))}");
                ix++;
            }

            return comps.ToArray();
        }

        static void DrawComponents(Dictionary<string, IComponent> components, List<Tuple<string, string>> allReceivers)
        {
            var comps = new List<IComponent>() { components["broadcaster"] };
            var drawn = new Dictionary<string, bool>();
            while (comps.Any())
            {
                var notDrawnC = comps.Where(x => !drawn.ContainsKey(x.Name)).ToList();
                var drawnC = comps.Where(x => drawn.ContainsKey(x.Name)).ToList();
                Console.Write($"{string.Join("; ", notDrawnC.Select(x => $"{x.ToString()}").ToArray())} | ");
                Console.WriteLine($"{string.Join("^; ", drawnC.Select(x => $"{x.ToString()}").ToArray())}");
                Console.WriteLine();
                notDrawnC.ForEach(x => drawn[x.Name] = true);
                comps = notDrawnC.SelectMany(x => x.Receivers.Select(y => components[y])).ToList();
            }
        }

        static long Part2(Dictionary<string, IComponent> components, long buttonPresses = 1000)
        {
            var high = 0;
            var low = 0;
            var res = new Dictionary<string, long>();
            for (long i = 0; i < buttonPresses; i++)
            {
                var signals = new Queue<Signal>();
                signals.Enqueue(new Signal(false, "broadcaster", "button"));
                while (signals.Any())
                {
                    var sig = signals.Dequeue();
                    //Console.WriteLine($"{sig.Sender} -{sig.Value}-> {sig.Receiver}");
                    if (sig.Value)
                        high++;
                    else
                        low++;
                    if (sig.Receiver == "nr" && sig.Value == true)
                    {
                        Console.WriteLine($"{i+1}: {sig.Sender}={sig.Value}=>{sig.Receiver}");
                        if (!res.ContainsKey(sig.Sender))
                            res[sig.Sender] = i + 1;
                    }
                    var comp = components[sig.Receiver];
                    var newSigs = comp.Input(sig);
                    foreach (var newSig in newSigs)
                        signals.Enqueue(newSig);
                }
                if (i % 100000 == 0)
                    Console.Write($"\r{i}");
            }

            //var output = components["output"] as Output;
            Console.WriteLine($"Part 2: {res.Values.Aggregate((prev, next) => prev*next)}");
            return 0;
        }

        static long Part1(Dictionary<string, IComponent> components, long buttonPresses = 1000)
        {
            var high = 0;
            var low = 0;
            for (long i = 0; i < buttonPresses; i++)
            {
                var signals = new Queue<Signal>();
                signals.Enqueue(new Signal(false, "broadcaster", "button"));
                while (signals.Any())
                {
                    var sig = signals.Dequeue();
                    if (sig.Receiver == "rx" && sig.Value == false)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"Part 2: {i + 1}");
                        return i + 1;
                    }
                    //Console.WriteLine($"{sig.Sender} -{sig.Value}-> {sig.Receiver}");
                    if (sig.Value)
                        high++;
                    else
                        low++;
                    var comp = components[sig.Receiver];
                    var newSigs = comp.Input(sig);
                    foreach (var newSig in newSigs)
                        signals.Enqueue(newSig);
                    if (i % 100000 == 0)
                        Console.Write($"\r{i}");
                }
                //Console.WriteLine($"{i}: {low}, {high}");
            }

            var output = components["output"] as Output;
            Console.WriteLine($"Part 1: {low * high}");
            return 0;
        }

        public interface IComponent
        {
            public Signal[] Input(Signal pulse);
            public string Name { get; set; }
            public string[] Receivers { get; set; }
        }

        public class Output : IComponent
        {
            public string Name { get; set; }
            public int CountHigh { get; set; }
            public int CountLow { get; set; }
            public string[] Receivers { get; set; }
            public Signal[] Input(Signal pulse)
            {
                if (pulse.Value)
                    CountHigh++;
                else
                    CountLow++;
                return new Signal[0];
            }
            public override string ToString()
            {
                return $"#{Name}(Outp): {string.Join(",", Receivers)}";
            }
        }

        public class FlipFlop: IComponent
        {
            public string Name { get; set; }
            public bool Value { get; set; }
            public string[] Receivers { get; set; }
            public Signal[] Input(Signal pulse)
            {
                if (pulse.Value == true)
                    return new Signal[0];
                Value = !Value;
                return Receivers.Select(x => new Signal(Value, x, Name)).ToArray();
            }
            public override string ToString()
            {
                return $"#{Name}(Flip): {string.Join(",", Receivers)}";
            }
        }

        public class Conjunction : IComponent
        {
            public string Name { get; set; }
            public bool Value { get
                {
                    return !Inputs.Values.All(x => x);
                } }
            public string[] Receivers { get; set; }
            public Dictionary<string, bool> Inputs { get; set; }
            public Conjunction()
            {
                Inputs = new Dictionary<string, bool>();
            }
            public void ConnectInput(string port)
            {
                Inputs.Add(port, false);
            }
            public Signal[] Input(Signal pulse)
            {
                Inputs[pulse.Sender] = pulse.Value;
                return Receivers.Select(x => new Signal(Value, x, Name)).ToArray();
            }
            public override string ToString()
            {
                return $"#{Name}(Conj): {string.Join(",", Receivers)} IN: {string.Join(",", Inputs.Keys)}";
            }
        }

        public class Broadcaster : IComponent
        {
            public string Name { get; set; }
            public bool Value { get; set; }
            public string[] Receivers { get; set; }
            public Signal[] Input(Signal pulse)
            {
                Value = pulse.Value;
                return Receivers.Select(x => new Signal(Value, x, Name)).ToArray();
            }
            public override string ToString()
            {
                return $"#{Name}(Brc): {string.Join(",", Receivers)}";
            }
        }

        public class Signal
        {
            public bool Value { get; set; }
            public string Receiver { get; set; }
            public string Sender { get; set; }
            public Signal(bool value, string receiver, string sender)
            {
                Value = value;
                Receiver = receiver;
                Sender = sender;
            }
        }

        private static string testData2 =
@"
broadcaster -> a
%a -> inv, con
&inv -> b
%b -> con
&con -> output
";

        private static string testData =
@"
broadcaster -> a, b, c
%a -> b
%b -> c
%c -> inv
&inv -> a
";
    }
}