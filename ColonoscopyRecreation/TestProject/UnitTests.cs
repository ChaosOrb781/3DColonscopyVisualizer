using ColonoscopyRecreation.Database;
using ColonoscopyRecreation.Entities;
using ColonoscopyRecreation.Extensions;
using ColonoscopyRecreation.GUI;
using Emgu.CV;
using Microsoft.EntityFrameworkCore;

namespace TestProject
{
    public class UnitTests
    {
        [Fact]
        public void TestPartialStringLength()
        {
            string test = Directory.GetCurrentDirectory();
            string partial_test = ConsoleUtil.GetPartialString(test, 50);
            Assert.Equal(50, partial_test.Length);
        }

        [Fact]
        public void MaskBoundingBoxCorrect()
        {
            Video v = new Video(@"C:\Users\tobia\OneDrive\Skrivebord\Github\3DColonscopyVisualizer\Videos\colonoscopy_key_frames_video3_sample.mp4", 
                @"C:\Users\tobia\OneDrive\Skrivebord\Github\3DColonscopyVisualizer\Masks\erda.png");
        }

        [Fact]
        public void ProcessVideo()
        {
            Video v = new Video(@"C:\Users\tobia\OneDrive\Skrivebord\Github\3DColonscopyVisualizer\Videos\colonoscopy_key_frames_video3_sample.mp4",
                @"C:\Users\tobia\OneDrive\Skrivebord\Github\3DColonscopyVisualizer\Masks\erda.png");

            using (var db = new DatabaseContext("processvideo.db"))
            {
                db.Database.EnsureDeleted();
            }

            TimeSpan ts = v.ProcessVideo("processvideo.db", true).TimeIt().Result;

            using (var db = new DatabaseContext("processvideo.db"))
            {
                Assert.True(db.Frames.Count() > 0);
                //All frames contained are atleast of some small size (10KB in this case)
                Assert.True(db.Frames.All(frame => frame.Content.Length > 1024 * 10));
            }
        }

        [Fact]
        public void ProcessVideoParallel()
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
                int count = db.Frames.Count();
                Assert.True(db.Frames.Count() > 0);
                //All frames contained are atleast of some small size (10KB in this case)
                Assert.True(db.Frames.All(frame => frame.Content.Length > 1024 * 10));
            }
        }

        [Fact]
        public void ProcessKeyFeatures()
        {
            using (var db = new DatabaseContext("processvideo.db"))
            {
                int count = db.Frames.Count(f => f.FrameIndex >= 0);
                int i = 1;
                foreach (Frame frame in db.Frames.Where(f => f.FrameIndex >= 0).Include(f => f.Video).AsEnumerable())
                {
                    TimeSpan ts = frame.GenerateKeyFeatures("processvideo.db", frame.Video.Mask.ToMat(), 1000).TimeIt().Result;
                    System.Diagnostics.Debug.WriteLine($"Frame {frame.FrameIndex} got {frame.KeyPoints.Count} keyfeatures in {ts} ({i++}/{count})");
                }
            }
        }
    }
}