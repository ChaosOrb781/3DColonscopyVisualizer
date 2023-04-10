using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColonoscopyRecreation.GUI
{
    public abstract class BaseConsoleContext : IConsoleContext
    {
        public string Title { get; set; } = "Empty";
        public string FullContextPath => Parent?.FullContextPath == null ? Title : Parent.FullContextPath + " > " + Title;
        public string PartialContextPath
        { 
            get
            {
                int width = Console.WindowWidth;
                string title = FullContextPath;
                bool exceeded_width = width < title.Length - 5;
                if (exceeded_width)
                    title = "..." + title.Substring(title.Length - width - 5, width - 5);
                return title;
            } 
        }
        public List<string> Controls { get; set; } = new List<string>()
        {
            "[Esc]: Quit"
        };
        public IConsoleContext Parent { get; } = null!;
        private CancellationTokenSource _cancellationTokenSource;

        public BaseConsoleContext(IConsoleContext parent, string title)
        {
            Title = title;
            Parent = parent;
            _cancellationTokenSource = new();
        }

        public virtual Task Display() => throw new NotImplementedException();
        public virtual Task<T> Display<T>() => throw new NotImplementedException();
        public void Cleanup()
        {
            _cancellationTokenSource.Cancel();
        }

        public override string ToString() => Title;
    }
}
