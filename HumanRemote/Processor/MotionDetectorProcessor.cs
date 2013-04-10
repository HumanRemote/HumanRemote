using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;

namespace HumanRemote.Processor
{
    class MotionDetectorProcessor : IImageProcessor
    {
        private Image<Bgr, byte> _current;
        private Image<Bgr, byte> _background; 
        public void Dispose()
        {
                
        }

        public Image<Bgr, byte> ProcessImage(Image<Bgr, byte> img)
        {
            _current = img.Clone();

            if(_background != null)
            {
                var grayScale = img.Convert<Bgr, byte>();
                var sub = grayScale - _background;
                return sub.Convert<Bgr, byte>();
            }
            else
            {
                return _current;
            }
        }

        public void InvokeAction()
        {
            _background = _current.Convert<Bgr, byte>();
        }
    }
}
