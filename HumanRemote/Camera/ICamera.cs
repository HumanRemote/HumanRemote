using System;
using OpenCvSharp;

namespace HumanRemote.Camera
{
    interface ICamera : IDisposable
    {
        int Id { get; }
        IplImage GetFrame();
        bool SetupDevice();
        void StopDevice();
        void SetResolution(int width, int height);
    }
}
