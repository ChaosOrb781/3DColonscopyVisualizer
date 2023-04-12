using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColonoscopyRecreation.GUI
{
    public class ErrorMessage : BaseConsoleContext
    {
        private string _message { get; set; }
        public ErrorMessage(IConsoleContext parent, string message) : base(parent, "Error")
        {
            _message = message;
        }

        public override T Display<T>()
        {
            ConsoleKeyInfo key = default;
            while ((Parent == null || key.Key != ConsoleKey.Backspace))
            {
                int offset = ConsoleUtil.ClearScreen(this, Controls);
                ConsoleUtil.ClearRow(offset);
                Console.WriteLine("Error occured:");
                Console.WriteLine(_message);
                key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.Escape: Environment.Exit(0); break;
                }
            }
            return default!;
        }
    }
}
