using System;
using System.IO;
using EGRPparser.Infrastructure;

namespace EGRPparser
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser prs = new Parser();
            prs.Parse();
            

            //ReadSourceHTML("data.txt");
        }  
    }
}
