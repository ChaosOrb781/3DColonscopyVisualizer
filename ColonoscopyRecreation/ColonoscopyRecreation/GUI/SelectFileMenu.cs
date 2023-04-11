using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColonoscopyRecreation.GUI
{
    public class SelectFileMenu : Menu<string>
    {
        private string _currentpath = null!;
        public string CurrentPath 
        { 
            get { return _currentpath; } 
            set { 
                _currentpath = value;
                SelectedItem = 0;
                Items = Directory.GetDirectories(CurrentPath)
                    .Select(folder => 
                        KeyValuePair.Create<string, Func<Task<string>>>(
                            Path.GetFileName(folder) + "/", () => Task.FromResult(folder)))
                    .Concat(Directory.GetFiles(CurrentPath)
                        .Where(file => 
                            _file_extensions == null //If nothing is defined, do any file
                         || _file_extensions.Contains(Path.GetExtension(file)))
                        .Select(file =>
                        KeyValuePair.Create<string, Func<Task<string>>>(
                            Path.GetFileName(file), () => Task.FromResult(file))))
                    .ToList();
            } 
        }

        private List<string> _file_extensions = null!;

        public SelectFileMenu(IConsoleContext parent, string title, string basepath, IEnumerable<string> include_file_exts = null!) 
            : base(parent, title, include_file_exts == null ? "Select folder" : "Select file", new List<IConsoleContext>())
        {
            CurrentPath = basepath;
            _file_extensions = include_file_exts?.ToList()!;

            Controls.Add("[Left-/RightArrow]: Exit/enter directory");

            AdditionalControls = async (key) =>
            {
                switch (key.Key)
                {
                    case ConsoleKey.RightArrow:
                        if (SelectedItem < Items.Count && SelectedItem >= 0 && Items[SelectedItem].Key.EndsWith("/"))
                            CurrentPath = await Items[SelectedItem].Value.Invoke();
                        break;
                    case ConsoleKey.LeftArrow:
                        var tmp = new DirectoryInfo(CurrentPath);
                        if (tmp.Parent != null)
                            CurrentPath = tmp.Parent.FullName;
                        break;
                }
                return CurrentPath;
            };
        }
    }
}
