using System;
using System.Collections.Generic;
using DirectShowLib;
using HumanRemote.Camera;

namespace HumanRemote.Controller
{
    internal delegate AbstractCamera CameraFactory(CameraController controller,
        int id, int cameraWidth, int cameraHeight, int frameRate, DsDevice device);

    class CameraController 
    {
        public int FrameRate { get; private set; }
        public int CameraWidth { get; private set; }
        public int CameraHeight { get; private set; }

        public List<AbstractCamera> Cameras { get; private set; }

        public CameraController(CameraFactory factory, int frameRate = 50, int cameraWidth = 640, int cameraHeight = 480)
        {
            FrameRate = frameRate;
            CameraWidth = cameraWidth;
            CameraHeight = cameraHeight;
            Cameras = new List<AbstractCamera>();

            DsDevice[] devs = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            for (int i = 0; i < devs.Length; i++)
            {
                AbstractCamera camera = factory(this, i, cameraWidth, cameraHeight, frameRate, devs[i]);
                Cameras.Add(camera);
            }
        }
    }
}
