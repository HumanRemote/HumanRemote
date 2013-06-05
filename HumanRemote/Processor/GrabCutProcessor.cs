using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;

namespace HumanRemote.Processor
{
    class GrabCutProcessor : IImageProcessor
    {
        public void Dispose()
        {
        }

        public Image<Bgr, byte> ProcessImage(Image<Bgr, byte> img)
        {
            try
            {
                int numberOfIterations = 15;
                Rectangle rect = new Rectangle(0, 0, img.Width, img.Height);
                Image<Gray, byte> mask = img.GrabCut(rect, numberOfIterations);
                mask = mask.ThresholdBinary(new Gray(2), new Gray(255));
                return img.Copy(mask);  
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return img;
        }

        public void InvokeAction()
        {
        }
    }
}
