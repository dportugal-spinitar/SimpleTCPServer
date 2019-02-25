# SimpleTCPServer
A way to create a simple C# TCP server with events and with useful methods

##How to use
The main namespace is `SimpleTCPServer.Core`. This provides the main class, `TCPServer` for you to use. Here is an example of how to use this class.
```
//Server Side
using SimpleTCPServer.Core;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;

namespace TestServerNamespace
{
  class Program
  {
    static void Main()
    => new Program().MainSync();
    public void MainSync()
    {
      TCPServer server = new TCPServer("192.168.1.103:8989", new TCPServerConfig //Create the TCPServer
      {
        BytesSize = 1024 //The default option is 1024
      }); //Do any ip address
      server.OnClientConnect += ClientConnected; //Set the method for when a client connects
      server.Start(); //Make sure you do TCPServer#Start(); so it starts listening
      Thread.Sleep(-1); //Make sure you do Thread.Sleep(-1); or Task.Delay(-1); on a console application so the program doesn't stop
    }
    public Task ClientConnected(TcpClient client)
    {
      System.Console.WriteLine("A client has connected!");
      return Task.CompletedTask;
    }
  }
}
//Client Side
using System.Net.Sockets;

namespace TestClientNamespace
{
  class Program
  {
    static void Main()
    {
      TcpClient client = new TcpClient();
      client.Connect("192.168.1.103", 8989);
    }
  }
}
/* Output
* A client has connected!
*/
```
