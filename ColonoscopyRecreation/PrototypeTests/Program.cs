using ColonoscopyRecreation.Database;
using ColonoscopyRecreation.Entities;
using ColonoscopyRecreation.Extensions;
using ColonoscopyRecreation.GUI;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace PrototypeTests
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //TestMaxHeightMenu()
            //TestDisplayFrameFromVideo();
            //TestExtractionOfKeypoints().Wait();
            //TestExtractionOfAllKeypoints().Wait();
            TestDisplayMatchesFrameFromVideo();
        }

        public static void TestDisplayFrameFromVideo()
        {
            Video v = new Video(@"..\..\..\..\..\Videos\colonoscopy_key_frames_video3_sample.mp4",
                @"..\..\..\..\..\Masks\erda.png");

            using (var db = new DatabaseContext("processvideo.db"))
            {
                db.Database.EnsureDeleted();
            }

            TimeSpan ts = v.ProcessVideoParallel("processvideo.db").TimeIt().Result;

            using (var db = new DatabaseContext("processvideo.db"))
            {
                var first_frame = db.Frames.First();
                var keypoints = first_frame.KeyPoints;
                var image = first_frame.ToImageWithKeypoints( Features2DToolbox.KeypointDrawType.DrawRichKeypoints);
                image.Save("testimage.png");
            }
        }

        public static async Task TestExtractionOfKeypoints()
        {
            Video v = new Video(@"..\..\..\..\..\Videos\colonoscopy_key_frames_video3_sample.mp4",
                @"..\..\..\..\..\Masks\erda.png");

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
                first_frame.GenerateKeyPoints(first_frame.Video.Mask.ToImageMat());
                first_frame.GenerateDescriptors();
                db.SaveChanges();
            }
        }

        public static void TestDisplayKeyPointsOnFrameFromVideo()
        {
            Video v = new Video(@"..\..\..\..\..\Videos\colonoscopy_key_frames_video3_sample.mp4",
                @"..\..\..\..\..\Masks\erda.png");

            /*
            using (var db = new DatabaseContext("processvideo.db"))
            {
                db.Database.EnsureDeleted();
            }
            */

            //TimeSpan ts = v.ProcessVideoParallel("processvideo.db").TimeIt().Result;

            using (var db = new DatabaseContext("processvideo.db"))
            {
                var first_frame = db.Frames.First();
                var keypoints = first_frame.KeyPoints;
                var image = first_frame.ToImageWithKeypoints(Features2DToolbox.KeypointDrawType.DrawRichKeypoints);
                image.Save("testimage.png");
            }
        }

        public static void TestDisplayMatchesFrameFromVideo()
        {
            Video v = new Video(@"..\..\..\..\..\Videos\colonoscopy_key_frames_video3_sample.mp4",
                @"..\..\..\..\..\Masks\erda.png");

            /*
            using (var db = new DatabaseContext("processvideo.db"))
            {
                db.Database.EnsureDeleted();
            }
            */

            //TimeSpan ts = v.ProcessVideoParallel("processvideo.db").TimeIt().Result;

            using (var db = new DatabaseContext("processvideo.db"))
            {
                v = db.Videos.Find(1)!;
                for (var frame_ind = 1; frame_ind < db.Frames.Count(); frame_ind++)
                {
                    var first_frame = v.Frames[frame_ind-1];
                    var second_frame = v.Frames[frame_ind];

                    // Create a BFMatcher and match the descriptors
                    BFMatcher matcher = new BFMatcher(DistanceType.L2);
                    Mat descriptors1 = first_frame.GetDescriptors();
                    Mat descriptors2 = second_frame.GetDescriptors();
                    VectorOfKeyPoint keypoints1 = first_frame.GetKeyPoints();
                    VectorOfKeyPoint keypoints2 = second_frame.GetKeyPoints();


                    /*
                    VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch();
                    matcher.KnnMatch(descriptors1, descriptors2, matches, 10);
                    float ratioThresh = 0.75f;
                    List<MDMatch> goodMatches = new List<MDMatch>();
                    for (int i = 0; i < matches.Size; i++)
                    {
                        if (matches[i][0].Distance < ratioThresh * matches[i][1].Distance &&
                            matches[i][0].QueryIdx >= 0 && matches[i][0].QueryIdx < descriptors2.Rows
                            && matches[i][0].TrainIdx >= 0 && matches[i][0].TrainIdx < descriptors1.Rows)
                        {
                            goodMatches.Add(matches[i][0]);

                        }
                    }
                    VectorOfDMatch good_matches = new VectorOfDMatch(goodMatches.ToArray());
                    */

                    VectorOfDMatch matches = new VectorOfDMatch();
                    matcher.Match(descriptors1, descriptors2, matches);
                    VectorOfDMatch good_matches = new VectorOfDMatch();
                    List<(int, double)> distance_arr = new List<(int, double)>();
                    for (int i = 0; i < matches.Size; i++)
                    {
                        if (matches[i].QueryIdx >= 0 && matches[i].QueryIdx < Math.Min(descriptors2.Rows, descriptors1.Rows)
                            && matches[i].TrainIdx >= 0 && matches[i].TrainIdx < Math.Min(descriptors2.Rows, descriptors1.Rows))
                            //good_matches.Push(new MDMatch[] { matches[i] });
                            distance_arr.Add((i, matches[i].Distance));
                    }
                    var top50matches = distance_arr.OrderBy(tup => tup.Item2).Take(50).Select(tup => matches[tup.Item1]).ToArray();
                    good_matches.Push(top50matches);
                
                    // Draw the matches on a new image
                    Mat result = new Mat();
                    Features2DToolbox.DrawMatches(first_frame.ToImage(), keypoints1, second_frame.ToImage(), keypoints2, good_matches, result, new MCvScalar(0, 255, 0, 40), new MCvScalar(0, 0, 255, 40), null);

                    // Display the result
                    CvInvoke.Imshow("Matches", result);
                    CvInvoke.WaitKey(0);

                    var keypoints = first_frame.KeyPoints;
                    var image = first_frame.ToImageWithKeypoints(Features2DToolbox.KeypointDrawType.DrawRichKeypoints);
                    image.Save($"testimage{frame_ind}.png");
                }
            }
        }


        public static async Task TestExtractionOfAllKeypoints()
        {
            Video v = new Video(@"..\..\..\..\..\Videos\colonoscopy_key_frames_video3_sample.mp4",
                @"..\..\..\..\..\Masks\erda.png");

            using (var db = new DatabaseContext("processvideo.db"))
            {
                db.Database.EnsureDeleted();
            }

            TimeSpan ts = v.ProcessVideoParallel("processvideo.db").TimeIt().Result;

            using (var db = new DatabaseContext("processvideo.db"))
            {
                int count = db.Frames.Count(f => f.FrameIndex >= 0);
                int i = 1;
                Mat mask = v.Mask.ToImageMat();
                foreach (Frame frame in db.Frames.Where(f => f.FrameIndex >= 0).Include(f => f.Video).AsEnumerable()) 
                {
                    DateTime before = DateTime.Now; 
                    frame.GenerateKeyPoints(mask, 250);
                    frame.GenerateDescriptors(250);
                    System.Diagnostics.Debug.WriteLine($"Frame {frame.FrameIndex} got {frame.KeyPoints.Count} keypoints in {DateTime.Now.Subtract(before)} ({i++}/{count})");
                }
                db.SaveChanges();
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