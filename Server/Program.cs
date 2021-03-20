using Commands;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static Random rnd = new Random();
        static void Main(string[] args)
        {
            const int port = 8080;
            IPAddress address = IPAddress.Parse("127.0.0.1");
            TcpListener server = new TcpListener(address, port);
            bool hasPlayer = false;

            PlayerInfo player1 = null;
            try
            {
               server.Start();

                while (true)
                {
                    try
                    {
                        TcpClient client = server.AcceptTcpClient();


                        BinaryFormatter formatter = new BinaryFormatter();
                        var command = (ClientCommand)formatter.Deserialize(client.GetStream());


                        switch (command.Type)
                        {

                            case CommandType.START:
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"Request to start a new game from {command.Nickname}");

                                if (!hasPlayer)
                                {

                                    player1 = new PlayerInfo()
                                    {
                                        Nickname = command.Nickname,
                                        TcpClient = client,
                                        IsX = Convert.ToBoolean(rnd.Next(2))
                                    };

                                    player1.SendWaitCommand();

                                    hasPlayer = true;
                                }

                                else
                                {

                                    PlayerInfo player2 = new PlayerInfo()
                                    {
                                        Nickname = command.Nickname,
                                        TcpClient = client,
                                        IsX = !player1.IsX
                                    };


                                    player1.SendStartCommand(player2.Nickname);
                                    player2.SendStartCommand(player1.Nickname);


                                    Task.Run(() => player1.StartSession(player2));
                                    Task.Run(() => player2.StartSession(player1));


                                    hasPlayer = false;
                                }
                                break;
                            default:
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine($"Unknown command from {command.Nickname}");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
            finally
            {
            server.Stop();

            }
        }
    }
}

