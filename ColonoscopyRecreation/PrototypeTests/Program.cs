using ColonoscopyRecreation.Database;
using ColonoscopyRecreation.Entities;
using ColonoscopyRecreation.Extensions;
using ColonoscopyRecreation.GUI;
using Microsoft.EntityFrameworkCore;

namespace PrototypeTests
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //TestMaxHeightMenu()
            //TestDisplayFrameFromVideo();
            //TestExtractionOfKeypoints();
            TestExtractionOfAllKeypoints().Wait();
        }

        public static void TestDisplayFrameFromVideo()
        {
            Video v = new Video(@"C:\Users\tobia\OneDrive\Skrivebord\Github\3DColonscopyVisualizer\Videos\colonoscopy_key_frames_video3_sample.mp4",
                @"C:\Users\tobia\OneDrive\Skrivebord\Github\3DColonscopyVisualizer\Masks\erda.png");

            using (var db = new DatabaseContext("processvideo.db"))
            {
                db.Database.EnsureDeleted();
            }

            TimeSpan ts = v.ProcessVideoParallel("processvideo.db").TimeIt().Result;

            using (var db = new DatabaseContext("processvideo.db"))
            {
                var first_frame = db.Frames.Include(f => f.Video).First();
                var image = first_frame.ToImage();
                image.Save("testimage.png");
            }
        }

        public static async Task TestExtractionOfKeypoints()
        {
            Video v = new Video(@"C:\Users\tobia\OneDrive\Skrivebord\Github\3DColonscopyVisualizer\Videos\colonoscopy_key_frames_video3_sample.mp4",
                @"C:\Users\tobia\OneDrive\Skrivebord\Github\3DColonscopyVisualizer\Masks\erda.png");

            using (var db = new DatabaseContext("processvideo.db"))
            {
                db.Database.EnsureDeleted();
            }

            TimeSpan ts = v.ProcessVideoParallel("processvideo.db").TimeIt().Result;

            Frame first_frame = new Frame();
            using (var db = new DatabaseContext("processvideo.db"))
            {
                first_frame = db.Frames
                    .Include(f => f.Video)
                        .ThenInclude(v => v.Frames)
                    .First();
            }

            await first_frame.ProcessKeyFeatures("processvideo.db", first_frame.Video.Mask.ToMat());
        }

        public static async Task TestExtractionOfAllKeypoints()
        {
            Video v = new Video(@"C:\Users\tobia\OneDrive\Skrivebord\Github\3DColonscopyVisualizer\Videos\colonoscopy_key_frames_video3_sample.mp4",
                @"C:\Users\tobia\OneDrive\Skrivebord\Github\3DColonscopyVisualizer\Masks\erda.png");

            using (var db = new DatabaseContext("processvideo.db"))
            {
                db.Database.EnsureDeleted();
            }

            TimeSpan ts = v.ProcessVideoParallel("processvideo.db").TimeIt().Result;

            Frame first_frame = new Frame();
            using (var db = new DatabaseContext("processvideo.db"))
            {
                int count = db.Frames.Count(f => f.FrameIndex >= 0);
                int i = 1;
                foreach (Frame frame in db.Frames.Where(f => f.FrameIndex >= 0).Include(f => f.Video).AsEnumerable()) 
                {
                    ts = frame.ProcessKeyFeatures("processvideo.db", v.Mask.ToMat(), 250).TimeIt().Result;
                    System.Diagnostics.Debug.WriteLine($"Frame {frame.FrameIndex} got {frame.KeyPoints.Count} keyfeatures in {ts} ({i++}/{count})");
                }
            }

        }

        public static void TestMaxHeightMenu()
        {

            //List<string> items = new() { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K" };
            List<KeyValuePair<string, int>> items = Enumerable.Range(0, 4).Select(i => KeyValuePair.Create(i.ToString(), i)).ToList();
            int selected = 0;
            ConsoleKeyInfo key = default;
            while (key.Key != ConsoleKey.Escape)
            {
                Console.Clear();
                ConsoleUtil.DrawMenu(items, selected, 10, 3, 4);
                Console.CursorVisible = false;
                key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow: selected--; break;
                    case ConsoleKey.DownArrow: selected++; break;
                }
                if (selected < 0) selected = items.Count - 1;
                if (selected >= items.Count) selected = 0;
            }
        }
    }
}