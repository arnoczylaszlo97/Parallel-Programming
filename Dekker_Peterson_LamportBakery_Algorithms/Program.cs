using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using ConsoleTools;

namespace Dekker_Peterson_LamportBakery_Algorithms
{
    static public class Util
    {
        public static Stopwatch watch = new Stopwatch();
        public static object ConsoleLock = new object();
        public static int allapotSzamlalo = 0;
        public const int CTime = 1000;
        public const int BTime = 200;
    }
    class Program
    {
        static void Main(string[] args)
        {
            // INIT
            bool[] flag = new bool[2];
            flag[0] = false;
            flag[1] = false;
            int turn = 0;

            // DEKKER-ALGORITMUSA (2 szál közötti kölcsönös kizárás)
            // -------------------------------------------------------------------

            Task dekker0 = new Task(() =>
            {
                flag[0] = true;
                lock (Util.ConsoleLock)
                {
                    Console.WriteLine($":: 0 WANTS TO ENTER! <{Util.watch.ElapsedMilliseconds} ms>");
                }
                while (flag[1] == true)
                {
                    if (turn != 0)
                    {
                        flag[0] = false;
                        while (turn != 0)
                        {
                            //BUSY WAITING
                            lock (Util.ConsoleLock)
                            {
                                Console.WriteLine($":: 0 IS BUSY WAITING <{Util.watch.ElapsedMilliseconds} ms>");
                            }
                            Thread.Sleep(Util.BTime);

                        }
                        flag[0] = true;
                    }
                }
                // ENTER CRITICAL SECTION
                lock (Util.ConsoleLock)
                {
                    Util.allapotSzamlalo++;
                    Console.WriteLine($"::[{Util.allapotSzamlalo}] 0 IS IN THE CRITICAL SECTION <{Util.watch.ElapsedMilliseconds} ms>");
                }
                Thread.Sleep(Util.CTime);
                //LEAVING CRITICAL SECTION
                turn = 1;
                flag[0] = false;
                lock (Util.ConsoleLock)
                {
                    Util.allapotSzamlalo++;
                    Console.WriteLine($"::[{Util.allapotSzamlalo}] 0 IS LEAVING THE CRITICAL SECTION <{Util.watch.ElapsedMilliseconds} ms>");
                }
            }, TaskCreationOptions.LongRunning);
            Task dekker1 = new Task(() =>
            {
                flag[1] = true;
                lock (Util.ConsoleLock)
                {
                    Console.WriteLine($":: 1 WANTS TO ENTER! <{Util.watch.ElapsedMilliseconds} ms>");
                }
                while (flag[0] == true)
                {
                    if (turn != 1)
                    {
                        flag[1] = false;
                        while (turn != 1)
                        {
                            //BUSY WAITING
                            lock (Util.ConsoleLock)
                            {
                                Console.WriteLine($":: 1 IS BUSY WAITING <{Util.watch.ElapsedMilliseconds} ms>");
                            }
                            Thread.Sleep(Util.BTime);
                        }
                        flag[1] = true;
                    }
                }
                // ENTER CRITICAL SECTION
                lock (Util.ConsoleLock)
                {
                    Util.allapotSzamlalo++;
                    Console.WriteLine($"::[{Util.allapotSzamlalo}] 1 IS IN THE CRITICAL SECTION <{Util.watch.ElapsedMilliseconds} ms>");
                }
                Thread.Sleep(Util.CTime);
                //LEAVING CRITICAL SECTION
                turn = 0;
                flag[1] = false;
                lock (Util.ConsoleLock)
                {
                    Util.allapotSzamlalo++;
                    Console.WriteLine($"::[{Util.allapotSzamlalo}] 1 IS LEAVING THE CRITICAL SECTION <{Util.watch.ElapsedMilliseconds} ms>");
                }
            }, TaskCreationOptions.LongRunning);

            List<Task> Dekker = new List<Task>();
            Dekker.Add(new Task(() => { Util.allapotSzamlalo=0; }));
            Dekker.Add(new Task(() => { dekker0.Start(); }));
            Dekker.Add(new Task(() => { dekker1.Start(); }));
            Dekker.Add(new Task(() => { Util.watch.Start(); }));



            // PETERSON-ALGORITMUSA (2 szál közötti kölcsönös kizárás)
            // -------------------------------------------------------------------

            Task Peterson0 = new Task(() =>
            {
                flag[0] = true;
                Console.WriteLine($":: 0 WANTS TO ENTER! <{Util.watch.ElapsedMilliseconds} ms>");
                turn = 1;
                while (flag[1] == true && turn != 0)
                {
                    //BUSY WAITING
                    lock (Util.ConsoleLock)
                    {
                        Console.WriteLine($":: 0 IS BUSY WAITING! <{Util.watch.ElapsedMilliseconds} ms>");
                    }
                    Thread.Sleep(Util.BTime);
                }
                //ENTER CRITICAL SECTION
                lock (Util.ConsoleLock)
                {
                    Util.allapotSzamlalo++;
                    Console.WriteLine($"::[{Util.allapotSzamlalo}] 0 IS IN THE CRITICAL SECTION! <{Util.watch.ElapsedMilliseconds} ms>");
                }
                Thread.Sleep(Util.CTime);
                //LEAVING CRITICAL SECTION
                flag[0] = false;
                lock (Util.ConsoleLock)
                {
                    Util.allapotSzamlalo++;
                    Console.WriteLine($"::[{Util.allapotSzamlalo}] 0 IS LEAVING THE CRITICAL SECTION! <{Util.watch.ElapsedMilliseconds} ms>");
                }


            }, TaskCreationOptions.LongRunning);
            Task Peterson1 = new Task(() =>
            {
                flag[1] = true;
                Console.WriteLine($":: 1 WANTS TO ENTER! <{Util.watch.ElapsedMilliseconds} ms>");
                turn = 0;
                while (flag[0] == true && turn != 1)
                {
                    //BUSY WAITING
                    lock (Util.ConsoleLock)
                    {
                        Console.WriteLine($":: 1 IS BUSY WAITING! <{Util.watch.ElapsedMilliseconds} ms>");
                    }
                    Thread.Sleep(Util.BTime);
                }
                //ENTER CRITICAL SECTION
                lock (Util.ConsoleLock)
                {
                    Util.allapotSzamlalo++;
                    Console.WriteLine($"::[{Util.allapotSzamlalo}] 1 IS IN THE CRITICAL SECTION! <{Util.watch.ElapsedMilliseconds} ms>");

                }
                Thread.Sleep(Util.CTime);
                //LEAVING CRITICAL SECTION
                flag[1] = false;
                lock (Util.ConsoleLock)
                {
                    Util.allapotSzamlalo++;
                    Console.WriteLine($"::[{Util.allapotSzamlalo}] 1 IS LEAVING THE CRITICAL SECTION! <{Util.watch.ElapsedMilliseconds} ms>");

                }
            }, TaskCreationOptions.LongRunning);

            List<Task> Peterson = new List<Task>();
            Peterson.Add(new Task(() => { Util.allapotSzamlalo = 0; }));
            Peterson.Add(new Task(() => { Peterson0.Start(); }));
            Peterson.Add(new Task(() => { Peterson1.Start(); }));
            Peterson.Add(new Task(() => { Util.watch.Start(); }));


            // LAMPORT BAKERY - ALGORITMUSA ( N szál közötti kölcsönös kizárás)
            // -------------------------------------------------------------------
            List<Lamport> lamports = Enumerable.Range(1, Lamport.N).Select(x => new Lamport()).ToList();

            List<Task> lamportThreads = lamports.Select(x => new Task(() =>
            {
                x.LamportBakery();
            }, TaskCreationOptions.LongRunning)).ToList();

            List<Task> LAMPORT = new List<Task>();
            LAMPORT.Add(new Task(() => { Util.allapotSzamlalo = 0; }));
            LAMPORT.Add(new Task(() => { lamportThreads.ForEach(x => x.Start()); }));
            LAMPORT.Add(new Task(() => { Util.watch.Start(); }));
            //lamportThreads.ForEach(x => x.Start());
            //Util.watch.Start();


            //MENU
            // -------------------------------------------------------------------
            var menu = new ConsoleMenu()
                .Add("Dekker - Algorithm", () => Dekker.ForEach(x => x.Start()))
                .Add("Peterson - Algorithm", () => Peterson.ForEach(x => x.Start()))
                .Add("Lamport - Algorithm", () => LAMPORT.ForEach(x => x.Start()))
                .Add("Close", ConsoleMenu.Close)
            .Configure(config =>
            {
                config.Selector = "--> ";
            });

            menu.Show();

            Console.ReadLine();
        }

    }
    class Lamport
    {
        public static int N = 5;
        static bool[] choosing = new bool[N];
        static int[] num = new int[N];
        static int next_ID = 0;
        public int THREAD_ID { get; private set; }
        public Lamport()
        {
            THREAD_ID = next_ID++;
        }

