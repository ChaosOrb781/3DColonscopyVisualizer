using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ColonoscopyRecreation.GUI
{
    public class ProgressBar : BaseConsoleContext
    {
        private string progress_text = "";
        private string progress_bar = "";
        private int progress_text_offset = -1;

        public ProgressBar(IConsoleContext parent, string title) : base (parent, title)
        {
            base.Controls.Clear();
        }

        public void UpdateTitle(string title)
        {
            this.Title = title;
            progress_text_offset = ConsoleUtil.ClearScreen(this, Controls) + 2;
            if (progress_text != null && progress_bar != null)
            {
                Console.SetCursorPosition(0, progress_text_offset);
                Console.WriteLine(progress_bar);
                Console.WriteLine(progress_text);
            }
            else
                this.Display<object>();
        }

        public void UpdateProgress(int current, int total)
        {
            if (progress_text_offset < 0)
                progress_text_offset = ConsoleUtil.ClearScreen(this, Controls) + 2;

            float percent = (float)current / total;
            progress_text = $"{current} / {total} ({(int)Math.Round(percent * 100,0)}%)";
            int contentWidth = Console.WindowWidth - 2;
            int numberOfEquals = (int)(percent * contentWidth);
            int numberOfEmpty = contentWidth - numberOfEquals;
            string oldtext = progress_bar;
            progress_bar = $"[{new string('=', numberOfEquals)}{new string('-', numberOfEmpty)}]";
            bool updated = progress_text != oldtext;

            if (updated)
            {
                Console.SetCursorPosition(0, progress_text_offset);
                Console.WriteLine(progress_bar);
                ConsoleUtil.ClearRow(progress_text_offset + 1);
                Console.WriteLine(progress_text);
            }
        }

        public void UpdateText(string text) => progress_text = text;

        public override T Display<T>()
        {
            if (progress_text_offset < 0)
                progress_text_offset = ConsoleUtil.ClearScreen(this, Controls) + 2;
            int contentWidth = Console.WindowWidth - 2;
            int startoffset = (int)(((float)DateTime.Now.Second / 59) * (contentWidth - 2));
            Console.SetCursorPosition(0, progress_text_offset);
            Console.WriteLine($"[{new string('-', startoffset)}={new string('-', contentWidth - startoffset - 1)}]");
            Console.WriteLine(progress_text ?? "");
            return default!;
        }
    }
}
