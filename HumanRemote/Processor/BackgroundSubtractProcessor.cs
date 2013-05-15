using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using Emgu.CV;
using Emgu.CV.CvEnum;
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
            _bg = new BackgroundSubtractorMOG2(10000, 2 * 2, false);
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

        public Image<Bgr, byte> ProcessImage(Image<Bgr, byte> img)
        {
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

            img = inputImage.Sub(img);
            _d.OnFrameUpdated(img);

            img = DetectSkin(img);
            img = img.Erode(2);
            img = img.Dilate(2);
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
