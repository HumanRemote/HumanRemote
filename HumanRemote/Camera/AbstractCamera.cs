using OpenCvSharp;

namespace HumanRemote.Camera
{
    public abstract class AbstractCamera 
    {
        public int Id { get; protected set; }
        public virtual string Name { get; set; }
        public virtual int Width { get; set; }
        public virtual int Height { get; set; }
        public virtual int FrameRate { get; set; }

        public abstract IplImage GetFrame();
        public abstract bool SetupDevice();
        public abstract void StopDevice();

        protected AbstractCamera(int id, int width, int height, int frameRate)
        {
            Id = id;
            Width = width;
            Height = height;
            FrameRate = frameRate;
        }

        public virtual void Dispose()
        {
            StopDevice();
        }

        public virtual void InvokeAction()
        {
            
        }
    }
}
