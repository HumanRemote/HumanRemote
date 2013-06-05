using System;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace HumanRemote.Server.Pipeline
{
    /// <summary>
    /// A simple image source which provides images from an opencv capture
    /// </summary>
    class SimpleImageCaptureSource :IImageSource<SimpleImageData>
    {
        private readonly Capture _capture;
        
        public SimpleImageCaptureSource(CaptureType t)
        {
            _capture = new Capture(t);
        }    

        public SimpleImageCaptureSource(int camIndex)
        {
            _capture = new Capture(camIndex);
        }   
        
        public SimpleImageCaptureSource(string fileName)
        {
            _capture = new Capture(fileName);
        }

        public void QueryFrame()
        {
            OnFrameUpdated(new SimpleImageData(this, _capture.QueryFrame()));
        }

        public void Dispose()
        {
            if(_capture != null) _capture.Dispose();
        }

        public event Action<SimpleImageData> FrameUpdated;
        protected virtual void OnFrameUpdated(SimpleImageData obj)
        {
            Action<SimpleImageData> handler = FrameUpdated;
            if (handler != null) handler(obj);
        }
    }
}
