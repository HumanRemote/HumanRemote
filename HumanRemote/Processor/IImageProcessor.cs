using System;
using Emgu.CV;
using Emgu.CV.Structure;

namespace HumanRemote.Processor
{
    interface IImageProcessor : IDisposable
    {
        Image<Bgr, Byte> ProcessImage(Image<Bgr, Byte> img);
        void InvokeAction();
    }
}
