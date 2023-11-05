using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinpoint.Linux
{
    public class PipeReader
    {

        public void Listen()
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
                    string request = "Hello, Server!";
                    Console.WriteLine($"Sending request: {request}");
                    writer.WriteLine(request);
                    writer.Flush();

                    // Read the response from the server
                    string response = reader.ReadLine();
                    Console.WriteLine($"Received response: {response}");
                }

                


                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
