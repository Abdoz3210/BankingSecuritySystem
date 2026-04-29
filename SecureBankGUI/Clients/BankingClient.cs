using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SecureBankGUI.Clients;

public class BankingClient
{
    private Socket? _socket;
    private bool _connected = false;
    public string WelcomeMessage = "";

    public string Connect(string ip, int port)
    {
        try
        {
            _socket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp
            );
            _socket.Connect(new IPEndPoint(IPAddress.Parse(ip), port));

            // Send mode
            _socket.Send(Encoding.ASCII.GetBytes("BANKING"));

            // Receive welcome — store separately
            byte[] buffer = new byte[1024];
            int bytes = _socket.Receive(buffer);
            WelcomeMessage = Encoding.ASCII.GetString(buffer, 0, bytes);
            _connected = true;

            return WelcomeMessage;
        }
        catch (Exception ex)
        {
            return "ERROR: " + ex.Message;
        }
    }

    public string SendCommand(string command)
    {
        if (!_connected || _socket == null)
            return "ERROR: Not connected.";
        try
        {
            // Send command
            _socket.Send(Encoding.ASCII.GetBytes(command));

            // Wait for reply to THIS command specifically
            byte[] buffer = new byte[1024];
            int bytes = _socket.Receive(buffer);
            return Encoding.ASCII.GetString(buffer, 0, bytes);
        }
        catch (Exception ex)
        {
            _connected = false;
            return "ERROR: " + ex.Message;
        }
    }

    public void Disconnect()
    {
        if (_connected && _socket != null)
        {
            try
            {
                SendCommand("exit");
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
            catch { }
            _connected = false;
        }
    }

    public bool IsConnected => _connected;
}