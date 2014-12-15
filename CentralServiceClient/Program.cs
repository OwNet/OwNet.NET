using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralServiceClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Jobs.StartJobs();
            string cmd = "";
            while ((cmd = Console.ReadLine()) != "exit")
            {

            }
            Jobs.EndJobs();
        }
    }
}
