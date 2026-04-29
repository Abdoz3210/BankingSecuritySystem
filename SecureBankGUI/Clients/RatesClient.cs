using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SecureBankGUI.Clients;

public class RatesClient
{
    public string GetRates(string ip, int port)
    {
        Socket? udpSocket = null;
        try
        {
            udpSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Dgram,
                ProtocolType.Udp
            );

            udpSocket.ReceiveTimeout = 3000;

            IPEndPoint serverEndPoint = new IPEndPoint(
                IPAddress.Parse(ip), port
            );

            udpSocket.SendTo(
                Encoding.ASCII.GetBytes("GET_RATES"),
                serverEndPoint
            );

            byte[] buffer = new byte[1024];
            EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            int bytes = udpSocket.ReceiveFrom(buffer, ref remoteEP);

            return Encoding.ASCII.GetString(buffer, 0, bytes);
        }
        catch (Exception ex)
        {
            return "ERROR: " + ex.Message;
        }
        finally
        {
            udpSocket?.Close();
        }
    }
}
