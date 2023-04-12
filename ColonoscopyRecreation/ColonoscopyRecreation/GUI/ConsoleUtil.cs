using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColonoscopyRecreation.GUI
{
    public static class ConsoleUtil
    {
        public static int ClearScreen(IConsoleContext context, List<string> controls = null!)
        {
            Console.Clear();
            SetRow(0, '-', '+', '+');

            //Context title
            Console.SetCursorPosition(0, 1);
            Console.WriteLine("|" + context.PartialContextPath);
            Console.SetCursorPosition(0, 2);

            var controlsstring = controls?
                .Aggregate((0, 0, new StringBuilder()), (acc, control) =>
                {
                    if (acc.Item2 == 0)
                        acc.Item2 = 1;
                    if (acc.Item1 + control.Length + 3 > Console.WindowWidth - 2)
                    {
                        acc.Item1 = 0;
                        acc.Item2++;
                        acc.Item3.Append("\n|");
                    }
                    acc.Item1 += control.Length + 3;
                    acc.Item3.Append(control).Append("   ");
                    return acc!;
                });

            bool hascontrols = controlsstring?.Item3.Length > 0;
            int offset = 3;
            if (hascontrols)
            {
                Console.WriteLine($"|{controlsstring.Value.Item3.ToString()}");
                DrawVerticalLine(Console.WindowWidth - 1, 1, controlsstring.Value.Item2 + 1, '|');
                SetRow(controlsstring.Value.Item2 + 2, '-', '+', '+');
                offset += controlsstring.Value.Item2;
            }
            else
            {
                DrawVerticalLine(Console.WindowWidth - 1, 1, 1, '|');
                SetRow(2, '-', '+', '+');
            }
            return offset; 
        }

        public static void DrawHorizontalLine(int row, int col_start, int col_end, char c) => DrawHorizontalLine(row, col_start, col_end, c, c, c);
        public static void DrawHorizontalLine(int row, int col_start, int col_end, char c, char left_c, char right_c)
        {
            Console.SetCursorPosition(col_start, row);
            Console.Write($"{left_c}{new string(c, col_end - col_start - 1)}{right_c}");
        }

        public static void DrawVerticalLine(int col, int row_start, int row_end, char c) => DrawVerticalLine(col, row_start, row_end, c, c, c);
        public static void DrawVerticalLine(int col, int row_start, int row_end, char c, char top_c, char bottom_c)
        {
            Console.SetCursorPosition(col, row_start);
            Console.Write(top_c);
            for (int i = row_start + 1; i < row_end; i++)
            {
                Console.SetCursorPosition(col, i);
                Console.Write(c);
            }
            Console.Write(bottom_c);
        }


        public static void ClearRow(int row)
        {
            SetRow(row, ' ');
            Console.SetCursorPosition(0, row);
        }
        public static void SetRow(int row, char c) => SetRow(row, c, c, c);
        public static void SetRow(int row, char c, char left_c, char right_c)
            => DrawHorizontalLine(row, 0, Console.WindowWidth - 1, c, left_c, right_c);

        public static void ClearColumn(int col) => SetColumn(col, ' ');
        public static void SetColumn(int col, char c) => SetColumn(col, c, c, c);
        public static void SetColumn(int col, char c, char top_c, char bottom_c)
            => DrawVerticalLine(col, 0, Console.WindowHeight - 1, c, top_c, bottom_c);


        public static void DrawMenu<T>(List<KeyValuePair<string, T>> items, int selected_index, int max_height, int col_inset = 3, int row_offset = 5)
        {
            if (selected_index < 0 || selected_index >= items.Count)
                return;

            int lower_count = max_height / 2;
            int upper_count = max_height - lower_count - 1;

            int start_index = Math.Min(Math.Max(0, items.Count - max_height), Math.Max(0, selected_index - upper_count));
            int end_index = Math.Max(Math.Min(max_height - 1, items.Count - 1), Math.Min(start_index + (max_height - 1), items.Count - 1));

            int items_above = start_index;
            int items_below = (items.Count - 1) - end_index;

            for (int i = start_index; i <= end_index; i++)
            {
                ConsoleUtil.ClearRow(i - start_index + row_offset);
                Console.SetCursorPosition(col_inset - 1, i - start_index + row_offset);
                if (i == start_index && items_above > 0)
                    Console.Write($"... {items_above} above ...");
                else if (i == end_index && items_below > 0)
                    Console.Write($"... {items_below} below ...");
                else
                    DrawMenuItem(items, i, i == selected_index);
            }

            //for (int i = Math.Min(max_height - (selected_index + 3), 0); i < Math.Min(items.Count, Console.WindowHeight - row_offset); i++) 
            //    DrawMenuItem<T>(items, i, i == selected_index, max_height, col_inset, row_offset, format);

        }

        public static void DrawMenuItem<T>(List<KeyValuePair<string, T>> items, int index, bool selected)
        {
            if (index < 0 || index >= items.Count)
                throw new ArgumentOutOfRangeException("index");

            if (selected) Console.Write($"[{items[index].Key}]");
            else Console.Write($" {items[index].Key}");
        }

        public static string GetPartialString(string input, int max_length)
        {
            int length = input.Length;
            bool exceeded_width = length > max_length;
            if (exceeded_width)
                return "..." + input.Substring(length - max_length - 3, max_length - 3);
            return input;
        }
    }
}
