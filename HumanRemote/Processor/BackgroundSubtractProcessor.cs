using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.GPU;
using Emgu.CV.Structure;
using Emgu.CV.VideoSurveillance;
using HumanRemote.Controller;
using System.Linq;
using Point = System.Drawing.Point;

namespace HumanRemote.Processor
{
    [StructLayout(LayoutKind.Sequential)]
    struct BackgroundSubtractorMOG2Data
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


    class BackgroundSubtractProcessor : IImageProcessor
    {
        private BackgroundSubtractorMOG2 _bg;
        private BackgroundSubtractorMOG2 _bg2;
        private CameraWindow _a = new CameraWindow();
        private CameraWindow _b = new CameraWindow();
        private CameraWindow _c = new CameraWindow();
        private CameraWindow _d = new CameraWindow();
        private CameraWindow _e = new CameraWindow();

        private Window _thresholdWindow;
        private double _threshold;
        private double _threshold2;
        public BackgroundSubtractProcessor()
        {
            _bg = new BackgroundSubtractorMOG2(10000, 2 * 2, true);
            BackgroundSubtractorMOG2Data data = (BackgroundSubtractorMOG2Data)Marshal.PtrToStructure(_bg.Ptr, typeof(BackgroundSubtractorMOG2Data));
            data.nmixtures = 3;

            _thresholdWindow = new Window();

            StackPanel pnl = new StackPanel();
            pnl.Orientation = Orientation.Vertical;
            _thresholdWindow.Content = pnl;

            Slider thresSlides = new Slider();
            thresSlides.Minimum = 0;
            thresSlides.Maximum = 100;
            thresSlides.ValueChanged += slider_ValueChanged;
            pnl.Children.Add(thresSlides);

            Slider thres2Slides = new Slider();
            thres2Slides.Minimum = 0;
            thres2Slides.Maximum = 100;
            thres2Slides.ValueChanged += thres2Slides_ValueChanged;
            pnl.Children.Add(thres2Slides);

            _a.WindowStartupLocation = WindowStartupLocation.Manual;
            _a.Top = 0;
            _a.Left = 0;
            _a.Title = "a";
            _a.Show();

            _b.WindowStartupLocation = WindowStartupLocation.Manual;
            _b.Top = 0;
            _b.Left = _a.Width;
            _b.Title = "b";
            _b.Show();

            _c.WindowStartupLocation = WindowStartupLocation.Manual;
            _c.Top = 0;
            _c.Left = _a.Width * 2;
            _c.Title = "c";
            _c.Show();

            _d.WindowStartupLocation = WindowStartupLocation.Manual;
            _d.Top = _a.Height;
            _d.Left = 0;
            _d.Title = "d";
            _d.Show();

            _e.WindowStartupLocation = WindowStartupLocation.Manual;
            _e.Top = _a.Height;
            _e.Left = _a.Width;
            _e.Title = "e";
            _e.Show();

            //_thresholdWindow.Show();
        }

        void thres2Slides_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _threshold2 = e.NewValue;
        }

