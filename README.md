ldsocket
========

Class which makes communication between two C# application in network easy.


REQUIREMENTS
------------

.NET 2.0 or higher


INSTALATION
-----------

Add new class to your project, name it `LDSocket.cs` and paste code from `LDSocket.cs`


EXAMPLE - SERVER
----------------

    using LDSocket;
    .....
    
    namespace Example
    {
      public partial class MainWindow : Window
      {
        LDSocket.SocketServer server;
      
        public MainWindow()
        {
          server = new LDSocket.SocketServer();
          server.DataReceived += new SocketServer.DataReceivedHandler(server_DataReceived);
        }
    
        void server_DataReceived(string data, int index) 
        {
          //data received from client and index
        }
        
        void sendMessage(string msg) 
        {
          server.sendMessage(msg);    //sends message to all clients
          server.sendMessage(msg, 2); //sends message only to client with index 2
        }
      }
    }
