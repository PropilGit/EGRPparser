using System;
using EGRPparser.Infrastructure;

namespace EGRPparser
{
    class Program
    {
        static void Main(string[] args)
        {
            XML_Parser prs = new XML_Parser();
            prs.Parse();

            Console.ReadLine();
        }  
    }
}
