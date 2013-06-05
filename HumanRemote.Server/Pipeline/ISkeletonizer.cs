using System;

namespace HumanRemote.Server.Pipeline
{
    interface ISkeletonizer<in TImageData, out TSkeletonData> : IDisposable
        where TImageData : IImageData
        where TSkeletonData : ISkeletonData
    {
        void Process(TImageData input);
        event Action<TSkeletonData> SkeletonProcessed;
    }
}