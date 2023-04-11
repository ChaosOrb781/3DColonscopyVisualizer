using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ColonoscopyRecreation.Entities
{
    public class KeyPoint
    {
        public int Id { get; set; }
        public int FrameId { get; set; }
        public virtual Frame Frame { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Size { get; set; }
        public float Angle { get; set; }
        public float Response { get; set; }
        public int Octave { get; set; }
        public int ClassId { get; set; }
        public byte[] Descriptors { get; set; } = new byte[0];

        public MKeyPoint AsEmguKeyPoint() 
            => new MKeyPoint()
            {
                Point = new PointF(this.X, this.Y),
                Size = this.Size,
                Angle = this.Angle,
                Response = this.Response,
                Octave = this.Octave,
                ClassId = this.ClassId
            };


        public Mat GetDescriptors()
        {
            Mat mat = new Mat(1, Descriptors.Length / 4, Emgu.CV.CvEnum.DepthType.Cv32F, 1);
            Marshal.Copy(Descriptors, 0, mat.DataPointer, Descriptors.Length);
            return mat;
        }
    }
}
