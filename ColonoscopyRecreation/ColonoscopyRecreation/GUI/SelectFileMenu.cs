using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColonoscopyRecreation.GUI
{
    public class SelectFileMenu : Menu
    {
        private bool _select_folder = true;
        private string _currentpath = null!;
        public string CurrentPath 
        { 
            get { return _currentpath; } 
            set {
                string old_folder = Path.GetFileName(Path.GetDirectoryName(_currentpath));
                _currentpath = value;
                Items = Directory.GetDirectories(_currentpath)
                    .Select(folder => 
                        KeyValuePair.Create<string, Action>(
                            Path.GetFileName(folder) + "\\", () => { if (_select_folder) { base.ReturnValue = folder; base.ExitEarly = true; } else { CurrentPath = folder; } }))
                    .Concat(Directory.GetFiles(CurrentPath)
                        .Where(file => 
                            _file_extensions != null //If nothing is defined, do any file
                            && _file_extensions.Contains(Path.GetExtension(file)))
                        .Select(file =>
                        KeyValuePair.Create<string, Action>(
                            Path.GetFileName(file), () => { if (!_select_folder) { base.ReturnValue = file; base.ExitEarly = true; } })))
                    .ToList();
                SelectedItem = Items.FindIndex(f => old_folder != null && f.Key.StartsWith(old_folder));
                if (SelectedItem < 0)
                    SelectedItem = 0;

                int width = Console.WindowWidth;
                string title = CurrentPath;
                bool exceeded_width = width - 5 < title.Length;
                if (exceeded_width)
                    title = "..." + title.Substring(title.Length - width + 3, width - 3);
                base.MenuHeader = $"{(_select_folder ? "Select folder" : $"Select file ({string.Join(", ", _file_extensions)})")}\n{title}";
            }
        }

        private List<string> _file_extensions = null!;

        public SelectFileMenu(IConsoleContext parent, string title, string basepath, IEnumerable<string> include_file_exts = null!) 
            : base(parent, title, null)
        {
            _file_extensions = include_file_exts?.ToList()!; 
            _select_folder = include_file_exts == null;
            CurrentPath = basepath;

            Controls.Add("[Left-/RightArrow]: Exit/enter directory");

            AdditionalControls = (key) =>
            {
                switch (key.Key)
                {
                    case ConsoleKey.RightArrow:
                        if (SelectedItem < Items.Count && SelectedItem >= 0 && Items[SelectedItem].Key.EndsWith("\\"))
                            CurrentPath = Path.Combine(CurrentPath, Items[SelectedItem].Key.Substring(0, Items[SelectedItem].Key.Length));
                        break;
                    case ConsoleKey.LeftArrow:
                        var tmp = new DirectoryInfo(CurrentPath);
                        if (tmp.Parent != null)
                            CurrentPath = tmp.Parent.FullName;
                        break;
                }
            };
        }
    }
}
