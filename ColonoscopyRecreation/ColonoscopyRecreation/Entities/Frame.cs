using Emgu.CV.CvEnum;
using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Emgu.CV.ML.KNearest;
using Emgu.CV.Features2D;
using Emgu.CV.Util;
using ColonoscopyRecreation.Database;
using Emgu.CV.Cuda;
using System.Runtime.InteropServices;

namespace ColonoscopyRecreation.Entities
{
    public enum FrameFormat
    {
        RGB,
        RGBA,
        Gray
    }
    public class Frame
    {
        public int Id { get; set; }
        public int VideoId { get; set; }
        public virtual Video Video { get; set; }
        public int FrameIndex { get; set; }
        public byte[] Content { get; set; }
        public virtual List<KeyPoint> KeyPoints { get; set; } = new List<KeyPoint>();


        public Mat ToImageMat()
        {
            // Create a new Mat object from the byte array
            Mat imageMat = new Mat(Video.Height, Video.Width, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
            imageMat.SetTo(Content);
            return imageMat;
        }

        public Image<Gray, byte> ToImage()
            => this.ToImageMat().ToImage<Gray, byte>();

        public Image<Gray, byte> ToImageWithKeypoints(Features2DToolbox.KeypointDrawType keypointdrawtype = Features2DToolbox.KeypointDrawType.Default)
        {
            var image = this.ToImage();
            Features2DToolbox.DrawKeypoints(image, this.GetKeyPoints(), image, new Bgr(0, 0, 1.0), keypointdrawtype);
            return image;
        }

        public void GenerateKeyPoints(Mat mask = null!, int n_features = 0, int m_octaves = 3)
        {
            ArgumentNullException.ThrowIfNull(this.Video);

            SIFT sift = new SIFT(n_features, m_octaves);
            var keypoints = sift.Detect(this.ToImageMat(), mask);

            this.KeyPoints = keypoints.ToArray().Select(kp=> new KeyPoint()
            {
                Frame = this,
                X = kp.Point.X,
                Y = kp.Point.Y,
                Octave = kp.Octave,
                Size = kp.Size,
                Angle = kp.Angle,
                ClassId = kp.ClassId,
                Response = kp.Response
            }).ToList();
        }

        public void GenerateDescriptors(int n_features = 0, int m_octaves = 3)
        {
            SIFT sift = new SIFT(n_features, m_octaves);
            Mat descriptors = new Mat();
            sift.Compute(this.ToImageMat(), new VectorOfKeyPoint(KeyPoints.Select(kp => kp.AsEmguKeyPoint()).ToArray()), descriptors);

            for (int i = 0; i < KeyPoints.Count; i++)
                KeyPoints[i].Descriptors = descriptors.Row(i).GetRawData();
        }

        public VectorOfKeyPoint GetKeyPoints() => new VectorOfKeyPoint(KeyPoints.Select(kp => kp.AsEmguKeyPoint()).ToArray());

        public Mat GetDescriptors()
        {
            ArgumentNullException.ThrowIfNull(KeyPoints);
            if (KeyPoints.Count == 0)
                throw new InvalidOperationException("No keypoints in the frame, run GenerateKeyPoints before attempting to get descriptors");
            Mat descriptors = new Mat(KeyPoints.Count, KeyPoints.First().Descriptors.Length / 4, DepthType.Cv32F, 1);

            IntPtr desc_ptr = descriptors.DataPointer;
            int step = descriptors.Step;
            for (int i = 0; i < this.KeyPoints.Count; i++)
                Marshal.Copy(this.KeyPoints[i].Descriptors, 0, desc_ptr + step * i, this.KeyPoints[i].Descriptors.Length);

            return descriptors;
        }
    }
}
