using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
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

        private Dictionary<string, IImageProcessor> _processorsSelected = new Dictionary<string, IImageProcessor>();
        private Dictionary<string, Type> _processors = new Dictionary<string, Type>();

        public MainWindow()
        {
            InitializeComponent();
            GuiDispatcher = Dispatcher;

            try
            {
                _windows = new List<CameraWindow>();

                _cameraController = new CameraController(CreateCamera, 25);

<<<<<<< HEAD
            _captureController = new CaptureController(100);
            _captureController.TimeFrameElapsed += _captureController_TimeFrameElapsed;
            CreateSubWindows();
            _start = DateTime.Now;
            _captureController.Start();
            _captureController.CaptureStatus = CaptureStatus.Recording;
            InitProcessorsList();
        }

        public void InitProcessorsList()
        {
            _processors.Add("HandDetectProcessor", typeof(HandDetectProcessor));
            filters.Items.Add("HandDetectProcessor");
=======
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
           
>>>>>>> 4b54db3aeb71bb980a9a4aedf7c8af6fa415583b
        }

        private AbstractCamera CreateCamera(CameraController controller, int id, int camerawidth, int cameraheight,
                                            int framerate, DsDevice device)
        {
<<<<<<< HEAD
            return new FilterCamera(id, camerawidth, cameraheight, framerate, videoinput,
                                    new MultipleFilterProcessor());
=======
            return new FilterCamera(id, camerawidth, cameraheight, framerate, device, new BackgroundSubtractProcessor());
>>>>>>> 4b54db3aeb71bb980a9a4aedf7c8af6fa415583b
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

        private void AddFilter(object sender, RoutedEventArgs e)
        {
            string toAdd = (string) filters.SelectedItem;
            selectedFilters.Items.Add(toAdd);
            _processorsSelected.Add(toAdd, (IImageProcessor)Activator.CreateInstance(_processors[toAdd]));
        }

        private void RemoveFilter(object sender, RoutedEventArgs e)
        {
            _processorsSelected[(string) selectedFilters.SelectedItem].Dispose();
            _processorsSelected.Remove((string) selectedFilters.SelectedItem);
            selectedFilters.Items.Remove(selectedFilters.SelectedItem);
        }
    }

}