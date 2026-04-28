using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
namespace SecureBankGUI.Clients;

class client
{
    static void Main(string[] args)
    {
        
        
        IPAddress ip = IPAddress.Parse("127.0.0.1");

        IPEndPoint endPoint = new IPEndPoint(ip, 8000);

        Socket clientSocket = new Socket(
            addressFamily: AddressFamily.InterNetwork,
            socketType: SocketType.Stream,
            protocolType: ProtocolType.Tcp

        );

        clientSocket.Connect(endPoint);

        Console.WriteLine("Connect to server successfully");

        
        byte[] receiveBuffer = new byte[1024];
        int bytesReceived = clientSocket.Receive(receiveBuffer);
        string messageReceived = Encoding.ASCII.GetString(receiveBuffer, 0, bytesReceived);
        Console.WriteLine("Server says: " + messageReceived);

        Console.WriteLine("=== SecureBank Client ===");
        Console.WriteLine("1 - Banking");
        Console.WriteLine("2 - Support Chat");
        Console.WriteLine("3 - Live Exchange Rates");
        HandleModes(clientSocket);

        while (true)
        {

            // Console.WriteLine("You: ");
            byte[] serverReplyBuffer = new byte[1024];
            int serverReplyBytes = clientSocket.Receive(serverReplyBuffer);
            string serverReply = Encoding.ASCII.GetString(serverReplyBuffer, 0, serverReplyBytes);

            Console.WriteLine("Server says: " + serverReply);

            string messageToSend = Console.ReadLine();
            byte[] sendBuffer = Encoding.ASCII.GetBytes(messageToSend);
            clientSocket.Send(sendBuffer);
            Console.WriteLine("client sent: " + messageToSend);


            if (messageToSend.ToLower() == "exit")
            {
                break;
            }

        }


        clientSocket.Shutdown(SocketShutdown.Both);
        clientSocket.Close();
        Console.WriteLine("Disconnected.");
        Console.ReadLine();

    }





    static void HandleModes(Socket clientSocket)
    {
        Console.Write("Select mode: ");
        string choice = Console.ReadLine();

        string mode = "";
        if (choice == "1")
        {
            mode = "BANKING";
            Console.WriteLine("");
        }
        else if (choice == "2") mode = "CHAT";
        else if (choice == "3")
        {
            GetLiveRates();
            HandleModes(clientSocket);
        } // done, no TCP needed
        // else if (choice.ToLower() == "exit") mode = "exit";
        else
        {
            Console.WriteLine("Invalid choice. try again....");
            HandleModes(clientSocket);
        }
        clientSocket.Send(Encoding.ASCII.GetBytes(mode));
    }
    static void GetLiveRates()
    {
        Console.WriteLine("/nFetchng live exchange rates.../n");
        Socket udpSocket = new Socket(
            AddressFamily.InterNetwork,
            SocketType.Dgram,
            ProtocolType.Udp
        );

        IPEndPoint serverEndPoint = new IPEndPoint(
            IPAddress.Parse("127.0.0.1"),
            8001
        );



        byte[] request = Encoding.ASCII.GetBytes("GET_RATES");

        udpSocket.SendTo(request, serverEndPoint);

        byte[] buffer = new byte[1024];
        EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
        int bytesReceived = udpSocket.ReceiveFrom(buffer, ref remoteEP);

        string rates = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
        Console.WriteLine(rates);

        udpSocket.Close();

        // Console.ReadLine();
    }
}