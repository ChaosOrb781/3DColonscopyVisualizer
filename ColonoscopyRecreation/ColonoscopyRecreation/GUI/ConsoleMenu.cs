using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColonoscopyRecreation.GUI
{
    public class Menu<T> : BaseConsoleContext
    {
        public string MenuHeader { get; set; } = "Select an item";
        public List<KeyValuePair<string, Action<Task>>> Items { get; set; }
        public int SelectedItem { get; set; } = 0;
        public Func<ConsoleKeyInfo, Task<T>> AdditionalControls { get; set; } = (_) => Task.FromResult<T>(default);
        public Menu(IConsoleContext parent, string title, string menuheader, List<IConsoleContext> navigation_items) : this(parent, title, menuheader, navigation_items
                .Select(item => KeyValuePair.Create<string, Func<Task<T>>>(
                    item.Title,
                    async () => await item.DisplayWithReturn<T>())
                ).ToList())
        {
        }

        public Menu(IConsoleContext parent, string title, string menuheader, List<KeyValuePair<string, Func<Task<T>>>> items) : base(parent, title)
        {
            MenuHeader = menuheader;
            Items = items;
            Controls.Add("[Down-/UpArrow] Navigate");
            Controls.Add("[Enter] Select item");
        }

        public void AddContextSwitchItem<S>(IConsoleContext context, Action<S> return_action)
            => Items.Add(KeyValuePair.Create<string, Func<Task<S>>>(context.Title, async () =>
            {
                if (return_action != null && context is IConsoleContext<S> newcontext)
                    await newcontext.DisplayWithReturn().ContinueWith(res => return_action(res.Result));
                else
                    await context.Display();
                return default!;
            }));

        public override async Task<T> Display()
        {
            Console.CursorVisible = false;
            ConsoleKeyInfo key = default;
            T selected_item = default!;
            while(Parent == null || key.Key != ConsoleKey.Backspace)
            {
                ConsoleUtil.ClearScreen(this, Controls);
                ConsoleUtil.ClearRow(4);
                Console.WriteLine(MenuHeader);
                ConsoleUtil.DrawMenu(Items, SelectedItem, Console.WindowHeight - 5, 3, 5);
                
                key = Console.ReadKey(true);
                int old_index = SelectedItem;
                switch (key.Key)
                {
                    case ConsoleKey.Escape: Environment.Exit(0); break;
                    case ConsoleKey.Enter:
                        selected_item = await Items[SelectedItem].Value.Invoke();
                        ConsoleUtil.ClearScreen(this);
                        break;
                    case ConsoleKey.DownArrow:
                        SelectedItem = SelectedItem < Items.Count - 1 ? SelectedItem + 1 : 0;
                        break;
                    case ConsoleKey.UpArrow:
                        SelectedItem = SelectedItem > 0 ? SelectedItem - 1 : Items.Count - 1;
                        break;
                    default:
                        await AdditionalControls.Invoke(key);
                        break;
                }
            }
            Console.CursorVisible = true;
            base.Cleanup();
            return selected_item;
        }
    }
}