        void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _threshold = e.NewValue;
        }

        public Image<Bgr, byte> ProcessImageHough(Image<Bgr, byte> img)
        {
            Image<Bgr, byte> image1 = new Image<Bgr, byte>(@"C:\Users\Manuel\SoftwareEntwicklung\C#\HumanRemote\HumanRemote\Images\input1.png");
            Image<Bgr, byte> image2 = new Image<Bgr, byte>(@"C:\Users\Manuel\SoftwareEntwicklung\C#\HumanRemote\HumanRemote\Images\input2.png");
            Image<Bgr, byte> image3 = new Image<Bgr, byte>(@"C:\Users\Manuel\SoftwareEntwicklung\C#\HumanRemote\HumanRemote\Images\input3.png");
            Image<Bgr, byte> image4 = new Image<Bgr, byte>(@"C:\Users\Manuel\SoftwareEntwicklung\C#\HumanRemote\HumanRemote\Images\input4.png");
            Image<Bgr, byte> image5 = new Image<Bgr, byte>(@"C:\Users\Manuel\SoftwareEntwicklung\C#\HumanRemote\HumanRemote\Images\input5.png");
            Image<Bgr, byte> image6 = new Image<Bgr, byte>(@"C:\Users\Manuel\SoftwareEntwicklung\C#\HumanRemote\HumanRemote\Images\input6.png");

            DrawHoughLines(image1);
            DrawHoughLines(image2);
            DrawHoughLines(image3);
            DrawHoughLines(image4);
            DrawHoughLines(image5);
            DrawHoughLines(image6);

            _a.OnFrameUpdated(image1);
            _b.OnFrameUpdated(image2);
            _c.OnFrameUpdated(image3);
            //_d.OnFrameUpdated(image4);
            _d.OnFrameUpdated(image6);
            _e.OnFrameUpdated(image5);


            return image1;
        }

        public Image<Bgr, byte> ProcessImageLinearOptimization(Image<Bgr, byte> img)
        {
            img.Draw(new CircleF(new Point(img.Width/2,10),10),new Bgr(Color.Red),-1);
            _a.OnFrameUpdated(img);


            return img;
        }




        public IEnumerable<CircleF> GetHougCircles(Image<Bgr, byte> img)
        {
            CircleF[] circles = img.HoughCircles(new Bgr(10, 10, 10), new Bgr(10, 10, 10), 10, 10, 10, 20)[0].ToArray();
            return circles;}

        public IEnumerable<LineSegment2D> GetHougLines(Image<Bgr, byte> img)
        {
            LineSegment2D[] lines = img.HoughLinesBinary(1, Math.PI / 90, 50, 20, 10)[0].ToArray();
            List<LineSegment2D> toTake = new List<LineSegment2D>();

            //foreach (LineSegment2D lineSegment2D in lines)
            //{
            //    dynamic temp = new ExpandoObject();
            //    temp.Line = lineSegment2D;
            //    //temp.Angle = CalculateAngle(lineSegment2D);
            //    lineAngles.Add(temp);
            //}

            return FilterVectoresByDistance(lines, 100).ToList();

            foreach (LineSegment2D line in lines.Where(item => item.Length > 50))
            {



                //IEnumerable<LineSegment2D> toInsert = toTake.Where(item =>
                //                                                          (Math.Abs(item.P1.X - line.P1.X) +
                //                                                           Math.Abs(item.P2.X - line.P2.X) +
                //                                                           Math.Abs(item.P1.Y - line.P1.Y) +
                //                                                           Math.Abs(item.P2.Y - line.P2.Y)) < 400);
                IEnumerable<LineSegment2D> toInsert = toTake.Where(item =>
                                                                          (Math.Abs(item.P1.X - line.P1.X) +
                                                                           Math.Abs(item.P2.X - line.P2.X) +
                                                                           Math.Abs(item.P1.Y - line.P1.Y) +
                                                                           Math.Abs(item.P2.Y - line.P2.Y)) < 400);
                foreach (LineSegment2D lineSegment2D in toInsert.ToArray())
                {
                    if (line.Length > lineSegment2D.Length)
                    {
                        toTake.Remove(lineSegment2D);
                    }
                }
                toTake.Add(line);



                //if (!(toTake.Any(item =>
                //    (Math.Abs(item.P1.X - line.P1.X) +
                //    Math.Abs(item.P2.X - line.P2.X) +
                //    Math.Abs(item.P1.Y - line.P1.Y) +
                //    Math.Abs(item.P2.Y - line.P2.Y)) < 300)
                //    //||
                //    //item.Angle - line.Angle < 40
                //))
                //{
                //    toTake.Add(line);
                //}
            }

            return toTake.Select(item => item).ToList();
        }

        public List<LineSegment2D> FilterVectoresByDistance(IEnumerable<LineSegment2D> lines, double minDifference)
        {
            List<LineSegment2D> toTake = new List<LineSegment2D>();

            foreach (LineSegment2D line in lines.Where(item => item.Length > 200))
            {
                var similarDifference = toTake.Where(point =>
                                                (
                                                    GetDistance(line.P1, point.P1) < minDifference &&
                                                    GetDistance(line.P2, point.P2) < minDifference
                                                )
                                                ||
                                                (
                                                    GetDistance(line.P2, point.P1) < minDifference &&
                                                    GetDistance(line.P1, point.P2) < minDifference
                                                )).OrderBy(item => item.Length).ToList();
                similarDifference.Add(line);
                toTake.RemoveAll(similarDifference.Contains);

                toTake.Add(similarDifference.FirstOrDefault());
            }
            return toTake;

        }

        public double GetDistance(PointF a, PointF b)
        {
            return (Math.Sqrt(Math.Pow(Math.Abs(a.X - b.X), 2) + Math.Pow(Math.Abs(a.Y - b.Y), 2)));
        }


        public void DrawHoughLines(Image<Bgr, byte> img, IEnumerable<LineSegment2D> lines)
        {
            foreach (LineSegment2D line in lines)
            {
                img.Draw(line, new Bgr(Color.Violet), 1);
                img.Draw(new CircleF(line.P1, 20), new Bgr(Color.Green), -1);
                img.Draw(new CircleF(line.P2, 20), new Bgr(Color.Red), -1);
            }
        }

        public void DrawHoughCircles(Image<Bgr, byte> img, IEnumerable<CircleF> circles)
        {
            foreach (CircleF circle in circles)
            {
                img.Draw(circle, new Bgr(Color.Violet), 5);
            }
        }


        public void DrawHoughLines(Image<Bgr, byte> img)
        {
            var toTake = GetHougLines(img);
            DrawHoughLines(img, toTake);
        }

        public void DrawHoughCircles(Image<Bgr, byte> img)
        {
            var toTake = GetHougCircles(img);
            DrawHoughCircles(img, toTake);
        }

        public double CalculateAngle(LineSegment2D line)
        {
            double deltaY = line.P2.Y - line.P1.Y;
            double deltaX = line.P2.X - line.P1.X;
            double angle;
            if (deltaX != 0)
                angle = RadianToDegree(Math.Atan2(deltaY, deltaX));
            else
                angle = 90;

            return angle;
        }

        private double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        public Image<Bgr, byte> ProcessImage(Image<Bgr, byte> img)
        {
            //return ProcessImageHough(img);

            var inputImage = img.Clone();
            _bg.Update(img);
            img = _bg.BackgroundMask.Convert<Bgr, Byte>();
            _a.OnFrameUpdated(img);
            img = img.Erode(1);
            img = img.Dilate(1);
            _b.OnFrameUpdated(img);
            //img.SmoothBlur(3, 3);
            img = FillBlobs(img);
            //DrawBlobs(img);
            _c.OnFrameUpdated(img);

            //use image as mask to display original image
            var temp = inputImage.Sub(img);
            _d.OnFrameUpdated(temp);



            //float[] BlueHist = GetHistogramData(img[0]);

            //Image<Bgr, byte> image = new Image<Bgr, byte>(img.Width, img.Height);

            //for (int i = 0; i < BlueHist.Length; i++)
            //{
            //    image.Draw(new LineSegment2D(new Point(i, (int)BlueHist[i]), new Point(i, 0)), new Bgr(Color.Red), 1);
            //}

            //_e.OnFrameUpdated(image);


            //only display skin
            img = img.Not();
            //img = DetectSkin(img);
            //img = img.Erode(2);
            //img = img.Dilate(2);
            //img = img.Not();

            //DrawHoughLines(img);
            _e.OnFrameUpdated(img);
            //img.MorphologyEx()

            //List<Contour<Point>> allContours;
            //var contours = DetectBlobs(img.Convert<Gray, byte>(), out allContours);


            //Image<Bgr, byte> image = new Image<Bgr, byte>(img.Width, img.Height, new Bgr(Color.White));
            //if (allContours != null)
            //{

            //    foreach (Contour<Point> contour in allContours.Take(3))
            //    {
            //        var convexityDefact = contour.GetConvexityDefacts(new MemStorage(), Emgu.CV.CvEnum.ORIENTATION.CV_CLOCKWISE);

            //        foreach (MCvConvexityDefect mCvConvexityDefect in convexityDefact)
            //        {
            //            PointF startPoint = new PointF(mCvConvexityDefect.StartPoint.X, mCvConvexityDefect.StartPoint.Y);
            //            CircleF startCircle = new CircleF(startPoint, 5f);
            //            image.Draw(startCircle, new Bgr(Color.Red), 5);
            //        }
            //        Draw(image, contour, false);
            //        //Draw(image, contour, true);
            //    }
            //}

            //_a.OnFrameUpdated(image);

            return img;
        }

        private float[] GetHistogramData(Image<Gray,byte> imgGray)
        {
            float[] histoGrammData;

            DenseHistogram histoGramm = new DenseHistogram(255, new RangeF(0, 255));
            histoGramm.Calculate(new Image<Gray, Byte>[] { imgGray }, true, null);
            //The data is here
            //Histo.MatND.ManagedArray
            histoGrammData = new float[256];
            histoGramm.MatND.ManagedArray.CopyTo(histoGrammData, 0);

            return histoGrammData;
        }

        private Image<Bgr, byte> DetectSkin(Image<Bgr, byte> img)
        {
            var yCrCbFrame = img.Clone();
            //Cv.CvtColor(yCrCbFrame, yCrCbFrame, ColorConversion.BgrToCrCb);

            var YCrCb_min = new Bgr(0, 10, 60);
            var YCrCb_max = new Bgr(255, 255, 255);

            return yCrCbFrame.InRange(YCrCb_min, YCrCb_max).Convert<Bgr, byte>();
        }

        public Image<Bgr, byte> DetectArm(Image<Bgr, byte> img)
        {
            var image = new Image<Bgr, byte>("C:/Users/Manuel/SoftwareEntwicklung/C#/HumanRemote/HumanRemote/Images/armtemplate.png");
            return img.MatchTemplate(image, TM_TYPE.CV_TM_CCORR_NORMED).Convert<Bgr, byte>();
        }


        public void DrawCountures(Image<Bgr, Byte> img)
        {
            var converted = img.Convert<Gray, Byte>();
            var contoures = converted.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_TC89_L1, RETR_TYPE.CV_RETR_LIST);
            if (contoures != null)
            {
                Draw(img, contoures);
            }
        }


        //public void Draw(Image<Bgr, byte> img, Seq<Point> points, bool fill = false)
        //{
        //    if (fill)
        //    {
        //        //img.FillConvexPoly(contoures.ToArray(), new Bgr(Color.Black));
        //        img.Draw(points.ApproxPoly(20), new Bgr(Color.Black), -1);
        //        //img.Draw(contoures.ApproxPoly(50), new Bgr(Color.Green), -1);
        //        img.Draw(points.ApproxPoly(20), new Bgr(Color.Red), 2);
        //    }
        //    else
        //    {
        //        img.DrawPolyline(points.ToArray(), true, new Bgr(Color.Red), 2);
        //    }
        //}

        public void Draw(Image<Bgr, byte> img, Contour<Point> contoures, bool fill = false)
        {
            if (fill)
            {
                //img.FillConvexPoly(contoures.ApproxPoly(5).ToArray(), new Bgr(Color.Black));
                img.Draw(contoures, new Bgr(Color.Black), -1);
                //img.Draw(contoures.ApproxPoly(50), new Bgr(Color.Green), -1);
            }
            else
            {
                img.DrawPolyline(contoures.ToArray(), true, new Bgr(Color.Red), 2);
            }
        }

        private Image<Bgr, byte> Canny(Image<Bgr, byte> newImg)
        {

            return newImg;
            Image<Ycc, Byte> ycc = newImg.Convert<Ycc, byte>();

            var skin = ycc.InRange(new Ycc(0, 131, 80), new Ycc(255, 185, 135));
            skin.Erode(2);
            skin.Dilate(2);

            var contours = skin.FindContours();
            if (contours != null)
            {
                List<Seq<Point>> allContours = new List<Seq<Point>>();
                for (Seq<Point> c = contours; c != null; c = c.HNext)
                {
                    allContours.Add(c);
                }
                allContours.Sort((a, b) => b.Total - a.Total);


                var biggest = allContours.Take(2);

                foreach (Seq<Point> points in biggest)
                {
                    var hull = points.GetConvexHull(ORIENTATION.CV_CLOCKWISE);

                    newImg.Draw(hull, new Bgr(Color.Red), 2);
                }
            }

            return newImg;
        }

        private Image<Bgr, Byte> Canny(Image<Gray, Byte> img)
        {
            var converted = img.Convert<Bgr, Byte>();
            converted = converted.Erode(4);
            converted = converted.Dilate(4);
            return converted;
        }

        public void DrawBlobs(Image<Bgr, byte> img, int i = -1)
        {
            List<Contour<Point>> temp;

            var countoures = DetectBlobs(img.Convert<Gray, byte>(), out temp);

            if (i != -1)
                countoures = countoures.OrderByDescending(item => item.Total).Take(i);
            if (countoures != null)
            {
                foreach (Contour<Point> points in countoures)
                {
                    Draw(img, points);
                }
            }
        }

        public Image<Bgr, byte> FillBlobs(Image<Bgr, byte> img, int i = -1)
        {
            List<Contour<Point>> temp;
            var whiteImage = new Image<Bgr, byte>(img.Width, img.Height, new Bgr(Color.White));

            var countoures = DetectBlobs(img.Convert<Gray, byte>().Not(), out temp);
            if (i != -1)
                countoures = countoures.OrderByDescending(item => item.Total).Take(i);
            if (countoures != null)
            {
                foreach (Contour<Point> points in countoures)
                {
                    Draw(whiteImage, points, true);
                }
            }
            return whiteImage;
        }

        public Seq<Point> GetConvexHull(List<Contour<Point>> contourPoints)
        {
            Seq<Point> hullPoints = new Seq<Point>(new MemStorage());
            foreach (Seq<Point> countoure in contourPoints)
            {
                hullPoints.PushMulti(countoure.ToArray(), BACK_OR_FRONT.FRONT);
            }

            hullPoints = hullPoints.GetConvexHull(ORIENTATION.CV_COUNTER_CLOCKWISE);
            return hullPoints;
        }

        private IEnumerable<Contour<Point>> DetectBlobs(Image<Gray, Byte> img, out List<Contour<Point>> allContours)
        {
            allContours = null;
            Contour<Point> contours = img.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, RETR_TYPE.CV_RETR_LIST);

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

        //public Image<Bgr, byte> ProcessImage(Image<Bgr, byte> img)
        //       {
        //           img = img.Erode(1);
        //           img = img.Dilate(1);

        //           _bg.Update(img);

        //           var fg = _bg.ForegroundMask;
        //           var bg = _bg.BackgroundMask;

        //           fg = fg.Erode(1);
        //           fg = fg.Dilate(1);

        //           //img = fg.Convert<Bgr, byte>();
        //           Contour<Point> contours = fg.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_NONE, RETR_TYPE.CV_RETR_EXTERNAL);

        //           var blueImage = img.Copy();
        //           if (contours != null)
        //           {
        //               List<Contour<Point>> allContours = new List<Contour<Point>>();
        //               while(contours != null)
        //               {
        //                   allContours.Add(contours);
        //                   contours = contours.HNext;
        //               }

        //               allContours.Sort((a, b) => b.Total.CompareTo(a.Total));

        //               var toDraw = allContours.Take(2);
        //               foreach (Contour<Point> contour in toDraw)
        //               {
        //                   blueImage.Draw(contour, new Bgr(Color.Blue), -1);
        //               }
        //           }

        //           return img;
        //       }


        public void InvokeAction()
        {
        }

        public void Dispose()
        {
            _thresholdWindow.Close();
        }
    }
}
