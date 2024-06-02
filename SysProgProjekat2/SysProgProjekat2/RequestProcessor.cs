using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SysProgProjekat2
{
    class RequestProcessor
    {
        private readonly string _rootDirectory;
        private static readonly Cache _cache = new Cache();

        public RequestProcessor(string rootDirectory)
        {
            _rootDirectory = rootDirectory;
        }

        public async Task ProcessRequestAsync(TcpClient client, CancellationToken cancellationToken)
        {
            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream);

            string request = null;
            try
            {
                request = await reader.ReadLineAsync();
            }
            catch (IOException ex)
            {
                Console.WriteLine($"IOException: {ex.Message}");
                await ResponseSender.SendErrorAsync(stream, "Error reading request.");
            }

            if (string.IsNullOrEmpty(request))
            {
                await ResponseSender.SendErrorAsync(stream, "Invalid request received.");
                CloseResources(reader, stream, client);
                return;
            }

            Console.WriteLine($"Zahtev: {request}");

            var stopwatch = Stopwatch.StartNew();

            string[] requestParts = request.Split(' ');
            if (requestParts.Length >= 2)
            {
                string url = requestParts[1];
                if (url.StartsWith("/download/"))
                {
                    string fileName = url.Substring("/download/".Length);
                    await DownloadFileAsync(stream, fileName);
                }
                else
                {
                    string keyword = "";
                    int lastSlashIndex = url.LastIndexOf('/');
                    if (lastSlashIndex != -1)
                    {
                        keyword = url.Substring(lastSlashIndex + 1);
                    }

                    string[] cachedFiles = _cache.GetFiles(keyword);
                    if (cachedFiles != null)
                    {
                        Console.WriteLine($"Fajlovi preuzeti iz keša za kljucnu rec: {keyword}");
                        await ResponseSender.SendResponseAsync(stream, cachedFiles);
                    }
                    else
                    {
                        var files = Directory.EnumerateFiles(_rootDirectory, "*", SearchOption.TopDirectoryOnly)
                            .Where(file => Path.GetFileName(file).Contains(keyword))
                            .ToArray();

                        _cache.SetFiles(keyword, files);
                        await ResponseSender.SendResponseAsync(stream, files);
                    }
                }
            }
            else
            {
                await ResponseSender.SendErrorAsync(stream, "Invalid request format.");
            }
            stopwatch.Stop();
            Console.WriteLine($"Processing time: {stopwatch.ElapsedMilliseconds} ms");
            CloseResources(reader, stream, client);
        }

        private async Task DownloadFileAsync(NetworkStream stream, string fileName)
        {
            string filePath = Path.Combine(_rootDirectory, fileName);
            if (File.Exists(filePath))
            {
                try
                {
                    byte[] fileBytes = await Task.Run(() => File.ReadAllBytes(filePath));
                    StringBuilder responseBuilder = new StringBuilder();
                    responseBuilder.AppendLine("HTTP/1.1 200 OK");
                    responseBuilder.AppendLine($"Content-Disposition: attachment; filename=\"{fileName}\"");
                    responseBuilder.AppendLine("Content-Type: application/octet-stream");
                    responseBuilder.AppendLine($"Content-Length: {fileBytes.Length}");
                    responseBuilder.AppendLine();

                    byte[] headerBytes = Encoding.UTF8.GetBytes(responseBuilder.ToString());
                    await stream.WriteAsync(headerBytes, 0, headerBytes.Length);
                    await stream.WriteAsync(fileBytes, 0, fileBytes.Length);
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"IOException: {ex.Message}");
                    await ResponseSender.SendErrorAsync(stream, "File is in use and cannot be accessed.");
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine($"UnauthorizedAccessException: {ex.Message}");
                    await ResponseSender.SendErrorAsync(stream, "You do not have permission to access this file.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                    await ResponseSender.SendErrorAsync(stream, "An error occurred while accessing the file.");
                }
            }
            else
            {
                await ResponseSender.SendNotFoundAsync(stream);
            }
        }

        private void CloseResources(StreamReader reader, NetworkStream stream, TcpClient client)
        {
            reader?.Close();
            stream?.Close();
            client?.Close();
        }
    }
}
