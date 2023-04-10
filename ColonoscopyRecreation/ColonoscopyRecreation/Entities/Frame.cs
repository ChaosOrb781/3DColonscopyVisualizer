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
        public int DescriptorCount { get; set; }
        public int DescriptorSize { get; set; }
        public byte[] Descriptors { get; set; }
        public int KeyPointsSize { get; set; }
        public virtual List<KeyPoint> KeyPoints { get; set; } = new List<KeyPoint>();

        public Mat GetDescriptors()
        {
            Image<Gray, float> image = new Image<Gray, float>(DescriptorCount,DescriptorSize);
            image.Bytes = Descriptors;
            Mat mat = image.Mat;
            return mat;
        }

        public Mat ToImageMat()
        {
            // Create a new Mat object from the byte array
            Mat imageMat = new Mat(Video.Height, Video.Width, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
            imageMat.SetTo(Content);
            return imageMat;
        }

        public Image<Gray, byte> ToImage()
            => this.ToImageMat().ToImage<Gray, byte>();

        public async Task GenerateKeyFeatures(string sqlconnection, Mat mask = null!, int n_features = 0, int m_octaves = 3)
        {
            ArgumentNullException.ThrowIfNull(this.Video);
            
            SIFT sift = new SIFT(n_features, m_octaves);
            VectorOfKeyPoint keypoints = new VectorOfKeyPoint();
            Mat descriptors = new Mat();

            sift.DetectAndCompute(this.ToImageMat(), mask, keypoints, descriptors, false);

            using (var db = new DatabaseContext(sqlconnection))
            {
                db.Attach(this);

                this.KeyPoints.AddRange(
                    keypoints.ToArray().Select((kp, i) => new KeyPoint() 
                    { 
                        Frame = this,
                        X = kp.Point.X, 
                        Y = kp.Point.Y,
                        Octave = kp.Octave,
                        Size = kp.Size,
                        Angle = kp.Angle,
                        ClassId = kp.ClassId,
                        Response = kp.Response,
                        Descriptors = descriptors.Row(i).GetRawData()
                    }));
                await db.SaveChangesAsync();
            }
        }
    }
}
