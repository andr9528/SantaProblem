using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SantaClauseProblem
{
    class Program
    {
        int elfCount = 0;
        int reindeerCount = 0;

        bool santaSleep = true; // true = santa is asleep, false = santa is awake.
        bool elfDoor = true; // true = door is open, false = door is closed.

        Semaphore reindeerSem = new Semaphore(9,9);
        Semaphore elfSem = new Semaphore(3,3);

        Mutex m = new Mutex();

        Random random = new Random();

        List<Thread> ElfThreads = new List<Thread>();
        List<Thread> ReindeerThreads = new List<Thread>();

        static void Main(string[] args)
        {
            Program Run = new Program();
            Run.Run();
        }

        private void Run()
        {
            Thread SantaThread = new Thread(Santa);
            
            int selector;
            int delay;

            SantaThread.Start();

            while (true)
            {
                selector = random.Next(0, 1000);

                if (selector < 500)
                {
                    Thread ElfThread = new Thread(Elf);
                    ElfThreads.Add(ElfThread);
                    ElfThread.Start();
                }
                else
                {
                    Thread ReindeerThread = new Thread(Reindeer);
                    ReindeerThreads.Add(ReindeerThread);
                    ReindeerThread.Start();
                }
                Thread.Sleep(random.Next(1000, 10000));
            }


        }
        private void Santa()
        {
            while (true)
            {
                while (santaSleep == false)
                {
                    Console.WriteLine("Santa is Awake.");
                    m.WaitOne();

                    if (reindeerCount == 9)
                    {
                        Console.WriteLine("The 9 Reindeers are Released.");
                        reindeerCount = 0;
                        reindeerSem.Release(9);
                        for (int i = 0; i < 9; i++)
                        {
                            ReindeerThreads.RemoveAt(0);
                        }
                        Console.WriteLine("{0} Reindeers Are inside waiting... {1} Reindeers exist In Total.", reindeerCount, ReindeerThreads.Count);
                        // prepSleigh();
                        
                    }
                    else
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            Thread.Sleep(random.Next(2000, 5000));
                            elfSem.Release(1);
                            HelpElf();
                        }
                    }
                    m.ReleaseMutex();
                    
                    if (reindeerCount != 9 && elfCount != 3)
                    {
                        Console.WriteLine("Santa went to Sleep");
                        santaSleep = true;
                    }
                }
                Thread.Sleep(1500);
            }
        }

        private void HelpElf()
        {
            m.WaitOne();
            elfCount--;
            
            ElfThreads.RemoveAt(0);
            Console.WriteLine("An Elf got help from Santa.");
            Console.WriteLine("{0} Elves Are inside waiting... {1} Elves exist In Total.", elfCount, ElfThreads.Count);
            if (elfCount == 0)
            {
                Console.WriteLine("The Elf Opens the door before leaving, Letting a new group enter.");
                elfDoor = true;
            }
            m.ReleaseMutex();
        }

        private void Elf()
        {
            Console.WriteLine("A New Elf Arrives.");
            
            m.WaitOne();
            elfDoor = false;

            Console.WriteLine("An Elf Enter & Closes the Door.");
            elfCount++;
            
            Console.WriteLine("{0} Elves Are inside waiting... {1} Elves exist In Total.", elfCount, ElfThreads.Count);

            if (elfCount == 3)
            {
                Console.WriteLine("The Elf Leaves the door closed and wakes Santa.");
                santaSleep = false;
            }
            else
            {
                Console.WriteLine("The Elf Opens the Door for the next one.");
                elfDoor = true;
            }
            m.ReleaseMutex();

            elfSem.WaitOne();
        }
        private void Reindeer()
        {
            Console.WriteLine("A New Reindeer Arrives.");
            reindeerSem.WaitOne();
            m.WaitOne();

            reindeerCount++;
            Console.WriteLine("{0} Reindeers Are inside waiting... {1} Reindeers exist In Total", reindeerCount, ReindeerThreads.Count);

            if (reindeerCount == 9)
            {
                santaSleep = false;
            }

            m.ReleaseMutex();
            
        }
    }
}
