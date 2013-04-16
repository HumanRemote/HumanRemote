using System;
using OpenCvSharp;

namespace HumanRemote.Processor
{
    public interface IImageProcessor : IDisposable
    {
        IplImage ProcessImage(IplImage img);
        void InvokeAction();
    }
}
