using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColonoscopyRecreation.GUI
{
    public interface IConsoleContext
    {
        public string Title { get; set; }
        public string FullContextPath { get; }
        public string PartialContextPath { get; }
        public List<string> Controls { get; set; }
        public IConsoleContext Parent { get; }

        public Task Display();
        public Task<T> Display<T>();
    }
}
