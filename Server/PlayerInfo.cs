using Commands;
using System;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace Server
{
    public class PlayerInfo
    {
        public TcpClient TcpClient { get; set; }
        public string Nickname { get; set; }
        public bool IsX { get; set; }


        private void SendCommand(ServerCommand command)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(TcpClient.GetStream(), command);
        }

        public void SendCloseCommand()
        {
            SendCommand(new ServerCommand(CommandType.CLOSE));
        }
        public void SendWaitCommand()
        {
            SendCommand(new ServerCommand(CommandType.WAIT));
        }
        public void SendStartCommand(string opponentName)
        {
            ServerCommand command = new ServerCommand(CommandType.START)
            {
                IsX = this.IsX,
                OpponentName = opponentName
            };

            SendCommand(command);
        }
        public void SendMoveCommand(CellCoord moveCoord)
        {
            ServerCommand command = new ServerCommand(CommandType.MOVE)
            {
                MoveCoord = moveCoord
            };

            SendCommand(command);
        }
        public void SendDrawCommand()
        {
            ServerCommand command = new ServerCommand(CommandType.DRAW);
            SendCommand(command);
        }
        public void SendLoseCommand(string opponentName)
        {
            ServerCommand command = new ServerCommand(CommandType.LOSE) { OpponentName = opponentName };


            SendCommand(command);
        }

        public ClientCommand ReceiveCommand()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return (ClientCommand)formatter.Deserialize(TcpClient.GetStream());
        }


        public void StartSession(PlayerInfo opponent)
        {
            bool isExit = false;
            while (!isExit)
            {

                ClientCommand command = ReceiveCommand();


                switch (command.Type)
                {

                    case CommandType.MOVE:
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"Move on {command.MoveCoord} from {command.Nickname}");
                        opponent.SendMoveCommand(command.MoveCoord);

                        break;
                    case CommandType.WIN:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"{command.Nickname} win!");
                        opponent.SendLoseCommand(command.Nickname);
                        break;

                    case CommandType.DRAW:
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine($"Draw!");
                        opponent.SendDrawCommand();
                        break;

                    case CommandType.CLOSE:
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine($"Close command from {command.Nickname}");

                        isExit = true;
                        break;

                    case CommandType.EXIT:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Exit command from {command.Nickname}");

                        opponent.SendCloseCommand();

                        isExit = true;
                        break;
                }
            }
        }
    }
}
