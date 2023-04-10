using ColonoscopyRecreation.Entities;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV.Structure;
using Emgu.CV;
using Emgu.CV.Reg;
using Emgu.CV.CvEnum;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.InteropServices;

namespace ColonoscopyRecreation.Extensions
{
    public static class VideoExtensions
    {
        public static Frame GetFrame(this Mat frame)
        {
            CvInvoke.CvtColor(frame, frame, ColorConversion.Bgr2Gray);

            // Calculate the number of bytes needed to store the image data
            int bytesNeeded = frame.Width * frame.Height * frame.ElementSize;

            // Allocate a byte array to store the image data
            byte[] imageData = new byte[bytesNeeded];

            // Pin the byte array in memory to get a pointer to its data
            GCHandle handle = GCHandle.Alloc(imageData, GCHandleType.Pinned);
            IntPtr dataPointer = handle.AddrOfPinnedObject();

            // Copy the image data to the byte array
            Marshal.Copy(frame.DataPointer, imageData, 0, bytesNeeded);

            // Release the handle to the byte array
            handle.Free();

            return new Frame() { Content = imageData };
        }

        public static Mat ImageFromBytes(this byte[] bytes)
        {
            // Use Emgu.CV's Imdecode function to decode the image from the stream
            Mat image = new();
            CvInvoke.Imdecode(bytes, ImreadModes.Unchanged, image);

            // If the image is null, it means the decoding failed
            if (image == null)
            {
                throw new Exception("Failed to decode image from byte array");
            }

            return image;
        }
    }
}
