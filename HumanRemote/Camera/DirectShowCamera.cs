using OpenCvSharp;
using VideoInputSharp;

namespace HumanRemote.Camera
{
    class DirectShowCamera : AbstractCamera
    {
        protected readonly VideoInput _videoInput;
        protected IplImage _cameraFrame;

        public DirectShowCamera(int id, int width, int height, int frameRate, VideoInput input)
            : base(id, width, height, frameRate)
        {
            _videoInput = input;
            SetupDevice();
            Name = VideoInput.GetDeviceName(Id);
        }

        public override IplImage GetFrame()
        {
            if (_videoInput.IsFrameNew(Id))
            {
                _videoInput.GetPixels(Id, _cameraFrame.ImageData, false, true);
            }
            return _cameraFrame;
        }

        public override bool SetupDevice()
        {
            if (_videoInput.IsDeviceSetup(Id))
            {
                StopDevice();
            }
            _videoInput.SetupDevice(Id, Width, Height);
            _cameraFrame = Cv.CreateImage(new CvSize(_videoInput.GetWidth(Id), _videoInput.GetHeight(Id)), BitDepth.U8, 3);

            _videoInput.SetIdealFramerate(Id, FrameRate);
            _videoInput.SetAutoReconnectOnFreeze(Id, true, 3);
            return true;
        }

        public override void StopDevice()
        {
            if (_videoInput.IsDeviceSetup(Id))
            {
                _videoInput.StopDevice(Id);
            }
        }
    }
}
 