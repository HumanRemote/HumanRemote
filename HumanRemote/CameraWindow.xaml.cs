using HumanRemote.Controller;

namespace HumanRemote
{
    /// <summary>
    /// Interaction logic for CameraWindow.xaml
    /// </summary>
    partial class CameraWindow
    {
        private VideoController _videoController;

        public CameraWindow()
        {
            InitializeComponent();
        }

        public void SetVideoController(VideoController videoController)
        {
            if(_videoController != null)
            {
                _videoController.FrameUpdated -= OnFrameUpdated;
            }
            _videoController = videoController;
            _videoController.FrameUpdated += OnFrameUpdated;
        }

        private void OnFrameUpdated(OpenCvSharp.IplImage obj)
        {
            imageWidget.RefreshImage(obj);
        }
    }
}
