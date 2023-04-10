using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
        public byte[] Descriptors { get; set; }
    }
}
