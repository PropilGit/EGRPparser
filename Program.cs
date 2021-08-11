using System;
using EGRPparser.Infrastructure;

namespace EGRPparser
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser prs = new Parser();

            prs.Parse();

            Console.ReadLine();
        }  
    }
}
