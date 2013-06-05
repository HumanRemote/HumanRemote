using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.GPU;
using Emgu.CV.Structure;
using Emgu.CV.VideoSurveillance;

namespace HumanRemote.Server.Pipeline
{
    class HaarDetectorImageProcessor : IImageProcessor<SimpleImageData>
    {
        private readonly Dictionary<IImageSource<IImageData>, HaarDetectorImageProcessorData> _data = new Dictionary<IImageSource<IImageData>, HaarDetectorImageProcessorData>();

        public void Process(SimpleImageData input)
        {
            lock (this)
            {
                var data = GetProcessorData(input);
                var result = data.Process(input);
                input.Image = result;
                OnImageProcessed(input);
            }
        }

        public void Dispose()
        {
            foreach (var value in _data.Values)
            {
                value.Dispose();
            }
        }

        private HaarDetectorImageProcessorData GetProcessorData(SimpleImageData data)
        {
            if (!_data.ContainsKey(data.Source))
            {
                _data.Add(data.Source, new HaarDetectorImageProcessorData(this));
            }
            return _data[data.Source];
        }

        public event Action<SimpleImageData> ImageProcessed;
        protected virtual void OnImageProcessed(SimpleImageData obj)
        {
            Action<SimpleImageData> handler = ImageProcessed;
            if (handler != null) handler(obj);
        }
        public event Action<SimpleImageData> PreImageProcessed;
        protected virtual void OnPreImageProcessed(SimpleImageData obj)
        {
            Action<SimpleImageData> handler = PreImageProcessed;
            if (handler != null) handler(obj);
        }

        class HaarDetectorImageProcessorData : IDisposable
        {
            private readonly HaarDetectorImageProcessor _processor;
            private readonly BackgroundSubtractorMOG2 _bg;
            private static GpuCascadeClassifier _nose;
            private static GpuCascadeClassifier _profile;
            private static GpuCascadeClassifier _hs;
            private static GpuCascadeClassifier _eye;
            private static GpuCascadeClassifier _hand;
            private static GpuCascadeClassifier _palm;
            private static MCvFont _font;

            static HaarDetectorImageProcessorData()
            {
                _nose = new GpuCascadeClassifier("Cascades/haarcascade_frontalface_default.xml");
                _profile = new GpuCascadeClassifier("Cascades/haarcascade_profileface.xml");
                _eye = new GpuCascadeClassifier("Cascades/haarcascade_eye.xml");
                _hs = new GpuCascadeClassifier("Cascades/HS.xml");
                _hand = new GpuCascadeClassifier("Cascades/Hand.Cascade.1.xml");
                _palm = new GpuCascadeClassifier("Cascades/palm.xml");
                _font = new MCvFont(FONT.CV_FONT_HERSHEY_COMPLEX, 1, 1);
            }

            public HaarDetectorImageProcessorData(HaarDetectorImageProcessor processor)
            {
                _processor = processor;
            }

            public Image<Bgr, byte> Process(SimpleImageData data)
            {
                _processor.OnPreImageProcessed(data);

                using (GpuImage<Gray, byte> gray = new GpuImage<Gray, byte>(data.Image.Convert<Gray, byte>()))
                {

                    var results = _nose.DetectMultiScale(gray, 1.1, 10, new Size(20, 20));
                    var headResults = results;
                    foreach (var rectangle in results)
                    {
                        data.Image.Draw(new CircleF(new PointF(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2), 10), new Bgr(Color.Red), 2);
                        data.Image.Draw("Head", ref _font,
                                        new Point(rectangle.X + rectangle.Width / 2 + 15, rectangle.Y + rectangle.Height / 2), new Bgr(Color.Red));
                        data.Image.Draw(new CircleF(new PointF(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height), 10), new Bgr(Color.Green), 2);
                        data.Image.Draw("Neck", ref _font,
                                      new Point(rectangle.X + rectangle.Width / 2 + 15, rectangle.Y + rectangle.Height), new Bgr(Color.Green));
                    }

                    results = _hs.DetectMultiScale(gray, 1.1, 10, new Size(20, 20));
                    foreach (var rectangle in results)
                    {
                        data.Image.Draw(rectangle, new Bgr(Color.Blue), 2);
                        data.Image.Draw(new CircleF(new PointF(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height), 10), new Bgr(Color.Blue), 2);
                        data.Image.Draw("Body", ref _font,
                                     new Point(rectangle.X + rectangle.Width / 2 + 15, rectangle.Y + rectangle.Height), new Bgr(Color.Blue));
                    }

                    //if (headResults.Length == 0)
                    //{
                    //    try
                    //    {
                    //        results = _profile.DetectMultiScale(gray, 1.2, 4, new Size(20, 20));
                    //        foreach (var rectangle in results)
                    //        {
                    //            data.Image.Draw(
                    //                new CircleF(
                    //                    new PointF(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2),
                    //                    10), new Bgr(Color.Orange), 2);
                    //            data.Image.Draw("Head", ref _font,
                    //                            new Point(rectangle.X + 15,
                    //                                      rectangle.Y + rectangle.Height / 2), new Bgr(Color.Orange));
                    //            data.Image.Draw(
                    //            new CircleF(
                    //                new PointF(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height),
                    //                10), new Bgr(Color.BlueViolet), 2);
                    //            data.Image.Draw("Neck", ref _font,
                    //                            new Point(rectangle.X + rectangle.Width / 2 + 30,
                    //                                      rectangle.Y + rectangle.Height), new Bgr(Color.BlueViolet));
                    //        }
                    //    }
                    //    catch (Exception e)
                    //    {
                    //        Console.WriteLine(e);
                    //    }
                    //}
                    //try
                    //{
                    //    results = _hand.DetectMultiScale(gray, 1.2, 4, new Size(20, 20));
                    //    foreach (var rectangle in results)
                    //    {
                    //        data.Image.Draw(new CircleF(new PointF(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2), 10), new Bgr(Color.DarkGoldenrod), 2);
                    //        data.Image.Draw("Hand", ref _font,
                    //                     new Point(rectangle.X + rectangle.Width / 2 + 15, rectangle.Y + rectangle.Height / 2), new Bgr(Color.DarkGoldenrod));
                    //    }
                    //}
                    //catch (Exception e)
                    //{
                    //    Console.WriteLine(e);
                    //}

                }

                return data.Image;
            }

            public void Dispose()
            {
                if (_bg != null) _bg.Dispose();
                if (_nose != null)
                {
                    _nose.Dispose();
                    _nose = null;
                }
            }
        }
    }
}
