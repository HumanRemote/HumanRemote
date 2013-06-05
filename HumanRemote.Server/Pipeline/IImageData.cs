using System;
using Emgu.CV;
using Emgu.CV.Structure;

namespace HumanRemote.Server.Pipeline
{
    /// <summary>
    /// Implement this interface to create a new image data container for the pipeline
    /// </summary>
    interface IImageData : IDisposable
    {
        IImageSource<IImageData> Source { get; }
        Image<Bgr, byte> Image { get; set; }
    }
}