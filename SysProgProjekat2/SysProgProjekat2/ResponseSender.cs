using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SysProgProjekat2
{
    static class ResponseSender
    {
        public static async Task SendResponseAsync(NetworkStream stream, string[] files)
        {
            StringBuilder responseBuilder = new StringBuilder();
            responseBuilder.AppendLine("HTTP/1.1 200 OK");
            responseBuilder.AppendLine("Content-Type: text/html");
            responseBuilder.AppendLine();

            if (files.Length > 0)
            {
                responseBuilder.AppendLine("<ul>");
                foreach (string file in files)
                {
                    string fileName = Path.GetFileName(file);
                    responseBuilder.AppendLine($"<li><a href=\"http://localhost:5050/download/{fileName}\">{fileName}</a></li>");
                }
                responseBuilder.AppendLine("</ul>");
            }
            else
            {
                responseBuilder.AppendLine("<p>Nijedan fajl nije pronadjen.</p>");
            }

            byte[] responseBytes = Encoding.UTF8.GetBytes(responseBuilder.ToString());
            await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
        }

        public static async Task SendNotFoundAsync(NetworkStream stream)
        {
            StringBuilder responseBuilder = new StringBuilder();
            responseBuilder.AppendLine("HTTP/1.1 404 Not Found");
            responseBuilder.AppendLine("Content-Type: text/html");
            responseBuilder.AppendLine();
            responseBuilder.AppendLine("<p>Fajl nije pronadjen.</p>");

            byte[] responseBytes = Encoding.UTF8.GetBytes(responseBuilder.ToString());
            await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
        }

        public static async Task SendErrorAsync(NetworkStream stream, string errorMessage)
        {
            StringBuilder responseBuilder = new StringBuilder();
            responseBuilder.AppendLine("HTTP/1.1 500 Internal Server Error");
            responseBuilder.AppendLine("Content-Type: text/html");
            responseBuilder.AppendLine();
            responseBuilder.AppendLine($"<p>{errorMessage}</p>");

            byte[] responseBytes = Encoding.UTF8.GetBytes(responseBuilder.ToString());
            await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
        }
    }
}
