using System;
using System.Net;
using System.Net.Sockets;
using System.Text;


class Server
{
    static int balance = 1000;
    static object balanceLock = new object();
    static void Main(string[] args)
    {

        Thread udpThread = new Thread(startUDPListener);
        udpThread.IsBackground = true;
        udpThread.Start();


        IPAddress ip = IPAddress.Parse("127.0.0.1");
        IPEndPoint endPoint = new IPEndPoint(ip, 8000);

        Socket serverSocket = new Socket(
            AddressFamily.InterNetwork,
            SocketType.Stream,
            ProtocolType.Tcp
        );

        serverSocket.Bind(endPoint);
        serverSocket.Listen(5);


        Console.WriteLine("[TCP] Server waiting on port 8000...");
        Console.WriteLine("[UDP] Listener running on port 8001...");
        Console.WriteLine("Waiting for clients...\n");




        while (true)
        {
            Socket clientSocket = serverSocket.Accept();

            Console.WriteLine("New Client connected! Handing to thread.... ");

            ThreadPool.QueueUserWorkItem(HandleClient, clientSocket);

        }


    }


    static void HandleClient(object obj)
    {
        Socket clientSocket = (Socket)obj;
        int threadId = Thread.CurrentThread.ManagedThreadId;

        Console.WriteLine("[Tresad " + threadId + "] Handling new client...");
        clientSocket.Send(Encoding.ASCII.GetBytes("Welcome client on thread: " + threadId));

        try
        {
            byte[] modeBuffer = new byte[1024];
            int modebytes = clientSocket.Receive(modeBuffer);
            string mode = Encoding.ASCII.GetString(
                modeBuffer, 0, modebytes
            ).ToLower().Trim();
            Console.WriteLine("[Thread " + threadId + "] Mode selected: " + mode);


            if (mode == "banking")
                HandleBanking(clientSocket);
            else if (mode == "chat")
                HandleChat(clientSocket);
            else
                clientSocket.Send(Encoding.ASCII.GetBytes("Error: Unknown mode."));

        }
        catch (Exception ex)
        {
            Console.WriteLine("[Thread " + threadId + " ] disconnectec unexpectedly: " + ex.Message);
        }
        finally
        {
            try
            {
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
            catch { }
            Console.WriteLine("[Thread " + threadId + "] Client session ended. ");
        }

    }

    static void HandleBanking(Socket clientSocket)
    {

        clientSocket.Send(Encoding.ASCII.GetBytes("BANKING MODE: Starting balance $" + balance +"/n"+"You can DEBOSIT:--- | WITHDRAW:--- | BALANCE | EXIT"));
        while (true)
        {
            byte[] buffer = new byte[1024];
            int bytesReceived = clientSocket.Receive(buffer);
            string message = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
            if (message.ToLower() == "exit")
            {
                clientSocket.Send(Encoding.ASCII.GetBytes("Session ended. \nFinal BALANCE: $" + balance));
                break;
            }
            string reply = ProcessCommand(message);
            clientSocket.Send(Encoding.ASCII.GetBytes(reply));

        }

    }

    static void HandleChat(Socket clientSocket)
    {
        clientSocket.Send(Encoding.ASCII.GetBytes("CHAT MODE: Welcome to SecureBank Support!"));

        while (true)
        {
            byte[] buffer = new byte[1024];
            int bytesReceived = clientSocket.Receive(buffer);
            string message = Encoding.ASCII.GetString(
                buffer, 0, bytesReceived
            );

            Console.WriteLine("[CHAT] " + message);

            if (message.ToLower() == "exit")
            {
                clientSocket.Send(Encoding.ASCII.GetBytes("Goodbye!"));
                break;
            }
            string reply = GetBotReply(message);
            clientSocket.Send(Encoding.ASCII.GetBytes(reply));
        }
    }

    static void startUDPListener()
    {
        Socket udpSocket = new Socket(
            AddressFamily.InterNetwork,
            SocketType.Dgram,
            ProtocolType.Udp
        );
        IPEndPoint udpEndpoint = new IPEndPoint(IPAddress.Any, 8001);
        udpSocket.Bind(udpEndpoint);

        while (true)
        {
            byte[] buffer = new byte[1024];
            EndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);

            int bytesReceived = udpSocket.ReceiveFrom(buffer, ref clientEndPoint);
            string request = Encoding.ASCII.GetString(buffer, 0, bytesReceived);


            if (request.ToLower() == "get_rates")
            {
                Console.WriteLine("Client requset GET_RATES");
                string rates = BuildRates();
                udpSocket.SendTo(Encoding.ASCII.GetBytes(rates), clientEndPoint);
            }
        }
    }


    static string GetBotReply(string message)
    {
        string msg = message;
        if (msg.ToLower().Contains("help"))
        {
            return "I can help! Try asking about: hours, loan, balance, or transfer.";
        }
        else if (msg.ToLower().Contains("hours"))
        {
            return "SecureBank is open Sunday to Thursday, 9:00 AM to 5:00 PM.";
        }
        else if (msg.ToLower().Contains("loan"))
        {
            return "We offer personal loans starting at 8% interest. Visit any branch to apply.";
        }
        else if (msg.ToLower().Contains("balance"))
        {
            return "To check your balance, please use the Banking module or visit a branch.";
        }
        else if (msg.ToLower().Contains("transfer"))
        {
            return "Transfers can be made via the Banking module. Minimum transfer is $10.";
        }
        else
        {
            return "I didn't understand that. Type 'help' to see what I can assist with.";
        }

    }
    static string ProcessCommand(string message)
    {
        string[] parts  = message.Split(':');
        if (parts.Length != 2)
        {
            return "ERRPR: Use DEPOSIT:100 or WITHDRAW:50";
            
        }

        string commands = parts[0].ToUpper();
        int amount;

        if (!int.TryParse(parts[1].Trim(), out amount))
            return "ERROR: Amount must be a number";

        if (amount <= 0)
            return "ERROR: Amount must be greater than Zero";

        lock (balanceLock)
        {
            if (commands == "DEPOSIT")
            {
                balance += amount;
                return "SUCCSS: Deposited $" + amount + "> New balance: $" + balance;
            }
            else if (commands == "WITHDRAW")
            {
                if (amount > balance)
                    return "ERROR: Insufficient funds. Balance: $"
                        + balance;
                balance -= amount;
                return "SUCCESS: Withdrew $" + amount +
                    ". New balance: $" + balance;
            }
            else if (commands == "BALANCE")
            {
                return "INFO: Current balance: $" + balance;
            }
            else if (commands == "EXIT")
            {
                return "exit";
            }
            else
            {
                return "ERROR: Unknown command.";
            }
        }


        return "hjhjh";
    }
    static string BuildRates()
    {
        Random rnd = new Random();
        return string.Format(
            "=== LIVE RATES ===\n" +
            "USD: {0:F2} EGP\n" +
            "EUR: {1:F2} EGP\n" +
            "GBP: {2:F2} EGP\n" +
            "SAR: {3:F2} EGP\n" +
            "==================",
            48.5 + rnd.NextDouble(),
            52.3 + rnd.NextDouble(),
            61.1 + rnd.NextDouble(),
            12.9 + rnd.NextDouble()
        );
    }

    
}