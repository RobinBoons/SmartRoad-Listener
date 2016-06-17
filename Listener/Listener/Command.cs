using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPClient
{
    public class Command
    {
        private string rawCommand;

        public const string id = "IDENTIFICATION";

        public string command { get
            {
                return rawCommand.Substring(0, rawCommand.IndexOf(':'));
            }
        }

        public string parameter
        {
            get
            {
                return rawCommand.Substring(rawCommand.IndexOf(':') + 1);
            }
        }
        public Command(string rawCommand)
        {
            this.rawCommand = rawCommand;
        }
    }
}
