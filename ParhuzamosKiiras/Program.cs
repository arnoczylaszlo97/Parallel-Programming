using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParhuzamosKiíras
{
    // # KONKURENS KIÍRÁS DURVA SZINKRONIZÁCIÓVAL.
    // # ADDIG ÍRUNK KI RANDOM SZÁMOKAT AMÍG ÖSSZEADVA NEM EGYENLŐEK A MEGADOTT SZÁMMAL.
    static class Util
    {
        public static Random rnd = new Random();
        public static List<int> Numbers = new List<int>();
    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(" < ADD THE MAXVALUE OF THE COUNTER! >");
            int maxValue = int.Parse(Console.ReadLine());
            Console.Clear();

            List<ConsoleWriter> Threads = Enumerable.Range(1, 8).Select(x => new ConsoleWriter()).ToList();
            object Lockobject = new object();
            Threads.ForEach(x => new Task(() =>
            {
                while (ConsoleWriter.Finished != true)
                {
                    int random = Util.rnd.Next(0, 10);
                    lock (Lockobject)
                    {
                        if (Util.Numbers.Sum() + random < maxValue)
                        {
                            Util.Numbers.Add(random);
                            x.oszlop++;
                            x.Write(random);
                        }
                        else if (Util.Numbers.Sum() + random == maxValue)
                        {
                            Util.Numbers.Add(random);
                            x.oszlop++;
                            x.Write(random);
                            ConsoleWriter.Finished = true;
                        }
                        int Y_max = Threads.Max(m => m.sor + 2);

                        Console.SetCursorPosition(0, Y_max);
                        if(Util.Numbers.Sum()!=maxValue)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($" [CURRENT NUMBERS.SUM() : [{ Util.Numbers.Sum()}] < [{maxValue}]");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($" [CURRENT NUMBERS.SUM() : [{ Util.Numbers.Sum()}] = [{maxValue}]");
                        }
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($" [MAX VALUE : {maxValue}]");
                        if (ConsoleWriter.Finished == true)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            int Y_max2 = Threads.Max(m => m.sor + 4);
                            Console.SetCursorPosition(0, Y_max2);
                            Console.WriteLine(" [SZIMULACIO VEGE]" + (Threads.Sum(y => y.Added.Sum()).Equals(maxValue) ? " [VERIFYD]" : " [NOT VERIFYD]"));
                        }

                        Thread.Sleep(Util.rnd.Next(50, 101));
                    }
                }
            }, TaskCreationOptions.LongRunning).Start());
            Console.ReadLine();
        }
    }

    class ConsoleWriter
    {
        static int _NextID = 0;
        public static bool Finished { get; set; }
        static object lockObject = new object();
        static List<ConsoleColor> Colors = new List<ConsoleColor>()
        {
            ConsoleColor.Red,
            ConsoleColor.Yellow,
            ConsoleColor.Green,
            ConsoleColor.DarkCyan,
            ConsoleColor.Magenta,
            ConsoleColor.White,
            ConsoleColor.DarkMagenta,
            ConsoleColor.DarkGray,
            ConsoleColor.DarkYellow,
            ConsoleColor.DarkGreen,
        };
        public ConsoleColor Color { get; private set; }
        public int oszlop { get; set; }
        public int sor { get; private set; }
        public List<int> Added { get; private set; }
        public ConsoleWriter()
        {
            Color = Colors[_NextID];
            sor = _NextID++;
            oszlop = 0;
            Added = new List<int>();
        }
        public void Write(int random)
        {
            Console.ForegroundColor = Color;
            Console.SetCursorPosition(oszlop, sor);
            Console.Write(random);
            Added.Add(random);
        }
    }
}
