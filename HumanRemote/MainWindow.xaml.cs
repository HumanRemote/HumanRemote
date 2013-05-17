using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using DirectShowLib;
using HumanRemote.Camera;
using HumanRemote.Controller;
using HumanRemote.Processor;

namespace HumanRemote
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static Dispatcher GuiDispatcher;

        private readonly CaptureController _captureController;
        private readonly CameraController _cameraController;
        private readonly List<CameraWindow> _windows;
        private DateTime _start;

        public MainWindow()
        {
            InitializeComponent();
            GuiDispatcher = Dispatcher;

            try
            {
                _windows = new List<CameraWindow>();

                _cameraController = new CameraController(CreateCamera, 25);

                _captureController = new CaptureController(100);
                _captureController.TimeFrameElapsed += _captureController_TimeFrameElapsed;
                CreateSubWindows();
                _start = DateTime.Now;
                _captureController.Start();
                _captureController.CaptureStatus = CaptureStatus.Recording;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
           
        }

        private AbstractCamera CreateCamera(CameraController controller, int id, int camerawidth, int cameraheight,
                                            int framerate, DsDevice device)
        {
            return new FilterCamera(id, camerawidth, cameraheight, framerate, device, new BackgroundSubtractProcessor());
        }


        private void _captureController_TimeFrameElapsed(object sender, EventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(RefreshCaptureTime));
            }
            else
            {
                RefreshCaptureTime();
            }
            GC.Collect();
        }

        private void CreateSubWindows()
        {
            foreach (AbstractCamera camera in _cameraController.Cameras)
            {
                VideoController videoController = new VideoController(camera);
                _captureController.AddVideoController(videoController);

                CameraWindow wnd = new CameraWindow();
                wnd.Title = camera.Name;
                wnd.SetVideoController(videoController);
                wnd.Show();
                _windows.Add(wnd);
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            _captureController.Dispose();
            foreach (CameraWindow window in _windows)
            {
                window.Close();
            }
        }

        private void RefreshCaptureTime()
        {
            elapsedBox.Text = (DateTime.Now - _start).ToString();
        }
    }

}