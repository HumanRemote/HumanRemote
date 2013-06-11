using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.GPU;
using Emgu.CV.Structure;
using Emgu.CV.VideoSurveillance;
using Emgu.Util;

namespace HumanRemote.Server.Pipeline
{
    class SilhouetteExtractingImageProcessor : IImageProcessor<SimpleImageData>
    {
        private readonly Dictionary<IImageSource<IImageData>, SilhouetteExtractingImageProcessorData> _data =
            new Dictionary<IImageSource<IImageData>, SilhouetteExtractingImageProcessorData>();

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

        private SilhouetteExtractingImageProcessorData GetProcessorData(SimpleImageData data)
        {

            if (!_data.ContainsKey(data.Source))
            {
                _data.Add(data.Source, new SilhouetteExtractingImageProcessorData(this));
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

        class SilhouetteExtractingImageProcessorData : IDisposable
        {
            private readonly SilhouetteExtractingImageProcessor _processor;
            private readonly BackgroundSubtractorMOG2 _bg;

            private static GpuCascadeClassifier _hs;

            static SilhouetteExtractingImageProcessorData()
            {
                _hs = new GpuCascadeClassifier("Cascades/HS.xml");
            }

            public SilhouetteExtractingImageProcessorData(SilhouetteExtractingImageProcessor processor)
            {
                _processor = processor;
                _bg = new BackgroundSubtractorMOG2(10000, 2 * 2, true);
                BackgroundSubtractorMOG2Data data = (BackgroundSubtractorMOG2Data)Marshal.PtrToStructure(_bg.Ptr, typeof(BackgroundSubtractorMOG2Data));
                data.nmixtures = 3;
            }

            public Image<Bgr, byte> Process(SimpleImageData data)
            {
                Image<Bgr, byte> image = data.Image;

                data.Image = image;
                //
                // Phase 1: Denoise
                image = Denoise(image);
                _processor.OnPreImageProcessed(data);

                //
                // Phase 2: Background Subtraction for Movement Detection
                image = BackgroundSubtraction(image);

                //
                // Phase 3: Additional Noise Reduction
                image = ReduceNoise(image);

                //
                // Phase 4: Blob Detection
                image = FillBlobs(image);

                // 
                // Phase 5: Mark Body
                image = MarkBody(image, data.OriginalImage);

                return image;
            }

            private Image<Bgr, byte> MarkBody(Image<Bgr, byte> image, Image<Bgr, byte> original)
            {
                using (GpuImage<Gray, byte> gray = new GpuImage<Gray, byte>(original.Convert<Gray, byte>()))
                {
                    var results = _hs.DetectMultiScale(gray, 1.1, 10, new Size(20, 20)).OrderByDescending(s => s.Width * s.Height).Take(1).ToArray();
                    if (results.Length == 1)
                    {
                        List<Contour<Point>> temp;
                        var bodyContours = DetectBlobs(image.Convert<Gray, byte>().Not(), out temp);
                        if (bodyContours != null)
                        {
                            bodyContours = bodyContours.OrderByDescending(item => item.Total).Take(10);

                            var bodyRect = results[0];
                            Point body = new Point(bodyRect.X + bodyRect.Width / 2, bodyRect.Y + bodyRect.Height);

                            // Find Left (left on the image) 120°-230°
                            Point elbow;
                            int elbowLength = 120;
                            for (int angle = 230; angle >= 120; angle -= 10)
                            {
                                // sin(angle) = offsetY / elbowLength
                                // cos(angle) = offsetX / elbowLength

                                // offsetY = sin(angle) * elbowLength
                                // offsetX = cos(angle) * elbowLength

                                double offsetY = Math.Sin(angle * Math.PI / 180) * elbowLength;
                                double offsetX = Math.Cos(angle * Math.PI / 180) * elbowLength;

                                elbow = new Point(body.X + (int)offsetX, body.Y + (int)offsetY);
                                image.Draw(new CircleF(new PointF(elbow.X, elbow.Y), 10), new Bgr(Color.Green), 2);
                            }
                        }

                    }
                    foreach (var rectangle in results)
                    {
                        image.Draw(rectangle, new Bgr(Color.Blue), 2);
                        image.Draw(new CircleF(new PointF(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height), 10), new Bgr(Color.Blue), 2);
                    }
                }
                return image;
            }

            private Image<Bgr, byte> Denoise(Image<Bgr, byte> image)
            {
                //float[,] k =
                //    {
                //        {0, 0, 0},
                //        {0, 0, -0},
                //        {0.33F, 0, -0}
                //    };

                //ConvolutionKernelF kernel = new ConvolutionKernelF(k);
                //return (image * kernel).Convert<Bgr, byte>();
                //return image.SmoothGaussian(3, 3, 0, 0);
                return image;
            }


            private Image<Bgr, byte> FillBlobs(Image<Bgr, byte> image, int limit = -1)
            {
                List<Contour<Point>> temp;
                var whiteImage = new Image<Bgr, byte>(image.Width, image.Height, new Bgr(Color.Black));

                var countoures = DetectBlobs(image.Convert<Gray, byte>().Not(), out temp);
                if (limit != -1)
                    countoures = countoures.OrderByDescending(item => item.Total).Take(limit);
                if (countoures != null)
                {
                    foreach (Contour<Point> points in countoures)
                    {
                        Draw(whiteImage, points, true);
                    }
                }
                return whiteImage;
            }

            private void Draw(Image<Bgr, byte> img, Contour<Point> contoures, bool fill = false)
            {
                if (fill)
                {
                    img.Draw(contoures, new Bgr(Color.White), -1);
                }
                else
                {
                    img.DrawPolyline(contoures.ToArray(), true, new Bgr(Color.Red), 2);
                }
            }

            private IEnumerable<Contour<Point>> DetectBlobs(Image<Gray, byte> image, out List<Contour<Point>> allContours)
            {
                allContours = null;

                Contour<Point> contours = image.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, RETR_TYPE.CV_RETR_LIST);
                if (contours != null)
                {
                    allContours = new List<Contour<Point>>();
                    while (contours != null)
                    {
                        allContours.Add(contours);
                        contours = contours.HNext;
                    }
                    allContours.Sort((a, b) => b.Total.CompareTo(a.Total));

                    var biggest = allContours.Take(10).ToArray();
                    return biggest;
                }

                return null;
            }

            private Image<Bgr, byte> ReduceNoise(Image<Bgr, byte> image)
            {
                image = image.Erode(1);
                image = image.Dilate(1);
                return image;
            }

            private Image<Bgr, byte> BackgroundSubtraction(Image<Bgr, byte> image)
            {
                _bg.Update(image);
                return _bg.BackgroundMask.Convert<Bgr, byte>();
            }

            public void Dispose()
            {
                if (_bg != null) _bg.Dispose();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BackgroundSubtractorMOG2Data
        {
            public int frameSizeWidth;
            public int frameSizeHeight;

            public MCvMat bgmodel;
            public MCvMat bgmodelusedModes;
            public int mframes;
            public int history;
            public int nmixtures;
            public double varThreshold;
            public float backgroundRatio;
            public float varThresholdGen;
            public float fVarInit;
            public float fVarMin;
            public float fVarMax;
            public float fCT;
            [MarshalAs(UnmanagedType.I1)]
            public bool bShadowDetection;
            [MarshalAs(UnmanagedType.U1)]
            public byte nShadowDetection;

            public float nTau;
        }
    }
}
