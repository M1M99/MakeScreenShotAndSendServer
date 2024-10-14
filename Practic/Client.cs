using System.Drawing;
using System.Net;
using System.Net.Sockets;

var a = 0;
var ip = IPAddress.Parse("127.0.0.1");
var port = 64405;
var ep = new IPEndPoint(ip, port);
var client = new TcpClient();

try
{
    client.Connect(ep);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Server Connected.");
    Console.WriteLine("Press Any Key For Cancel...And Wait 10 Second\n");
    Console.ResetColor();
    //while (a < 5)
    //{
    //    TakeScreenShotAndSend(client);
    //    a++;
    //    Thread.Sleep(5000);
    //}
    ConsoleKeyInfo rK = new ConsoleKeyInfo();
    var cancellationSource = new CancellationTokenSource();
    var cancellationToken = cancellationSource.Token;
    _ = Task.Run(() =>
    {
        rK = Console.ReadKey();
        cancellationSource.Cancel();
    });

    while (true)
    {
        if (cancellationToken.IsCancellationRequested) // threadsleep olduguna gore 10 saniye gozdedmek lazimci cancel etmek ucun
        {
            Console.WriteLine($"You Pressed {rK.Key}\nProcess Ended");
            client.Close();
            client.Dispose();
            break;
        }

        var task = Task.Run(() => { TakeScreenShotAndSend(client, cancellationToken); });
        Thread.Sleep(10000);
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
finally
{
    client.Close();
}

static void TakeScreenShotAndSend(TcpClient client, CancellationToken t)
{
    using (var bitmap = new Bitmap(1920, 1080))
    using (var gBitMap = Graphics.FromImage(bitmap))
    {
        gBitMap.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
        using (var ms = new MemoryStream())
        {
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            byte[] imageBytes = ms.ToArray();

            var lengthBytes = BitConverter.GetBytes(imageBytes.Length);
            var networkStream = client.GetStream();

            networkStream.Write(lengthBytes, 0, lengthBytes.Length);
            networkStream.Write(imageBytes, 0, imageBytes.Length);
            networkStream.Flush();
        }
        t.ThrowIfCancellationRequested();

        Console.WriteLine($"ScreenShot Sended");
    }
}
