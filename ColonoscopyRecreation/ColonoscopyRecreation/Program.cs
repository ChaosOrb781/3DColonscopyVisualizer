using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using ColonoscopyRecreation.Database;
using ColonoscopyRecreation.Entities;
using ColonoscopyRecreation.GUI;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Reg;
using Emgu.CV.Structure;

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

            var mainmenu = new Menu<object>(null!, "Main menu", "Select an action", new List<IConsoleContext>());
            mainmenu.AddContextSwitchItem(new SelectFileMenu(mainmenu, "Process Video", Directory.GetCurrentDirectory(), new List<string>() { ".mp4" }),
                (string videopath) =>
                {
                    if (videopath != null)
                    {
                        using (var db = new DatabaseContext(SQLiteDatabasePath))
                        {
                            var video = new Video { VideoFilepath = videopath };
                            db.Videos.Add(video);
                            db.SaveChanges();
                            var videocapture = new VideoCapture(videopath);
                            var frames = new List<Image<Gray, byte>>();
                            Mat frame = new Mat();
                            while (videocapture.Read(frame))
                            {
                                CvInvoke.CvtColor(frame, frame, ColorConversion.Bgr2Gray);

                                using (var ms = new MemoryStream())
                                {
                                    db.Frames.Add(new Frame() { });
                                }
                            }
                            //return null;
                            db.SaveChanges();
                        }
                    }
                });
            mainmenu.AddContextSwitchItem(new SelectFileMenu(mainmenu, "Process Frames", Directory.GetCurrentDirectory()),
                (object videopath) =>
                {

                });
            mainmenu.AddContextSwitchItem(new SelectFileMenu(mainmenu, "Match Features", Directory.GetCurrentDirectory()), 
                (object videopath) =>
                {

                });
            mainmenu.AddContextSwitchItem(new SelectFileMenu(mainmenu, "Folder4", Directory.GetCurrentDirectory()),
                (object videopath) =>
                {

                });

            await mainmenu.Display();


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