using System;
using DirectShowLib;
using Emgu.CV;
using Emgu.CV.Structure;

namespace HumanRemote.Camera
{
    class DirectShowCamera : AbstractCamera
    {
        protected Capture _videoInput;
        protected DateTime _lastUpdate;
        protected Image<Bgr, Byte> _cameraFrame;

        public DirectShowCamera(int id, int width, int height, int frameRate, DsDevice device)
            : base(id, width, height, frameRate)
        {
            SetupDevice();
            _videoInput = new Capture(id);
            Name = device.Name;
        }

        public override Image<Bgr, Byte> GetFrame()
        {
            if ((DateTime.Now - _lastUpdate) > TimeSpan.FromMilliseconds(200))
            {
                _cameraFrame = _videoInput.QueryFrame();
            }
            return _cameraFrame;
        }

        public override bool SetupDevice()
        {
            return true;
        }

        public override void StopDevice()
        {
            _videoInput.Dispose();
        }
    }
}
 