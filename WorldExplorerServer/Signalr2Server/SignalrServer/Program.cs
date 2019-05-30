using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SignalrServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // http://*:8080
            using (WebApp.Start<Startup>("http://*:8080"))
            {
                Console.WriteLine("Server running at http://localhost:8080/");
                Console.ReadLine();
            }


        }
    }
}
