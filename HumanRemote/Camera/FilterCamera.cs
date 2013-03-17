using HumanRemote.Processor;
using OpenCvSharp;
using VideoInputSharp;

namespace HumanRemote.Camera
{
    class FilterCamera : DirectShowCamera
    {
        protected IplImage _realFrame;

        public IImageProcessor ImageProcessor { get; set; }

        public FilterCamera(int id, int width, int height, int frameRate, VideoInput input, IImageProcessor processor)
            : base(id, width, height, frameRate, input)
        {
            ImageProcessor = processor;
        }

        public override bool SetupDevice()
        {
            _realFrame = Cv.CreateImage(new CvSize(_videoInput.GetWidth(Id), _videoInput.GetHeight(Id)), BitDepth.U8, 3);
            return base.SetupDevice();
        }

        public override IplImage GetFrame()
        {
            if (_videoInput.IsFrameNew(Id))
            {
                _videoInput.GetPixels(Id, _cameraFrame.ImageData, false, true);
                if (ImageProcessor != null)
                {
                    _realFrame = ImageProcessor.ProcessImage(_cameraFrame);
                }
            }
            return _realFrame;
        }

    }
}
