using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omnia.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            var runtime = Hosting.Omnia.CreateRuntime();
            runtime.ExecuteFile("test.om");
        }
    }
}
