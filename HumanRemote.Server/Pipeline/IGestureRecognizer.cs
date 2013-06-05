using System;

namespace HumanRemote.Server.Pipeline
{
    interface IGestureRecognizer<TSkeletonHistory, TGestureData> : IDisposable
        where TSkeletonHistory : ISkeletonHistory
        where TGestureData : IGestureData
    {
        void Process(TSkeletonHistory data);
        event Action<TGestureData> GestureRecognized;
    }
}
