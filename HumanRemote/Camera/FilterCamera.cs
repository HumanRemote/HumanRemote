using System;
using DirectShowLib;
using Emgu.CV;
using Emgu.CV.Structure;
using HumanRemote.Processor;

namespace HumanRemote.Camera
{
    class FilterCamera : DirectShowCamera
    {
        protected Image<Bgr, Byte> _realFrame;

        public FilterCamera(int id, int width, int height, int frameRate, DsDevice device, IImageProcessor imageProcessor) : base(id, width, height, frameRate, device)
        {
            ImageProcessor = imageProcessor;
        }

        public IImageProcessor ImageProcessor { get; set; }


        public override Image<Bgr, Byte> GetFrame()
        {
            if ((DateTime.Now - _lastUpdate) > TimeSpan.FromMilliseconds(200))
            {
                _cameraFrame = _videoInput.QueryFrame();
                if (ImageProcessor != null)
                {
                    _realFrame = ImageProcessor.ProcessImage(_cameraFrame);
                }
                else
                {
                    _realFrame = _cameraFrame;
                }
            }
            return _realFrame;
        }

        public override void InvokeAction()
        {
            if(ImageProcessor != null)
            {
                ImageProcessor.InvokeAction();
            }
        }

    }
}
