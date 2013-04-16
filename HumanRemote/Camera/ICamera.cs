using System;
using Emgu.CV;
using Emgu.CV.Structure;

namespace HumanRemote.Camera
{
    interface ICamera : IDisposable
    {
        int Id { get; }
        Image<Bgr, Byte> GetFrame();
        bool SetupDevice();
        void StopDevice();
        void SetResolution(int width, int height);
    }
}
