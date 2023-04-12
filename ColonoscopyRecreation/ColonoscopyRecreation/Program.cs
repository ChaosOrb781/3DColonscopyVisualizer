using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ColonoscopyRecreation.Database;
using ColonoscopyRecreation.Entities;
using ColonoscopyRecreation.GUI;
using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Reg;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.VideoStab;
using Microsoft.EntityFrameworkCore;

namespace ColonoscopyRecreation
{
    internal class Program
    {
        public const string SQLiteDatabasePath = @"..\..\..\local.db";

        [STAThread]
        static async Task Main(string[] args)
        {
            string sqlstring = SQLiteDatabasePath;
            if (args.Length == 2)
                sqlstring = args[2];

            /*
            using (var db = new DatabaseContext(sqlstring))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            }
            */

            var mainmenu = new Menu(null!, "Main menu", "Select an action");
            mainmenu.AddContextSwitchItem(new SelectFileMenu(mainmenu, "Load video", Directory.GetCurrentDirectory(), new List<string>() { ".mp4" }),
            
                 async (IConsoleContext current, string videopath) =>
                {
                    if (videopath != null)
                    {
                        var maskfile = videopath.Replace(".mp4", ".png");

                        Video? video;
                        using (var db = new DatabaseContext(sqlstring))
                        {
                            db.Database.EnsureCreated();
                            video = await db.Videos.FirstOrDefaultAsync(v => v.VideoFilepath == videopath);
                            if (video == null)
                            {
                                video = new Video { VideoFilepath = videopath, MaskFilePath = File.Exists(maskfile) ? maskfile : null! };
                                db.Database.EnsureCreated();
                                db.Videos.Add(video);
                                db.SaveChanges();
                            }
                        }
                        var progressBar = new ProgressBar(current, "Processing...");
                        await video.ProcessVideoParallel(sqlstring, progressBar);
                    }
                    current.Reset();
                });
            mainmenu.AddContextSwitchItem(new SelectVideoMenu(mainmenu, "Process Frames", (int vid) => {
                    using (var db = new DatabaseContext(sqlstring))
                    {
                        return db.Frames.Any(f => f.VideoId == vid && f.FrameIndex > -1 && !f.KeyPoints.Any());
                    }
                }),
                (IConsoleContext current, int? videoid) =>
                {
                    if (videoid != null)
                    {
                        using (var db = new DatabaseContext(sqlstring))
                        {
                            Video v = db.Videos.Find(videoid)!;
                            var mask = v.Mask?.ToImageMat();
                            var pb = new ProgressBar(current, "Generating keypoints...");
                            int total = v.Frames.Count(f => f.FrameIndex > -1);
                            int count = 1;
                            foreach (var f in v.Frames.Where(f => f.FrameIndex > -1)) //Exclude mask
                            {
                                pb.UpdateProgress(count++, total);
                                f.GenerateKeyPoints(mask);
                            }
                            pb.UpdateTitle("Generating descriptors...");
                            count = 1;
                            foreach (var f in v.Frames.Where(f => f.FrameIndex > -1)) //Exclude mask
                            {
                                pb.UpdateProgress(count++, total);
                                f.GenerateDescriptors();
                            }
                            db.SaveChanges();
                        }
                    }
                    current.Reset();
                });
            mainmenu.AddContextSwitchItem(new SelectVideoMenu(mainmenu, "Show Feature Matches", (int vid) => {
                    using (var db = new DatabaseContext(sqlstring))
                    {
                        var frameid = db.Frames.Where(f => f.VideoId == vid && f.FrameIndex > -1).Select(f => f.Id).FirstOrDefault()!;
                        return frameid > 0 && db.KeyPoints.Any(kp => kp.FrameId == frameid);
                    }
                }), 
                (IConsoleContext current, int? videoid) =>
                {
                    if (videoid != null)
                    {
                        using (var db = new DatabaseContext(sqlstring))
                        {
                            Video v = db.Videos.Find(videoid)!;
                            var frames = db.Frames
                                .Where(f => f.FrameIndex > -1 && f.VideoId == videoid)
                                .OrderBy(f => f.FrameIndex)
                                .Select(f => f.Id).ToList();
                            int frame_ind = 1;

                            Frame prev_frame = db.Frames.Find(frames[0]);

                            while (true)
                            {
                                Frame current_frame = db.Frames.Find(frames[frame_ind]);

                                // Create a BFMatcher and match the descriptors
                                BFMatcher matcher = new BFMatcher(DistanceType.L2);
                                Mat descriptors1 = prev_frame.GetDescriptors();
                                Mat descriptors2 = current_frame.GetDescriptors();
                                VectorOfKeyPoint keypoints1 = prev_frame.GetKeyPoints();
                                VectorOfKeyPoint keypoints2 = current_frame.GetKeyPoints();

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
                                    if (matches[i].QueryIdx >= 0 && matches[i].QueryIdx < descriptors2.Rows
                                        && matches[i].TrainIdx >= 0 && matches[i].TrainIdx < descriptors1.Rows)
                                        //good_matches.Push(new MDMatch[] { matches[i] });
                                        distance_arr.Add((i, matches[i].Distance));
                                }
                                var top_n_matches = distance_arr.OrderBy(tup => tup.Item2).Take(100).Select(tup => matches[tup.Item1]).ToArray();
                                good_matches.Push(top_n_matches);

                                // Draw the matches on a new image
                                Mat result = new Mat();
                                Features2DToolbox.DrawMatches(prev_frame.ToImage(), keypoints2, current_frame.ToImage(), keypoints1, good_matches, result, new MCvScalar(0, 255, 0, 40), new MCvScalar(0, 0, 255, 40), null, Features2DToolbox.KeypointDrawType.DrawRichKeypoints);

                                // Display the result
                                CvInvoke.PutText(result, $"Frame {frame_ind - 1}-{frame_ind} (total: {frames.Count})", new System.Drawing.Point(5, 20), Emgu.CV.CvEnum.FontFace.HersheyPlain, 1, new MCvScalar(255, 100, 100), 1);
                                CvInvoke.PutText(result, $"Esc/Backspace to exit", new System.Drawing.Point(5, 40), Emgu.CV.CvEnum.FontFace.HersheyPlain, 1, new MCvScalar(255, 100, 100), 1);
                                CvInvoke.Imshow("Matches", result);
                                int key = CvInvoke.WaitKey(0);

                                //Debug.WriteLine(key);
                                if (key == 8 || key == 27) break; //Backspace or escape

                                prev_frame = current_frame;
                                frame_ind = frame_ind < frames.Count - 1 ? frame_ind + 1 : 1;
                            }
                        }
                    }
                    CvInvoke.DestroyAllWindows();
                    current.Reset();
                });
            mainmenu.AddContextSwitchItem(new SelectFileMenu(mainmenu, "Initialize ColMap workspace", Directory.GetCurrentDirectory(), null!) { MenuHeader = "Select workspace folder" },
               async (IConsoleContext current, string folderpath) =>
               {
                   var videomenu = new SelectVideoMenu(current, "Select video", (int vid) => {
                       using (var db = new DatabaseContext(sqlstring))
                       {
                           return db.Frames.Any(f => f.VideoId == vid);
                       }
                   });

                   var videoid = videomenu.Display<int?>();
                   if (videoid == null) //backspaced
                       return;

                   Video video;
                   using (var db = new DatabaseContext(sqlstring))
                   {
                       video = db.Videos.Find(videoid)!;
                   }

                   //Setup workspace
                   var videoname = Path.GetFileNameWithoutExtension(video.VideoFilepath);
                   var workspacepath = Path.Combine(folderpath, videoname);
                   if (Directory.Exists(workspacepath))
                       Directory.Delete(workspacepath, true);

                   var progressbar = new ProgressBar(current, "Loading video to frames...");

                   Directory.CreateDirectory(workspacepath);

                   var imagefolder = Path.Combine(workspacepath, "Image");
                   Directory.CreateDirectory(imagefolder);

                   var maskfolder = Path.Combine(workspacepath, "Mask");
                   Image<Gray, byte> mask = null!;
                   if (video.MaskFilePath != null)
                   {
                       Directory.CreateDirectory(maskfolder);
                       mask = CvInvoke.Imread(video.MaskFilePath, ImreadModes.Grayscale).ToImage<Gray, byte>();
                   }

                   var videocapture = new VideoCapture(video.VideoFilepath);
                   Mat frame = new Mat();
                   int frame_counter = 1;
                   while (videocapture.Read(frame))
                   {
                       progressbar.UpdateText($"Processed: {frame_counter} frames");
                       progressbar.Display<object>();

                       var image = frame.ToImage<Bgr, byte>();
                       image.Save(Path.Combine(imagefolder, string.Format("frame{0:D4}.png", frame_counter)));

                       if (mask != null)
                           mask.Save(Path.Combine(maskfolder, string.Format("frame{0:D4}.png", frame_counter)));

                       frame_counter++;
                   }

                   using (var db = new DatabaseContext(sqlstring))
                   {
                       ColMapWorkspace workspace = db.ColMapWorkspaces.FirstOrDefault(ws => ws.FolderPath == workspacepath)!;
                       if (workspace == null)
                       {
                           workspace = new ColMapWorkspace()
                           {
                               FolderPath = workspacepath,
                               Video = video
                           };
                       }
                       workspace.Status = WorkspaceStatus.Initialized;
                       db.ColMapWorkspaces.Update(workspace);
                       db.SaveChanges();
                   }

                   current.Reset();
               });
            mainmenu.AddContextSwitchItem(new SelectColMapWorkspaceMenu(mainmenu, "Run ColMap reconstruction (windows)", (int wid) => {
                    using (var db = new DatabaseContext(sqlstring))
                    {
                        var workspace = db.ColMapWorkspaces.Find(wid);
                        return workspace.Status == WorkspaceStatus.Initialized;
                    }
                }) { MenuHeader = "Select workspace" },
               async (IConsoleContext current, int? workspaceid) =>
               {
                   if (workspaceid == null) //backspaced
                       return;

                   ColMapWorkspace workspace;
                   ExecutableFilePath colmapexecutable;
                   using (var db = new DatabaseContext(sqlstring))
                   {
                       workspace = db.ColMapWorkspaces.Find(workspaceid)!;

                       colmapexecutable = db.ExecutableFilePaths.FirstOrDefault(exe => exe.Name == "colmap")!;
                       if (colmapexecutable == null)
                       {
                           var selectcolmap = new SelectFileMenu(current, "Select location of COLMAP.bat", Directory.GetCurrentDirectory(), new List<string>() { ".bat" });
                           string colmappath = selectcolmap.Display<string>();
                           colmapexecutable = new ExecutableFilePath() { Name = "colmap", FilePath = colmappath };
                           db.ExecutableFilePaths.Add(colmapexecutable);
                           db.SaveChanges();
                       }
                   }

                   //Setup workspace
                   var workspacepath = workspace.FolderPath;
                   var imagepath = Path.Combine(workspacepath, "Image");
                   var maskpath = Directory.Exists(Path.Combine(workspacepath, "Mask")) ? Path.Combine(workspacepath, "Mask") : null;
                   var datatype = "video";
                   var quality = "high";

                   Process colmapproc = new Process();
                   colmapproc.StartInfo.FileName = colmapexecutable.FilePath;
                   StringBuilder args = new StringBuilder();
                   args.Append($"automatic_reconstructor ");
                   args.Append($"--log_to_stderr=1 ");
                   args.Append($"--workspace_path='{workspacepath}' ");
                   args.Append($"--image_path='{imagepath}' ");
                   if (maskpath != null)
                       args.Append($"--mask_path='{maskpath}' ");
                   args.Append($"--data_type={datatype} ");
                   args.Append($"--quality={quality} ");
                   args.Append($"--single_camera={1} ");
                   colmapproc.StartInfo.Arguments = args.ToString();
                   //colmapproc.StartInfo.RedirectStandardError = true;
                   //colmapproc.StartInfo.RedirectStandardOutput = true;
                   colmapproc.StartInfo.UseShellExecute = true;

                   Debug.WriteLine($"Running command: COLMAP.bat {colmapproc.StartInfo.Arguments}");
                   Debug.WriteLine($")

                   colmapproc.Start();

                   colmapproc.WaitForExit();

                   using (var db = new DatabaseContext(sqlstring))
                   {
                       db.Attach(workspace);
                       workspace.Status = WorkspaceStatus.Reconstructed;
                       db.SaveChanges();
                   }

                   current.Reset();
               });
            mainmenu.Display<object>();


            String win1 = "Test Window"; //The name of the window
            CvInvoke.NamedWindow(win1); //Create the window using the specific name

            Mat img = new Mat(200, 400, DepthType.Cv8U, 3); //Create a 3 channel image of 400x200
            img.SetTo(new Bgr(255, 0, 0).MCvScalar); // set it to Blue color

            //Draw "Hello, world." on the image using the specific font
            CvInvoke.PutText(
               img,
               "Hello, world",
               new System.Drawing.Point(10, 80),
               FontFace.HersheyComplex,
               1.0,
               new Bgr(0, 255, 0).MCvScalar);

            using (var db = new DatabaseContext(SQLiteDatabasePath))
            {
                var video = new Video { VideoFilepath = "" };
                db.Videos.Add(video);
                db.SaveChanges();
            }

            CvInvoke.Imshow(win1, img); //Show the image
            CvInvoke.WaitKey(0);  //Wait for the key pressing event
            CvInvoke.DestroyWindow(win1); //Destroy the window if key is pressed
        }

        public static List<Image<Gray, byte>> LoadGrayscaleVideo(string videopath)
        {
            var video = new VideoCapture(videopath);
            var frames = new List<Image<Gray, byte>>();
            Mat frame = new Mat();
            while (video.Read(frame))
            {
                
            }
            return null;
        }

        public static List<Image<Gray, byte>> ProcessVideo()
        {
            return null;
        }
    }
}