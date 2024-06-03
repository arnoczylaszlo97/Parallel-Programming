using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Futoverseny
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Write(" < PRESS A BUTTON TO START THE RACE > ");
            Console.ReadKey();
            Console.OutputEncoding = Encoding.Unicode;

            List<Dog> dogs = Enumerable.Range(0, 20 )
                .Select(x => new Dog())
                .ToList();

            Task Init = new Task(() => {
                Dog.Init();
            },TaskCreationOptions.LongRunning);

            List<Task> ToConsole = dogs.Select(x => new Task(() =>
            {
                x.toConsole();
            }, TaskCreationOptions.LongRunning)).ToList();

            List<Task> Race = dogs.Select(x => new Task(() =>
            {
                x.CalcRow();
                x.Step();
            }, TaskCreationOptions.LongRunning)).ToList();

            Task Timer = new Task(() =>
            {
                while (Race.Any(x => !x.IsCompleted))
                {
                    lock (Dog.ConsoleLock)
                    {
                        Dog.Stat();
                    }
                }
            }, TaskCreationOptions.LongRunning);

            Init.Start();
            ToConsole.ForEach(x => x.Start());
            Race.ForEach(x => x.Start());
            Timer.Start();
            Console.ReadKey();
        }
        public class Dog: IComparable<Dog>
        {
            // STATIC LIST,CLOCK
            static List<Dog> AllDog = new List<Dog>();
            public static Stopwatch clock = Stopwatch.StartNew();
            // STATIC LOCK OBEJCTS
            public static object ConsoleLock = new object();
            public static object HelyezesLock = new object();
            public static object ListLock = new object();
            // STATIC FIELDS
            public static bool Sorted = false;
            public static int Start { get; private set; }
            public static int MaxIdLength { get; private set; }
            public static int MAX = 60;
            public static int GlobalHely = 1;
            public static string character = "●";
            public static char PaddingChar = '□';
            public static int nextID = 1;
            // LOCK OBJECTS
            public object FinishLock = new object();
            public object StepLock = new object();
            // FIELDS
            public int ID { get; private set; }
            public int RowIndex { get; private set; }
            public int Pace { get; private set; }
            public int TDistance { get; private set; }
            public int Helyezes { get; private set; }
            public long ElapsedTime { get; private set; }

            public Dog()
            {
                ID = nextID++;
                Pace = Util.rnd.Next(100,200);
                TDistance = 0;
                AllDog.Add(this);
            }

            //-------------------------
            // STATIC CONSOLE STAT METHOD***
            public static void Stat()
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.SetCursorPosition(0, 0);
                Console.Write($" ::FUTOVERSENY:: MAXTAVOLSAG[{MAX}] IDO({clock.ElapsedMilliseconds} ms)");
            }
            // STATIC INIT METHODS***
            public static void Init()
            {
                MaxIDLength();
                StartPoint();
            }
            // THREAD'S WORK METHOD***
            public void Step()
            {
                Thread.Sleep(Pace);
                while (TDistance != MAX)
                {
                    TDistance++;
                    lock (StepLock)
                    {
                        Monitor.Pulse(StepLock);
                    }
                    Thread.Sleep(Pace);
                }
                lock (HelyezesLock)
                {
                    Helyezes = GlobalHely++;
                    ElapsedTime = clock.ElapsedMilliseconds;
                }
                lock (FinishLock)
                {
                    Monitor.Pulse(FinishLock);
                }
            }
            // STEP TO CONSOLE METHOD***
            public void toConsole()
            {
                while (TDistance != MAX)
                {
                    lock (StepLock)
                    {
                        Monitor.Wait(StepLock);
                    }
                    lock (ConsoleLock)
                    {
                        ProgressToConsole();
                    }
                }
                lock (FinishLock)
                {
                    Monitor.Wait(FinishLock);
                }
                lock (ConsoleLock)
                {
                    FinishedToConsole();
                }
                if (AllFinished())
                {
                    clock.Stop();
                    lock (ConsoleLock)
                    {
                        AllFinishedToConsole();
                    }
                }
            }
            //-------------------------

            // CONSOLE PRIVATE METHODS
            private void ProgressToConsole()
            {
                if (Fastest().Contains(this))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else if (Slowest().Contains(this))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }
                Console.SetCursorPosition(0, RowIndex);
                Console.Write($" [{ID}] <{Pace} ms>:  ");
                Console.SetCursorPosition(Start, RowIndex);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"{ character.PadLeft(TDistance + 1, PaddingChar)}");
                Console.SetCursorPosition(Start + MAX + 1, RowIndex);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"|[{MAX - TDistance}] ");
            }
            private void FinishedToConsole()
            {
                Console.SetCursorPosition(Start + MAX + $"[{MAX - TDistance}] ".Length + 3, RowIndex);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"({Helyezes}#)");
                Console.SetCursorPosition(Start + MAX + $"[{MAX - TDistance}] ".Length + 3 + MaxIdLength + 1, RowIndex);
                Console.Write($"[{ElapsedTime} ms] <{ElapsedTime - (Pace * MAX)}>");
                if (Helyezes.Equals(1))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($" [NYERTES] ");
                }
                else if (Helyezes.Equals(MaxId()))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($" [UTOLSO] ");
                }
                Console.ForegroundColor = ConsoleColor.White;
            }
            private void AllFinishedToConsole()
            {
                Console.SetCursorPosition(0, AllDog.Select(x => x.ID).Max() + 1);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"::[ SZIMULACIO VEGE ]:: ({clock.ElapsedMilliseconds} ms)");
            }         
            // STAT PRIVATE METHODS
            private List<Dog> Fastest()
            {
                lock (ListLock)
                {
                    return AllDog.Where(x => x.Pace.Equals(AllDog.Select(y => y.Pace).Min())).ToList();
                }
            }
            private List<Dog> Slowest()
            {
                lock (ListLock)
                {
                    return AllDog.Where(x => x.Pace.Equals(AllDog.Select(y => y.Pace).Max())).ToList();
                }
            }
            private bool AllFinished()
            {
                lock (ListLock)
                {
                    return AllDog.All(x => x.TDistance.Equals(MAX));
                }
            }
            // STATIC INIT METHODS
            private static void StartPoint()
            {
                lock (ListLock)
                {
                    Start = AllDog.Select(x => $" [{x.ID}] ({x.Pace} ms): ".Length).Max();
                }
            }
            private static void MaxIDLength()
            {
                lock (ListLock)
                {
                    MaxIdLength = AllDog.Select(x => $"({MaxId()}#)".Length).Max();
                }
            }
            private static int MaxId()
            {
                lock (ListLock)
                {
                    return AllDog.Max(x => x.ID);
                }
            }
            // SORT ON CONSOLE
            public void CalcRow()
            {
                if(Sorted==true)
                {
                    lock (ListLock)
                    {
                        AllDog.Sort();
                        RowIndex = AllDog.IndexOf(this) + 1;
                    }
                }
                else
                {
                    RowIndex = ID;
                }
            }
            // ICOMPARABLE<Dog>
            public int CompareTo(Dog other)
            {         
               return Pace - other.Pace;
            }
            // UTILS
            static class Util
            {
                public static Random rnd = new Random();
            }
        }

    }
}
