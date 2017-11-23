using System;
using System.Threading;
class Program
{
    // Create a Random Number Generator
    static Random Rand = new Random();
    
    static int MaxCustomers;
    static int MaxBarbers;
    static int waitingChairs;
    static int waitingSeatsAvailable;
    static Semaphore waitingRoom;
    static Semaphore barberChair;
    static Semaphore[] barberPillow;
    static Semaphore[] seatBelt;

    static bool[] barberSleep;
    static bool[] barberOnWork;
    // Are we finished?
    static bool AllDone = false;

    static int chooseBarber()
    {
        int i;
        for(i = 0; i < MaxBarbers; i++)
        {
            if (barberOnWork[i] == false)
                break;
        }
        return i;
    }
    static void Barber(Object number)
    {
        int Number = (int)number;
        while (!AllDone)
        {
            if (waitingSeatsAvailable == waitingChairs)
            {
                barberSleep[Number] = true;
                barberOnWork[Number] = false;
                Console.WriteLine("The barber{0} is sleeping.", Number);                       
            }

            barberPillow[Number].WaitOne();

            
            // Cutting hair for a random amount of time.
            barberSleep[Number] = false;
            barberOnWork[Number] = true;
            Console.WriteLine("The barber{0} is cutting the hair.\n", Number);
            Thread.Sleep(Rand.Next(1,4)*1000);

            
            Console.WriteLine("The barber{0}'s finished cutting hair.", Number);
            seatBelt[Number].Release();          
            
        }
        
    }
    static void Customer(Object number)
    {
        int Number = (int)number;
        int barbernum;

        //Customer's arrive time is random. 
        Thread.Sleep(Rand.Next(1, 5) * 1000);

        Console.WriteLine("Customer {0} has arrived.", Number);

        //Console.WriteLine("{0}.", waitingRoom.WaitOne());
        if (waitingSeatsAvailable == 0)          
        {
            Console.WriteLine("Because there is no available seat in the waiting room, customer {0} leaves the barber shop.", Number);
        }
        else
        {
            waitingRoom.WaitOne();
            waitingSeatsAvailable--;
            Console.WriteLine("Customer {0} entering waiting room", Number);

            
            barberChair.WaitOne();
            barbernum = chooseBarber();

            waitingRoom.Release();
            waitingSeatsAvailable++;

            if (barberSleep[barbernum] == true)
            {
                Console.WriteLine("Barber{0}, customer {1} wants to wake you up!",barbernum, Number);
            }
            else
            {
                Console.WriteLine("Barber{0}, here is your next customer {1}.",barbernum, Number);
            }
            barberPillow[barbernum].Release();

            seatBelt[barbernum].WaitOne();
            barberOnWork[barbernum] = false;
            Console.WriteLine("Customer {0} finished the hair cut and leaves the barber shop.\n", Number);
            barberChair.Release();
           
        }
    }
    static void Main()
    {
        Console.WriteLine("The number of customers : ");
        MaxCustomers = int.Parse(Console.ReadLine());

        Console.WriteLine("The number of barbers : ");
        MaxBarbers = int.Parse(Console.ReadLine());

        Console.WriteLine("The number of waiting room chairs : ");
        waitingChairs = int.Parse(Console.ReadLine());
        waitingSeatsAvailable = waitingChairs;

        //Console.WriteLine("\n");
        

        waitingRoom = new Semaphore(waitingChairs, waitingChairs);
        barberChair = new Semaphore(MaxBarbers, MaxBarbers);       
        barberPillow = new Semaphore[MaxBarbers];        
        seatBelt = new Semaphore[MaxBarbers];
        
        barberSleep = new bool[MaxBarbers];
        barberOnWork = new bool[MaxBarbers];

        Thread[] Barberthreads = new Thread[MaxBarbers];
        for (int i = 0; i < MaxBarbers; i++)
        {
            
            barberPillow[i] = new Semaphore(0, 1);
            seatBelt[i] = new Semaphore(0, 1);
            barberSleep[i] = true;
            barberOnWork[i] = false;

            Barberthreads[i] = new Thread(new ParameterizedThreadStart(Barber));
            Barberthreads[i].Start(i);
        }

        //Console.WriteLine("\n");

        Thread[] Customerthreads = new Thread[MaxCustomers];
        for (int i = 0; i < MaxCustomers; i++)
        {
            Customerthreads[i] = new Thread(new ParameterizedThreadStart(Customer));
            Customerthreads[i].Start(i);
        }
        for (int i = 0; i < MaxCustomers; i++)
        {
            Customerthreads[i].Join();
        }

        AllDone = true;
        Console.WriteLine("Customers all served, barbers are going home.");
       

        // Wait for the Barber's thread to finish before exiting.

        /*for (int i = 0; i < MaxBarbers; i++)
        {
            Barberthreads[i].Join();
        }*/

        
        Console.ReadLine();
    }
}
