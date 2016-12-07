using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuskarSharp
{
    class Program
    {
        private static Bootstrap bootstrap;

        static void Main(string[] args)
        {
            bootstrap = new Bootstrap();
            bootstrap.SubscribeEvents();
        }

    }
}
