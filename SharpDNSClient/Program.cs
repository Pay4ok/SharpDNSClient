using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SharpDNSClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter domain name: ");
            string domainName = Console.ReadLine();

            // Put here your IP of DNS server
            byte[] ipaddr = { 8,8,8,8};

            string[] subDomains = domainName.Split('.');

            //Headers of DNS packet
            byte[] bytePayloadStart = { 0x32, 0x34, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
            byte[] bytePayloadEnd = { 0x00, 0x00, 0x01, 0x00, 0x01 };
            byte[] bytePayloadDomain = { };

            //Сonverting the domain name to the correct format
            //For example www.google.com ==
            //3(symbols before dot) 119 119 119
            //6(symbols before dot) 103 111 111 103 108 101
            //3(symbols before dot) 99 111 109
            foreach (string s in subDomains)
            {
                byte[] temp = (new byte[1] { (byte)s.Length }).Concat(Encoding.Default.GetBytes(s)).ToArray();
                bytePayloadDomain = bytePayloadDomain.Concat(temp).ToArray();
            }

            byte[] bytePayload = bytePayloadStart.Concat(bytePayloadDomain).Concat(bytePayloadEnd).ToArray();

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint endPoint = new IPEndPoint(new IPAddress(ipaddr), 53);

            Console.WriteLine($"Payload in ASCII: {Encoding.ASCII.GetString(bytePayload)}");
            {
                string response = "";
                bytePayload.ToList().ForEach(e => response += e + " ");
                Console.WriteLine($"Payload in HEX: {response}");
            }
            Console.WriteLine("\nConnecting");
            try
            {
                socket.Connect(endPoint);
                socket.Send(bytePayload);
                Console.WriteLine("Sending completed\n");

                byte[] buffer = new byte[1024];
                int bytesReceived = socket.Receive(buffer);
                Array.Resize<byte>(ref buffer, bytesReceived);
                byte[] resIP = new byte[4];
                Array.Copy(buffer, buffer.Length - 4, resIP, 0, 4); // Copy 4 last bytes from buffer to resIP, it will be an IP adress of your domain
                Array.Resize<byte>(ref buffer, bytesReceived - 4);

                {
                    string response = "";
                    buffer.ToList().ForEach(e => response += e + " ");
                    Console.Write("Recieved in HEX: " + response);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                {
                    string str = "";
                    resIP.ToList().ForEach(e => str += e + " ");
                    Console.WriteLine(str);
                }
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Recieved in ASCII: " + Encoding.ASCII.GetString(buffer));

                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
            }
        }
    }
}
