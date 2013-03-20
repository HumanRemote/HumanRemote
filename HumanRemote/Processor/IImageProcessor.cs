using System;
using OpenCvSharp;

namespace HumanRemote.Processor
{
    interface IImageProcessor : IDisposable
    {
        IplImage ProcessImage(IplImage img);
        void InvokeAction();
    }
}
