using System;

namespace HumanRemote.Server.Pipeline
{
    interface IGestureProcessor<TGestureData> : IDisposable where TGestureData : IGestureData 
    {
        void Process(TGestureData data);
    }
}
