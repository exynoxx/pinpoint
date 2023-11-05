using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pinpoint.Core.Results;
using Pinpoint.Core;
using System.Reflection;
using System.Threading;

namespace Pinpoint.Linux
{
    public class PipeReader
    {

        private readonly PluginEngine _pluginEngine;

        public PipeReader(PluginEngine pluginEngine)
        {
            _pluginEngine = pluginEngine;
        }

        public async Task ListenAsync()
        {

            string pipeName = "df";

            try
            {
                using var pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
                pipeClient.Connect();

                using var writer = new StreamWriter(pipeClient);
                using var reader = new StreamReader(pipeClient);
                while (true)
                {
                    string response = reader.ReadLine();

                    var query = new Query(response.Trim());
                    if (query.IsEmpty) continue;

                    await foreach (var result in _pluginEngine.RunPlugins(CancellationToken.None, query))
                    {
                        writer.WriteLine(result.Title); //TODO fix
                    }
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
    }
}
