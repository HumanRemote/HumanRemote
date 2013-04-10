using System;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using HumanRemote.Camera;

namespace HumanRemote.Controller
{
    public class VideoController : IDisposable
    {
        private Thread _thread;
        private bool _running;
        private readonly AutoResetEvent _autoReset = new AutoResetEvent(true);

        public AbstractCamera Camera { get; private set; }
        
        public VideoController(AbstractCamera camera = null)
        {
            Camera = camera;
        }

        public void Start()
        {
            _running = true;
            _thread = new Thread(Run);
            _thread.Start();
        }

        public void Kill()
        {
            _running = false;
            _autoReset.Set();
            _thread.Interrupt();
        }

        public void Suspend()
        {
            _autoReset.Reset();
        }

        public void Resume()
        {
            _autoReset.Set();
            if (!_running)
            {
                Start();
            }
        }

        private void Run()
        {
            while (_running)
            {
                try
                {
                    var currentFrame = Camera.GetFrame();
                    OnFrameUpdated(currentFrame);
                    Suspend();
                    _autoReset.WaitOne();
                }
                catch (ThreadInterruptedException )
                {
                    break;
                }
            }
        }

        public event Action<Image<Bgr, byte>> FrameUpdated;
        protected virtual void OnFrameUpdated(Image<Bgr, byte> obj)
        {
            Action<Image<Bgr, byte>> handler = FrameUpdated;
            if (handler != null) handler(obj);
        }

        public void Dispose()
        {
            Kill();
        }

        public void InvokeAction()
        {
            Camera.InvokeAction();
        }
    }
}
