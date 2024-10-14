using System.Net;
using System.Net.Sockets;

//client ekrani 10 saniyeden bir screenshot edib servere gonderir.
//server client uchun folder , onun ichirisine download edir.

TcpClient listener;
var ip = IPAddress.Parse("127.0.0.1");
var port = 64405;
var endPoint = new IPEndPoint(ip, port);
var server = new TcpListener(endPoint);
try
{
    server.Start();
    Console.WriteLine("Server Started...");
    while (true)
    {
        listener = server.AcceptTcpClient();
        if (listener != null)
        {
            Console.WriteLine($"{listener.Client.RemoteEndPoint} Connected.");
            _ = Task.Run(() => AcceptClient(listener));
        }
        else Console.WriteLine("listener is Null!");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Have Problem: {ex.Message}");
}

static void AcceptClient(TcpClient client)
{
    var adresss = client.Client.RemoteEndPoint as IPEndPoint;
    if (adresss != null)
    {
        string clientFolder = $"C:\\Users\\ASUS\\Desktop\\{adresss.Address}";
        if (!Directory.Exists(clientFolder))
        {
            Directory.CreateDirectory(clientFolder);
        }

        using (var stream = client.GetStream())
        {
            while (true)
            {
                var lengthBuffer = new byte[5];
                int bytesRead = stream.Read(lengthBuffer, 0, lengthBuffer.Length);
                if (bytesRead == 0) break;

                int length = BitConverter.ToInt32(lengthBuffer, 0);
                var buffer = new byte[length];

                int totalRead = 0;
                while (totalRead < length)
                {
                    int read = stream.Read(buffer, totalRead, length - totalRead);
                    if (read == 0) break;
                    totalRead += read;
                }

                var path = Path.Combine(clientFolder, $"screenshot_{DateTime.Now:HH.mm.ss}.png");
                using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    fileStream.Write(buffer, 0, length);
                }
                Console.WriteLine($"Photo Saved: {path}");
            }
        }
    }
    else Console.WriteLine("Adress Can not be Null");
    client.Close();
}
