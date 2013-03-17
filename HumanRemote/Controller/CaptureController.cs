using System;
using System.Collections.Generic;
using System.Threading;

namespace HumanRemote.Controller
{
    public enum CaptureStatus
    {
        Recording,
        Playing,
        Stop
    }

    class CaptureController : IDisposable
    {
        private readonly List<VideoController> _videoControllers = new List<VideoController>();
        private Timer _timer;
        private readonly int _updateTimer;

        public CaptureStatus CaptureStatus { get; set; }

        public void AddVideoController(VideoController videoController)
        {
            _videoControllers.Add(videoController);
        }

        public CaptureController(int updateTimer)
        {
            CaptureStatus = CaptureStatus.Playing; 
            _updateTimer = updateTimer;
        }

        public void Start()
        {
            _timer = new Timer(state => TimerEvent(), null, 1000, _updateTimer);
        }

        public void TimerEvent()
        {
            if (CaptureStatus != CaptureStatus.Stop)
            {
                if (_videoControllers.Count > 0)
                {
                    foreach (VideoController videoController in _videoControllers)
                    {
                        videoController.Resume();
                    }
                    OnTimeFrameElapsed(EventArgs.Empty);
                }
            }
        }

        public event EventHandler TimeFrameElapsed;
        protected virtual void OnTimeFrameElapsed(EventArgs e)
        {
            EventHandler handler = TimeFrameElapsed;
            if (handler != null) handler(this, e);
        }

        public void Stop()
        {
            if (_timer != null)
            {
                _timer.Dispose();
            }
        }

        public void Dispose()
        {
            foreach (VideoController videoController in _videoControllers)
            {
                if (videoController != null)
                    videoController.Dispose();
            }
        }
    }
}
