using Emgu.CV.Ocl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColonoscopyRecreation.GUI
{
    public class Menu : BaseConsoleContext
    {
        public string MenuHeader { get; set; } = "Select an item";
        public List<KeyValuePair<string, Action>> Items { get; set; } = new List<KeyValuePair<string, Action>>();
        public int SelectedItem { get; set; } = 0;
        protected object ReturnValue { get; set; } = null!;
        protected bool ExitEarly { get; set; } = false;
        public Action<ConsoleKeyInfo> AdditionalControls { get; set; } = (_) => { };

        public Menu(IConsoleContext parent, string title, string menuheader) : base(parent, title)
        {
            MenuHeader = menuheader;
            Controls.Add("[Down-/UpArrow]: Navigate");
            Controls.Add("[Enter]: Select item");
        }

        public void AddContextSwitchItem<S>(IConsoleContext context, Action<IConsoleContext, S> return_action)
            => Items.Add(KeyValuePair.Create<string, Action>(context.Title, () =>
            {
                return_action(context, context.Display<S>());
            }));
        
        public void AddItem(string itemname, Action action)
            => Items.Add(KeyValuePair.Create(itemname, action));

        public override void Reset()
        {
            this.ReturnValue = null!;
            this.ExitEarly = false;
        }

        public override T Display<T>()
        {
            Console.CursorVisible = false;
            ConsoleKeyInfo key = default;
            while((Parent == null || key.Key != ConsoleKey.Backspace) && !ExitEarly)
            {
                int offset = ConsoleUtil.ClearScreen(this, Controls);
                ConsoleUtil.ClearRow(offset);
                foreach (string str in MenuHeader.Split('\n'))
                {
                    Console.WriteLine(str);
                    offset++;
                }
                ConsoleUtil.DrawMenu(Items, SelectedItem, Console.WindowHeight - offset - 1, 3, offset);
                
                key = Console.ReadKey(true);
                int old_index = SelectedItem;
                switch (key.Key)
                {
                    case ConsoleKey.Escape: Environment.Exit(0); break;
                    case ConsoleKey.Enter:
                        Items[SelectedItem].Value.Invoke();
                        break;
                    case ConsoleKey.DownArrow:
                        SelectedItem = SelectedItem < Items.Count - 1 ? SelectedItem + 1 : 0;
                        break;
                    case ConsoleKey.UpArrow:
                        SelectedItem = SelectedItem > 0 ? SelectedItem - 1 : Items.Count - 1;
                        break;
                    default:
                        AdditionalControls.Invoke(key);
                        break;
                }
            }
            Console.CursorVisible = true;
            base.Cleanup();
            ExitEarly = false;
            return ReturnValue is T res ? res : default!;
        }
    }
}