        public void LamportBakery()
        {
            choosing[THREAD_ID] = true;
            num[THREAD_ID] = num.Max() + 1;
            choosing[THREAD_ID] = false;
            for (int j = 0; j < N; j++)
            {
                while (choosing[j])
                {
                    //BUSY WAITING
                    Console.WriteLine($"::{THREAD_ID} IS BUSY WAITING!:: <{Util.watch.ElapsedMilliseconds} ms>");
                    Thread.Sleep(Util.BTime);
                }
                if (num[j] > 0 && (num[j] < num[THREAD_ID] | (num[j] == num[THREAD_ID] && j < THREAD_ID)))
                {
                    while (num[j] > 0)
                    {
                        //BUSY WAITING
                        Console.WriteLine($"::{THREAD_ID} IS BUSY WAITING!:: <{Util.watch.ElapsedMilliseconds} ms>");
                        Thread.Sleep(Util.BTime);
                    }
                }
            }
            //KRITIKUS SZAKASZ BELÉPÉS
            lock (Util.ConsoleLock)
            {
                Util.allapotSzamlalo++;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"::[{Util.allapotSzamlalo}]{THREAD_ID} IS IN THE CRITICAL SECTION!  <{Util.watch.ElapsedMilliseconds} ms>");
                Console.ForegroundColor = ConsoleColor.White;
            }
            Thread.Sleep(Util.CTime);
            //KRITIKUS SZAKASZ KILÉPÉS
            lock (Util.ConsoleLock)
            {
                num[THREAD_ID] = 0;
                Util.allapotSzamlalo++;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"::[{Util.allapotSzamlalo}]{THREAD_ID} IS LEAVING THE CRITICAL SECTION!  <{Util.watch.ElapsedMilliseconds} ms>");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

    }
}
