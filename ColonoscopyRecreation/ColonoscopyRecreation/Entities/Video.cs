using ColonoscopyRecreation.Database;
using ColonoscopyRecreation.Extensions;
using ColonoscopyRecreation.GUI;
using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColonoscopyRecreation.Entities
{
    [Flags]
    public enum VideoState
    {
        NotProcessed = 0,
        VideoProcessed = 1,
        FramesProcessed = 2
    }

    public class Video
    {
        public int Id { get; set; }
        public string VideoFilepath { get; set; } = null!;
        public int Width { get; set; }
        public int Height { get; set; }
        public string MaskFilePath { get; set; } = null!;
        public int MaskOffsetX { get; set; } = 0;
        public int MaskOffsetY { get; set; } = 0;
        public virtual Frame Mask => Frames.FirstOrDefault(f => f.FrameIndex == -1);
        public virtual List<Frame> Frames { get; set; } = new List<Frame>();

        public Video() { }
        public Video(string videofilepath, string maskfilepath = null!)
        {
            if (!File.Exists(videofilepath))
                throw new FileNotFoundException(videofilepath);
            if (maskfilepath != null && !File.Exists(maskfilepath))
                throw new FileNotFoundException(maskfilepath);
            VideoFilepath = videofilepath;
            MaskFilePath = maskfilepath!;

            using (var vc = new VideoCapture(VideoFilepath))
            {
                Mat frame = new Mat();
                vc.Read(frame);
                Width = frame.Width;
                Height = frame.Height;
            }

            if (MaskFilePath != null)
                SetWidthHeightBasedOnMask();
        }

        public void SetWidthHeightBasedOnMask()
        {
            var image = CvInvoke.Imread(MaskFilePath, ImreadModes.Grayscale).ToImage<Gray, byte>();
            int minx = int.MaxValue, maxx = int.MinValue, miny = int.MaxValue, maxy = int.MinValue;
            for (int col = 0; col < image.Cols; col++)
            {
                for (int row = 0; row < image.Rows; row++)
                {
                    if (image[row, col].Intensity > 0.0)
                    {
                        if (col < minx) minx = col;
                        if (col > maxx) maxx = col;
                        if (row < miny) miny = row;
                        if (row > maxy) maxy = row;
                    }
                }
            }
            Width = maxx - minx;
            Height = maxy - miny;
            MaskOffsetX = minx;
            MaskOffsetY = miny;
        }

        public async Task ProcessVideoParallel(string sqlconnection, ProgressBar progressbar = null!, bool force_generate = false)
        {
            if (!File.Exists(VideoFilepath))
                throw new InvalidDataException($"VideoFilepath {VideoFilepath} does not exist when tried to process it");

            using (var db = new DatabaseContext(sqlconnection))
            {
                db.Database.EnsureCreated();
                db.Attach(this);

                //Read in the video and mask
                var videocapture = new VideoCapture(VideoFilepath);
                Image<Gray, byte> mask = null!;
                if (MaskFilePath != null && File.Exists(MaskFilePath) && !this.Frames.Any(f => f.FrameIndex == -1))
                {
                    mask = CvInvoke.Imread(MaskFilePath, ImreadModes.Grayscale).ToImage<Gray, byte>();

                    //Add frame to the database
                    Frame db_frame = new Frame()
                    {
                        Content = GetMaskedImageBytesParallel(mask, mask),
                        FrameIndex = -1,
                        Video = this
                    };
                    Width = mask.Width; 
                    Height = mask.Height;
                    this.Frames.Insert(0, db_frame);
                }


                int frame_counter = 0;
                //Skip already generated frames
                HashSet<int> existing_frames = new(Frames.Select(f => f.FrameIndex));

                Mat frame = new Mat();
                while (videocapture.Read(frame))
                {
                    progressbar.UpdateText($"Processed: {frame_counter + 1} frames");
                    progressbar.Display<object>();
                    if (force_generate || !existing_frames.Contains(frame_counter))
                    {
                        //Convert the frame to a grayscale image
                        CvInvoke.CvtColor(frame, frame, ColorConversion.Bgr2Gray);
                        var image = frame.ToImage<Gray, byte>();

                        //Add frame to the database
                        Frame db_frame = new Frame()
                        {
                            Content = GetMaskedImageBytesParallel(image, mask),
                            FrameIndex = frame_counter,
                            Video = this,
                        };
                        db.Frames.Add(db_frame);
                    }
                    frame_counter++;
                }
                progressbar.UpdateText($"Saving...");

                await db.SaveChangesAsync();
            }
        }

        private byte[] GetMaskedImageBytesParallel(Image<Gray, byte> image, Image<Gray, byte> mask)
        {
            //Extract relevant pixels based on the Mask (if no mask, it extracts all)
            byte[] extracted_image_data = new byte[Width * Height];

            Parallel.For(0, Width * Height, index =>
            {
                int col = index % Width;
                int row = index / Width;

                int image_row = row + MaskOffsetY;
                int image_col = col + MaskOffsetX;

                double pixel_intensity = image[image_row, image_col].Intensity;
                double mask_intensity = (mask != null) ? mask[image_row, image_col].Intensity : 1.0;
                byte final_intensity = Math.Clamp((byte)(pixel_intensity * mask_intensity * 255), byte.MinValue, byte.MaxValue);

                extracted_image_data[index] = final_intensity;
            });

            return extracted_image_data;
        }

        public async Task ProcessFrames(string sqlconnection)
        {
            using (var db = new DatabaseContext(sqlconnection))
            {
                db.Attach(this);
                foreach (var frame in this.Frames.Where(f => f.FrameIndex >= 0).OrderBy(f => f.FrameIndex).ToList())
                {
                    frame.GenerateKeyPoints(this.Mask.ToImageMat());
                    frame.GenerateDescriptors();
                }
                db.SaveChanges();
            }
        }

        public void GenerateMatches(string sqlconnection, string folderpath)
        {
            using (var db = new DatabaseContext(sqlconnection))
            {
                db.Attach(this);
                var frames = this.Frames.Where(f => f.FrameIndex >= 0).OrderBy(f => f.FrameIndex).ToList();
                var prev_descriptors = frames[0].GetDescriptors();
                for (int i = 1; i < frames.Count; i++)
                {
                    var current_descriptors = frames[i].GetDescriptors();

                    var before = DateTime.Now;

                    CudaBFMatcher matcher = new CudaBFMatcher(DistanceType.Hamming);
                    matcher.Add(prev_descriptors);
                    var matches = new VectorOfDMatch();
                    matcher.Match(current_descriptors, matches);

                    Debug.WriteLine($"Matching frames {i-1} and {i} in {DateTime.Now.Subtract(before)} ({i}/{frames.Count})");

                    prev_descriptors = current_descriptors;
                }
            }
        }
    }
}
