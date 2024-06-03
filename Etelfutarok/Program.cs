using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Etelfutarok2._0
{
    class Program
    {
        /*
         * SZABÁLYOZHATÓ BENNE :
         *  >> FUTAROK SZAMA 
         *  >> RENDELESEK SZAMA
         *  >> KONZOL FRISSITESI IDŐ
         *  >> HÁNY MILLISEC LEGYEN 1PERC (IDŐ)
         */

        static void Main(string[] args)
        {
            Console.ReadKey();
            //FUTAROK LETREHOZASA
            List<Futar> futarok = new List<Futar>();
            futarok.AddRange(Enumerable.Range(1, 2).Select(x => new FurgeFutar()).ToList());
            futarok.AddRange(Enumerable.Range(1, 2).Select(x => new TurboTeknos()).ToList());

            //BEJÖVŐ RENDELESEK SZÁL
            Task rendelestKeszit = new Task(() =>
            {
                Etterem.RendelestKeszit();
            }, TaskCreationOptions.LongRunning);

            //FUTAR SZÁLAK
            List<Task> futarTASK = futarok.Select(x => new Task(() =>
            {
                x.Work();
            }
            , TaskCreationOptions.LongRunning)).ToList();

            bool Vege()
            {
                if (futarok.Sum(x => x.OsszesPenz.Count()) == Etterem.MAX)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            string Nyertes()
            {
                double TurboTeknos = futarok.Where(x => x.GetType() == typeof(TurboTeknos)).Sum(x => x.OsszesPenz.Sum());
                double FurgeFutar = futarok.Where(x => x.GetType() == typeof(FurgeFutar)).Sum(x => x.OsszesPenz.Sum());
                double max = Math.Max(TurboTeknos, FurgeFutar);
                if (max.Equals(TurboTeknos))
                {
                    return typeof(TurboTeknos).Name;
                }
                else
                {
                    return typeof(FurgeFutar).Name;
                }
            }
            Task naplozo = new Task(() =>
            {
                int refreshRate = 300;
                var clock = Stopwatch.StartNew();

                while (futarTASK.Any(x => !x.IsCompleted))
                {
                    Console.Clear();

                    //BEJÖVŐ RENDELÉSEK  
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(" :: BEJOVO RENDELESEK ::");
                    lock (Etterem.RendelesekLock)
                    {
                        foreach (var rendeles in Etterem.Rendelesek)
                        {
                            switch (rendeles.Status)
                            {
                                case RendelesStatus.FutarraVar:
                                    Console.ForegroundColor = ConsoleColor.White;
                                    Console.WriteLine(rendeles.ToString());
                                    break;

                                case RendelesStatus.SzallitasAlatt:
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine(rendeles.ToString());
                                    break;
                                case RendelesStatus.Kiszallitva:
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine(rendeles.ToString());
                                    break;
                            }
                        }
                    }
                    Console.WriteLine();

                    //IDŐ
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($" {clock.ElapsedMilliseconds / Util.PERC} . perc (STOPWATCH) ");

                    //STATISZTIKA
                    Console.ForegroundColor = ConsoleColor.White;
                    lock (Etterem.RendelesekLock)
                    {
                        Console.WriteLine($" Elkeszult rendelesek szama: {Etterem.Rendelesek.Count()}");
                        Console.WriteLine($" Kiszallitas alatt levo rendelesek: {Etterem.Rendelesek.Where(x => x.Status == RendelesStatus.SzallitasAlatt).Count()}");
                        Console.WriteLine($" Futarra varakozó rendelesek: {Etterem.Rendelesek.Where(x => x.Status == RendelesStatus.FutarraVar).Count()}");
                        Console.WriteLine($" Kiszallitott rendelesek: {Etterem.Rendelesek.Where(x => x.Status == RendelesStatus.Kiszallitva).Count()}");
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($" [FuregFutar]  [{Etterem.Rendelesek.Where(x => x.Status == RendelesStatus.Kiszallitva && x.Futar.GetType() == typeof(FurgeFutar)).Count()}] : " +
                            $"{futarok.Where(x => x.GetType() == typeof(FurgeFutar)).Sum(x => x.OsszesPenz.Sum()) } Ft " +
                            $"\t[{futarok.Where(x => x.GetType() == typeof(FurgeFutar)).Sum(y => y.OsszesPenz.Count())}] " +
                            $"{(Vege().Equals(true) && Nyertes().Equals(typeof(FurgeFutar).Name) ? " < NYERTES >" : "")}");
                        Console.WriteLine($" [TurboTeknos] [{Etterem.Rendelesek.Where(x => x.Status == RendelesStatus.Kiszallitva && x.Futar.GetType() == typeof(TurboTeknos)).Count()}] : " +
                            $"{Math.Round(futarok.Where(x => x.GetType() == typeof(TurboTeknos)).Sum(x => x.OsszesPenz.Sum())) } Ft " +
                            $"\t[{futarok.Where(x => x.GetType() == typeof(TurboTeknos)).Sum(y => y.OsszesPenz.Count())}] " +
                            $"{(Vege().Equals(true) && Nyertes().Equals(typeof(TurboTeknos).Name) ? " < NYERTES >" : "")}");
                    }
                    Console.WriteLine();

                    //FUTAROK
                    foreach (var futar in futarok)
                    {
                        switch (futar.Status)
                        {
                            case FutarStatus.RendelesetSzallit:
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.Write(futar.ToString());
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(futar.Stat());
                                break;
                            default:
                                Console.ForegroundColor = ConsoleColor.Gray;
                                Console.Write(futar.ToString());
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(futar.Stat());
                                break;
                        }
                    }
                    Console.ForegroundColor = ConsoleColor.Green;

                    //ETTEREM ALLAPOTA GLOBALISAN 
                    lock (Etterem.RendelesekLock)
                    {
                        if (Etterem.Rendelesek.Count().Equals(Etterem.MAX) && Util.state==1)
                        {
                            Util.StateTimes.Add(clock.ElapsedMilliseconds / Util.PERC);
                            Util.state = 2;
                        }
                        if (Etterem.Rendelesek.All(x => x.Status.Equals(RendelesStatus.Kiszallitva)) == true && Util.state == 2)
                        {
                            Util.StateTimes.Add(clock.ElapsedMilliseconds / Util.PERC);
                            Util.state = 3;
                        }
                        if (Vege() == true && Util.state == 3)
                        {
                            Util.StateTimes.Add(clock.ElapsedMilliseconds / Util.PERC);
                            Util.state = 4;
                        }
                        Console.Write($"{(Etterem.Rendelesek.Count().Equals(Etterem.MAX) ? $" [ OSSZES ELKESZULT <{Util.StateTimes[0]}.perc> ]" : "")} ");
                        Console.Write($"{(Etterem.Rendelesek.All(x => x.Status.Equals(RendelesStatus.Kiszallitva) == true) ? $" -> [ OSSZES KISZALLITVA <{Util.StateTimes[1]}.perc> ]" : "")} ");
                    }
                    Console.Write($"{(Vege() == true ? $"  -> [ OSSZES FUTAR FIZETVE <{Util.StateTimes[2]}.perc>]" : "")}");
                    Thread.Sleep(refreshRate);
                }
                clock.Stop();
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine(" [:: SZIMULACIO VEGE ::] ");
            }, TaskCreationOptions.LongRunning);

            rendelestKeszit.Start();
            futarTASK.ForEach(x => x.Start());
            naplozo.Start();

            Console.ReadLine();
        }
        public enum RendelesStatus
        {
            FutarraVar,
            SzallitasAlatt,
            Kiszallitva
        }
        public enum FutarStatus
        {
            RendelesreVar,
            RendelesetSzallit,
            Visszauton,

            Pihen
        }
        static class Etterem
        {
            public static int MAX = 10;
            public static object RendelesekLock = new object();
            public static List<Rendeles> Rendelesek = new List<Rendeles>();
            public static bool Aktiv = true;
            public static void RendelestKeszit()
            {
                while (Rendelesek.Count() < MAX)
                {
                    lock (RendelesekLock)
                    {
                        Rendelesek.Add(new Rendeles());
                    }
                    Thread.Sleep(Util.rnd.Next(2 * Util.PERC, 5 * Util.PERC + 1));
                }
                Aktiv = false;
            }
        }
        public class Rendeles
        {
            static int NextId = 1;
            public int Id { get; private set; }
            public int Ertek { get; private set; }
            public double Tavolsag { get; private set; }
            public RendelesStatus Status { get; set; }
            public Futar Futar { get; set; }
            public Rendeles()
            {
                Id = NextId++;
                Ertek = Util.rnd.Next(2000, 10001);
                Tavolsag = Util.rnd.Next(500, 10001);
                Status = RendelesStatus.FutarraVar;
                Futar = null;
            }
            public override string ToString()
            {
                return $" RendelesID: #{Id} ( {Status} )  [ {Tavolsag / 1000} km ] " +
                    $"{ (Status == RendelesStatus.FutarraVar ? "" : $"(FutarID: #{Futar.Id})") } " +
                    $"{(Status == RendelesStatus.Kiszallitva ? $"[{Futar.Kiszallitott.FirstOrDefault(x => x.Value == this).Key}]" : "")}";
            }
        }
        public abstract class Futar
        {
            static List<Futar> OsszesFutar = new List<Futar>();
            static object FutarListLock = new object();
            static bool OsszesPihen = false;
            static public int Hanyadik = 0;
            static object DictLock = new object();
            static public bool OsszesKisszallitva { get; private set; }

            static int NextId = 1;
            public int Id { get; private set; }
            public FutarStatus Status { get; private set; }
            public Rendeles AktualisR { get; set; }
            public double SzallitasiDij { get; set; }
            public int SzallitasiIdo { get; private set; }
            public List<double> OsszesPenz { get; set; }
            public Dictionary<int, Rendeles> Kiszallitott { get; private set; }
            public Futar()
            {
                Id = NextId++;
                Status = FutarStatus.RendelesreVar;
                AktualisR = null;
                OsszesPenz = new List<double>();
                Kiszallitott = new Dictionary<int, Rendeles>();
                OsszesFutar.Add(this);
            }
            public void SzallitasiIdoKalk()
            {
                int km = (int)(AktualisR.Tavolsag / 1000);
                for (int i = 0; i < km; i++)
                {
                    SzallitasiIdo += Util.rnd.Next(2 * Util.PERC, 3 * Util.PERC + 1);
                }
            }
            public override string ToString()
            {
                return $" FutarID: #{Id} [{GetType().Name}] ( {Status} ) {(AktualisR == null ? "" : $" R: #{AktualisR.Id}")}"; 
            }
            public string Stat()
            {
                return $" \n Kiszallitott [ { Kiszallitott.Count()} ] :  { DeliveredIDtoString()} || Összes penz: {OsszesPenz.Sum()} Ft" +
                       $" \n --------------------------------------";
            }
            public string DeliveredIDtoString()
            {
                string toConsole = "";
                foreach (var rendeles in Kiszallitott)
                {
                    toConsole += $"#{rendeles.Value.Id} , ";
                }
                return toConsole;
            }
            protected abstract void SzallitasiDijKalk();
            protected abstract void Valasztas();
            public void Work()
            {
                while (Etterem.Aktiv != false || OsszesKisszallitva != true || OsszesPihen != true)
                {
                    Status = FutarStatus.RendelesreVar; // VARAKOZAS

                    lock (Etterem.RendelesekLock)
                    {
                        Valasztas();
                    }
                    if (AktualisR != null) // HA VOLT ELÉRHETŐ RENDELÉS
                    {
                        AktualisR.Futar = this;
                        Status = FutarStatus.RendelesetSzallit;
                        SzallitasiIdoKalk();
                        Thread.Sleep(SzallitasiIdo); // ODAÚT

                        Thread.Sleep(Util.rnd.Next(2 * Util.PERC, 5 * Util.PERC + 1)); //ATADASI IDŐ
                        SzallitasiDijKalk();
                        AktualisR.Status = RendelesStatus.Kiszallitva;

                        lock (DictLock)
                        {
                            Hanyadik++;
                        }
                        Kiszallitott.Add(Hanyadik, AktualisR);
                        AktualisR = null;

                        Status = FutarStatus.Visszauton;
                        Thread.Sleep(SzallitasiIdo); //VISSZAUT
                        OsszesPenz.Add(SzallitasiDij);
                        if (!Etterem.Aktiv && OsszesKisszallitva != true)
                        {
                            lock (Etterem.RendelesekLock) // lehetne monitort használni
                            {
                                if (Etterem.Rendelesek.All(x => x.Status == RendelesStatus.Kiszallitva))
                                {
                                    OsszesKisszallitva = true;
                                }
                            }
                        }
                    }
                    else // HA NEM VOLT ELÉRHETŐ RENDELÉS
                    {
                        Status = FutarStatus.Pihen;
                        if (Etterem.Aktiv == false && OsszesKisszallitva == true)
                        {
                            lock (FutarListLock)
                            {
                                if (OsszesFutar.All(x => x.Status == FutarStatus.RendelesreVar || x.Status == FutarStatus.Pihen))
                                {
                                    OsszesPihen = true;
                                }
                            }
                        }
                        Thread.Sleep(Util.rnd.Next(Util.PERC, 2 * Util.PERC + 1));
                    }
                }
            }
        }
        public class FurgeFutar : Futar
        {
            protected override void SzallitasiDijKalk()
            {
                SzallitasiDij = 600;
            }

            protected override void Valasztas()
            {
                if (Etterem.Rendelesek.Where(x => x.Status == RendelesStatus.FutarraVar).Count() > 0)
                {
                    AktualisR = Etterem.Rendelesek.Where(x => x.Status == RendelesStatus.FutarraVar).OrderBy(x => x.Tavolsag).First();
                    AktualisR.Status = RendelesStatus.SzallitasAlatt;
                }
            }
        }
        public class TurboTeknos : Futar
        {
            protected override void SzallitasiDijKalk()
            {
                SzallitasiDij = AktualisR.Ertek * 0.05;
                if (AktualisR.Tavolsag / 1000 > 3)
                {
                    SzallitasiDij += ((AktualisR.Tavolsag / 1000) - 3) * 200;
                }
            }
            protected override void Valasztas()
            {
                if (Etterem.Rendelesek.Where(x => x.Status == RendelesStatus.FutarraVar).Count() > 0)
                {
                    AktualisR = Etterem.Rendelesek.Where(x => x.Status == RendelesStatus.FutarraVar).OrderBy(x => x.Tavolsag).Last();
                    AktualisR.Status = RendelesStatus.SzallitasAlatt;
                }
            }
        }
        static class Util
        {
            static public Random rnd = new Random();
            static public int PERC = 1000 / 10; // 1PERC - 60.000 MS
            static public int state = 1;
            public static List<long> StateTimes = new List<long>();
        }
    }
}
