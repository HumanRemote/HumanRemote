using System.Windows.Input;
using Emgu.CV;
using Emgu.CV.Structure;
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

        public void OnFrameUpdated(Image<Bgr, byte> obj)
        {
            imageWidget.RefreshImage(obj);
        }

        private void OnClick(object sender, MouseButtonEventArgs e)
        {
            if(_videoController!=null)
            _videoController.InvokeAction();
        }
    }
}
