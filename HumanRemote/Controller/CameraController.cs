using System;
using System.Collections.Generic;
using HumanRemote.Camera;
using VideoInputSharp;

namespace HumanRemote.Controller
{
    internal delegate AbstractCamera CameraFactory(CameraController controller,
        int id, int cameraWidth, int cameraHeight, int frameRate, VideoInput videoInput);

    class CameraController 
    {
        private readonly VideoInput _videoInput;

        public int FrameRate { get; private set; }
        public int CameraWidth { get; private set; }
        public int CameraHeight { get; private set; }

        public List<AbstractCamera> Cameras { get; private set; }

        public CameraController(CameraFactory factory, int frameRate = 50, int cameraWidth = 640, int cameraHeight = 480)
        {
            _videoInput = new VideoInput();

            FrameRate = frameRate;
            CameraWidth = cameraWidth;
            CameraHeight = cameraHeight;
            Cameras = new List<AbstractCamera>();

            int nCameras = VideoInput.ListDevices(true);
            for (int i = 0; i < nCameras; i++)
            {
                AbstractCamera camera = factory(this, i, cameraWidth, cameraHeight, frameRate, _videoInput);
                Cameras.Add(camera);
            }
        }
    }
}
