using System;

namespace HumanRemote.Server.Pipeline
{
    /// <summary>
    /// Implement this interface to provide new image sources to the pipeline
    /// </summary>
    interface IImageSource<out T> : IDisposable
        where T : IImageData
    {
        event Action<T> FrameUpdated;
    }
}
