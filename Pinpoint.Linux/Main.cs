using Microsoft.Extensions.Configuration;
using Pinpoint.Core;
using Pinpoint.Core.Results;
using Pinpoint.Linux;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace Pinpoint.Plugin.Linux
{
    public class Main
    {
        public static void Main(string[] args)
        {
            var conf = new ConfigurationBuilder().AddJsonFile("file.json").Build;

            var pipe = new PipeReader();
            pipe.Listen();
                 
        }
    }
}