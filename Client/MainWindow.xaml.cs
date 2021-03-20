using Commands;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Client
{

    public partial class MainWindow : Window
    {
        int port = 8080;
        IPAddress address = IPAddress.Parse("127.0.0.1");

        TcpClient client;
        const int SIZE = 3;
        char[,] field = new char[SIZE, SIZE];
        public bool IsX { get; set; }
        public char Symbol => IsX ? 'X' : 'O';
        public char OpponentSymbol => IsX ? 'O' : 'X';
        public string Nickname { get; set; }
        public string OpponentNickname { get; set; }
        public bool IsPlaying { get; set; }
        private bool isMoving = false;
       
        public bool IsMoving
        {
            get { return isMoving; }
            set
            {
                isMoving = value;


                var brush = isMoving ? (Brushes.White) : Brushes.Gray;
                foreach (Border item in fieldGrid.Children.OfType<Border>())
                {
                    item.Background = brush;
                }
            }
        }
          public bool IsDraw()
        {
            int count = 0;
            if (Symbol == 'X')
            {


                for (int i = 0; i < SIZE; i++)
                {
                    for (int j = 0; j < SIZE; j++)
                    {
                        if (field[i, j] == Symbol)
                            count++;
                    }
                }
                if (count == 5)
                    return true;
            }
            return false;
               
        }
        public bool IsWin()
        {
            int count1 = 0;
            int count2 = 0;


            for (int i = 0; i < SIZE; i++)
            {
                if (field[i, i] == Symbol)
                    count1++;
                if (field[i, SIZE - i - 1] == Symbol)
                    count2++;
            }
            if (count1 == SIZE || count2 == SIZE)
                return true;
            count1 = 0;
            count2 = 0;
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = 0; j < SIZE; j++)
                {
                    if (field[i, j] == Symbol)
                        count1++;
                    if (field[j, i] == Symbol)
                        count2++;
                }
                if (count1 == SIZE || count2 == SIZE)
                    return true;
                count1 = 0;
                count2 = 0;
            }


            return false;
        }
        public MainWindow()
        {
            InitializeComponent();

            IsPlaying = false;
            IsMoving = false;
        }


        private Task SendCommand(ClientCommand command)
        {

            return Task.Run(() =>
            {

                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(client.GetStream(), command);
            });
        }

        private Task<ServerCommand> ReceiveCommand()
        {
            return Task.Run(() =>
            {

                BinaryFormatter formatter = new BinaryFormatter();
                return (ServerCommand)formatter.Deserialize(client.GetStream());
            });
        }


        private async void Listen()
        {
            try
            {
                bool isExit = false;
                while (!isExit)
                {

                    ServerCommand command = await ReceiveCommand();


                    switch (command.Type)
                    {

                        case CommandType.WAIT:

                            statusTB.Text = "Waiting...";
                            break;

                        case CommandType.START:
                       
                            IsX = command.IsX;
                            IsMoving = IsX;
                            statusTB.Text = IsMoving ? "Your turn" : "Opponent's turn";
                            symbolTB.Text = Symbol.ToString();
                            OpponentNickname = command.OpponentName;
                            opponentTB.Text = OpponentNickname;
                            break;
                        case CommandType.DRAW:                  
                            IsPlaying = false;
                            statusTB.Text = "Game end.";
                            MessageBox.Show("It look like it's a draw.", $"Game Result for {Nickname}");
                            break;
                        case CommandType.LOSE:
                            IsPlaying = false;
                            statusTB.Text = "Game end.";
                            IsMoving = false;
                            MessageBox.Show("You lose.", $"Game Result for {Nickname}");

                            break;
                        case CommandType.MOVE:
                            statusTB.Text = "Your turn";
                            foreach (Border item in fieldGrid.Children.OfType<Border>())
                            {

                                if (item.Tag.Equals(command.MoveCoord))
                                {
                                    ((TextBlock)item.Child).Text = OpponentSymbol.ToString();
                                    item.IsEnabled = false;
                                }

                                IsMoving = true;
                            }
                            break;

                        case CommandType.CLOSE:
                            if (IsPlaying)
                                MessageBox.Show($"{OpponentNickname} left the game.");
                            
                            CloseSession();

                            isExit = true;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
               

                if (!IsPlaying)
                {
                    IPEndPoint serverEndPoint = new IPEndPoint(address, port);
                    client = new TcpClient();

                    client.Connect(serverEndPoint);

                    IsPlaying = false;
                    IsMoving = false;
                    symbolTB.Text = "-";
                    statusTB.Text = "-";
                    opponentTB.Text = "-";
                    field = new char[SIZE, SIZE];
                    foreach (Border item in fieldGrid.Children.OfType<Border>())
                    {
                        ((TextBlock)item.Child).Text = string.Empty;
                        item.IsEnabled = true;
                    }
                    Nickname = nameTB.Text;

                    await SendCommand(new ClientCommand(CommandType.START, Nickname));

                    Listen();

                    IsPlaying = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private async void CloseSession()
        {

            await SendCommand(new ClientCommand(CommandType.CLOSE, Nickname));


            client.Close();
            client = null;


            IsPlaying = false;
            IsMoving = false;
            symbolTB.Text = "-";
            statusTB.Text = "-";
            opponentTB.Text = "-";
            field = new char[SIZE, SIZE];
            foreach (Border item in fieldGrid.Children.OfType<Border>())
            {
                ((TextBlock)item.Child).Text = string.Empty;
                item.IsEnabled = true;
            }
        }


        async private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {

            if (IsMoving)
            {
                statusTB.Text = "Opponent's turn";
                ((TextBlock)((Border)sender).Child).Text = Symbol.ToString();
                ((Border)sender).IsEnabled = false;


                ClientCommand command = new ClientCommand(CommandType.MOVE, Nickname)
                {
                    MoveCoord = (CellCoord)((Border)sender).Tag
                };
                field[command.MoveCoord.RowIndex, command.MoveCoord.ColumnIndex] = Symbol;
                await SendCommand(command);

                IsMoving = false;
                if (IsWin())
                {
                    command = new ClientCommand(CommandType.WIN, Nickname);
                    IsPlaying = false;
                  
                    await SendCommand(command);
                    statusTB.Text = "Game end.";
                    MessageBox.Show("You win!", $"Game Result for {Nickname}");
                }
                else if (IsDraw())
                {
                    command = new ClientCommand(CommandType.DRAW);
                    await SendCommand(command);
                    IsPlaying = false;
                    statusTB.Text = "Draw";
                    MessageBox.Show("It look like it's a draw.", $"Game Result for {Nickname}");

                }
            }
        }


        private void Window_Closed(object sender, EventArgs e)
        {

            if (IsPlaying)

                SendCommand(new ClientCommand(CommandType.EXIT, Nickname));
        }
    }
}
