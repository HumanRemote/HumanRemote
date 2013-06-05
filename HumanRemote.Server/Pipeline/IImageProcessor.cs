using System;

namespace HumanRemote.Server.Pipeline
{
    /// <summary>
    /// Implement this class to create new image processors 
    /// like filters
    /// </summary>
    interface IImageProcessor<T> : IDisposable
        where T : IImageData
    {
        void Process(T input);
        event Action<T> PreImageProcessed;
        event Action<T> ImageProcessed;
    }
}
