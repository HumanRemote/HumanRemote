using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight;

namespace HumanRemote.Server.Pipeline
{
    class ProcessingPipeline<TImageData, TSkeletonData, TSkeletonHistory, TGestureData> : ViewModelBase, IDisposable
        where TImageData : IImageData
        where TSkeletonData : ISkeletonData
        where TSkeletonHistory : ISkeletonHistory
        where TGestureData : IGestureData
    {
        private IImageProcessor<TImageData> _bodyDetector;
        private ISkeletonizer<TImageData, TSkeletonData> _skeletonizer;
        private ISkeletonProcessor<TSkeletonData, TSkeletonHistory> _skeletonProcessor;
        private IGestureRecognizer<TSkeletonHistory, TGestureData> _gestureRecognizer;
        private IGestureProcessor<TGestureData> _gestureProcessor;

        public List<IImageSource<TImageData>> ImageSources { get; private set; }

        public IImageProcessor<TImageData> BodyDetector
        {
            get { return _bodyDetector; }
            set
            {
                if (_bodyDetector == value) return;
                if (_bodyDetector != null)
                {
                    _bodyDetector.ImageProcessed -= OnBodyDetected;
                }
                _bodyDetector = value;
                if (_bodyDetector != null)
                {
                    _bodyDetector.ImageProcessed += OnBodyDetected;
                }
                RaisePropertyChanged("BodyDetector");
            }
        }

        public ISkeletonizer<TImageData, TSkeletonData> Skeletonizer
        {
            get { return _skeletonizer; }
            set
            {
                if (_skeletonizer == value) return;
                if (_skeletonizer != null)
                {
                    _skeletonizer.SkeletonProcessed -= OnSkeletonDetected;
                }
                _skeletonizer = value;
                if (_skeletonizer != null)
                {
                    _skeletonizer.SkeletonProcessed += OnSkeletonDetected;
                }
                RaisePropertyChanged("Skeletonizer");
            }
        }

        public ISkeletonProcessor<TSkeletonData, TSkeletonHistory> SkeletonProcessor
        {
            get { return _skeletonProcessor; }
            set
            {

                if (_skeletonProcessor == value) return;
                if (_skeletonProcessor != null)
                {
                    _skeletonProcessor.SkeletonProcessed -= OnSkeletonProcessed;
                }
                _skeletonProcessor = value;
                if (_skeletonProcessor != null)
                {
                    _skeletonProcessor.SkeletonProcessed += OnSkeletonProcessed;
                }
                RaisePropertyChanged("SkeletonProcessor");
            }
        }

        public IGestureRecognizer<TSkeletonHistory, TGestureData> GestureRecognizer
        {
            get { return _gestureRecognizer; }
            set
            {
                if (_gestureRecognizer == value) return;
                if (_gestureRecognizer != null)
                {
                    _gestureRecognizer.GestureRecognized -= OnGestureRecognized;
                }
                _gestureRecognizer = value;
                if (_gestureRecognizer != null)
                {
                    _gestureRecognizer.GestureRecognized += OnGestureRecognized;
                }
                RaisePropertyChanged("GestureRecognizer");
            }
        }

        public IGestureProcessor<TGestureData> GestureProcessor
        {
            get { return _gestureProcessor; }
            set
            {
                _gestureProcessor = value;
                RaisePropertyChanged("GestureProcessor");
            }
        }

        public ProcessingPipeline()
        {
            ImageSources = new List<IImageSource<TImageData>>();
        }

        public void Dispose()
        {
            foreach (var imageSource in ImageSources)
            {
                imageSource.Dispose();
            }
            if (_bodyDetector != null)
                _bodyDetector.Dispose();
            if (_skeletonizer != null)
                _skeletonizer.Dispose();
            if (_skeletonProcessor != null)
                _skeletonProcessor.Dispose();
            if (_gestureRecognizer != null)
                _gestureRecognizer.Dispose();
            if (_gestureProcessor != null)
                _gestureProcessor.Dispose();
        }

        public override void Cleanup()
        {
            Dispose();
        }

        public void AddImageSource(IImageSource<TImageData> source)
        {
            if (!ImageSources.Contains(source))
            {
                source.FrameUpdated += OnSourceFrameUpdated;
                ImageSources.Add(source);
            }
        }

        public void RemoveImageSource(IImageSource<TImageData> source)
        {
            if (ImageSources.Remove(source))
            {
                source.FrameUpdated -= OnSourceFrameUpdated;
            }
        }

        private void OnSourceFrameUpdated(TImageData data)
        {
            if (BodyDetector != null)
                BodyDetector.Process(data);
        }

        private void OnBodyDetected(TImageData data)
        {
            if (Skeletonizer != null)
                Skeletonizer.Process(data);
        }

        private void OnSkeletonDetected(TSkeletonData data)
        {
            if (SkeletonProcessor != null)
                SkeletonProcessor.Process(data);
        }

        private void OnSkeletonProcessed(TSkeletonHistory data)
        {
            if (GestureRecognizer != null)
                GestureRecognizer.Process(data);
        }

        private void OnGestureRecognized(TGestureData data)
        {
            if(GestureProcessor != null)
                GestureProcessor.Process(data);
        }
    }
}
