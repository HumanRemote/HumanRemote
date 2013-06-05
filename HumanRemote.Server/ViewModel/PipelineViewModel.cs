using System;
using System.Threading;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using HumanRemote.Server.Pipeline;
using HumanRemote.Server.Utils;

namespace HumanRemote.Server.ViewModel
{
    public class PipelineViewModel : ViewModelBase, IDisposable
    {
        private const int FrameRate = 25;
        private readonly ProcessingPipeline<SimpleImageData, ISkeletonData, ISkeletonHistory, IGestureData> _pipeline;
        private readonly Timer _timer;

        private readonly SimpleImageCaptureSource _mainSource;

        private Image<Bgr,byte> _originalOpenCvImage;
        private BitmapSource _originalImage;
        private BitmapSource _bodyDetectionImage;
        private BitmapSource _originalImageBodyMasked;
        private BitmapSource _preDetectionImage;


        public BitmapSource BodyDetectionImage
        {
            get { return _bodyDetectionImage; }
            set
            {
                _bodyDetectionImage = value;
                if (_bodyDetectionImage != null)
                    _bodyDetectionImage.Freeze();
                RaisePropertyChanged("BodyDetectionImage");
            }
        }

        public BitmapSource PreDetectionImage
        {
            get { return _preDetectionImage; }
            set
            {
                _preDetectionImage = value;
                if(_preDetectionImage != null)
                    _preDetectionImage.Freeze();
                RaisePropertyChanged("PreDetectionImage");
            }
        }

        public BitmapSource OriginalImageBodyMasked
        {
            get { return _originalImageBodyMasked; }
            set
            {
                _originalImageBodyMasked = value;
                if (_originalImageBodyMasked != null)
                    _originalImageBodyMasked.Freeze();
                RaisePropertyChanged("OriginalImageBodyMasked");
            }
        }

        public BitmapSource OriginalImage
        {
            get { return _originalImage; }
            set
            {
                _originalImage = value;
                if (_originalImage != null)
                    _originalImage.Freeze();
                RaisePropertyChanged("OriginalImage");
            }
        }

        public PipelineViewModel()
        {

            _pipeline = new ProcessingPipeline<SimpleImageData, ISkeletonData, ISkeletonHistory, IGestureData>();
            _mainSource = new SimpleImageCaptureSource(0);
            _mainSource.FrameUpdated += OnSourceUpdated;
            _pipeline.AddImageSource(_mainSource);
            _pipeline.BodyDetector = new HaarDetectorImageProcessor();
            _pipeline.BodyDetector.ImageProcessed += OnBodyDetectorImageProcessed;
            _pipeline.BodyDetector.PreImageProcessed += OnPreBodyDetectorImageProcess;

            if (!IsInDesignMode)
                _timer = new Timer(OnTimerUpdate, null, 0, 1000 / FrameRate);
        }

        public override void Cleanup()
        {
            Dispose();
        }

        private void OnSourceUpdated(SimpleImageData obj)
        {
            _originalOpenCvImage = obj.OriginalImage.Copy();
            OriginalImage = obj.OriginalImage.ToBitmapSource();
        }

        private void OnPreBodyDetectorImageProcess(SimpleImageData obj)
        {
            PreDetectionImage = obj.Image.ToBitmapSource();
        }

        private void OnBodyDetectorImageProcessed(SimpleImageData obj)
        {
            BodyDetectionImage = obj.Image.ToBitmapSource();
            OriginalImageBodyMasked = (_originalOpenCvImage.Sub(obj.Image.Not())).ToBitmapSource();
        }

        private void OnTimerUpdate(object state)
        {
            _mainSource.QueryFrame();
        }

        public void Dispose()
        {
            _timer.Dispose();
            _pipeline.Dispose();
        }

        protected override void RaisePropertyChanged(string propertyName)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => base.RaisePropertyChanged(propertyName));
        }
        protected override void RaisePropertyChanged<T>(string propertyName, T oldValue, T newValue, bool broadcast)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => base.RaisePropertyChanged<T>(propertyName, oldValue, newValue, broadcast));
        }
    }
}