using System;

namespace HumanRemote.Server.Pipeline
{
    interface ISkeletonProcessor<TSkeletonData, TSkeletonHistory> : IDisposable
        where TSkeletonData : ISkeletonData
        where TSkeletonHistory : ISkeletonHistory
    {
        void Process(TSkeletonData input);
        event Action<TSkeletonHistory> SkeletonProcessed;
    }
}
