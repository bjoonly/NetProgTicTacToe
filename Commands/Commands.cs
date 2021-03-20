using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commands
{
    public enum CommandType
    {
        WAIT,   
        START,  
        DRAW,
        MOVE,   
        CLOSE,  
        EXIT,
        WIN,
        LOSE
    }

  
    [Serializable]
    public struct CellCoord
    {
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }

        public override string ToString()
        {
            return $"{RowIndex}:{ColumnIndex}";
        }
    }

 
    [Serializable]
    public class ClientCommand
    {
        public CommandType Type { get; set; }
        public string Nickname { get; set; }
        public CellCoord MoveCoord { get; set; }

        public ClientCommand(CommandType type, string nick=null)
        {
            this.Type = type;
            if(nick!=null)
            this.Nickname = nick;
        }
    }


    [Serializable]
    public class ServerCommand
    {
        public CommandType Type { get; set; }
        public bool IsX { get; set; }
        public string OpponentName { get; set; }
        public CellCoord MoveCoord { get; set; }

        public ServerCommand(CommandType type)
        {
            this.Type = type;
        }
    }
}
