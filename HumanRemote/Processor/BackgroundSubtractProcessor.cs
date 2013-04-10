using System;
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
        private Window _thresholdWindow;
        private double _threshold;
        private double _threshold2;

        public BackgroundSubtractProcessor()
        {
            _bg = new BackgroundSubtractorMOG2(500, 2 * 2, false);
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



            _thresholdWindow.Show();
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
            List<Contour<Point>> allContours;
            var rois = DetectBlobs(img, out allContours);

            if (rois == null) return img;
            if (allContours == null) return img;
            Point min = new Point(int.MaxValue, int.MaxValue);
            Point max = new Point(int.MinValue, int.MinValue);

            foreach (Seq<Point> points in rois)
            {
                var bounding = points.BoundingRectangle;
                if (bounding.Left < min.X)
                {
                    min.X = bounding.Left;
                }
                if (bounding.Top < min.Y)
                {
                    min.Y = bounding.Top;
                }
                if (bounding.Bottom > max.Y)
                {
                    max.Y = bounding.Bottom;
                }
                if (bounding.Right > max.X)
                {
                    max.X = bounding.Right;
                }
            }

            Image<Bgr, byte> newImg = new Image<Bgr, byte>(img.Width, img.Height, new Bgr(Color.Black));
            //foreach (Contour<Point> contour in allContours)
            //{
            //    if((contour.BoundingRectangle.Width * contour.BoundingRectangle.Height) >= 50)
            //    {
            //        newImg.Draw(contour, new Bgr(Color.White), -1);
            //    }
            //}
            foreach (var biggest in allContours.Take(2))
            {
                img.Draw(biggest.ApproxPoly(3).GetConvexHull(ORIENTATION.CV_CLOCKWISE), new Bgr(Color.Lime), 2);
            }


            return img;
            //Rectangle boundingAll = Rectangle.FromLTRB(min.X, min.Y, max.X, max.Y);
            //var roi = _bg.ForegroundMask.Copy(boundingAll);
            //var canny = Canny(roi);

            //canny.CopyTo(newImg.GetSubRect(boundingAll));
            //return newImg;
        }

        private Image<Bgr, byte> SkinDetector(Image<Bgr, byte> newImg)
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
            img = img.Erode(1);
            img = img.Dilate(1);

            //var contours = img.Canny(_threshold, _threshold2);
            //return contours.Convert<Bgr, Byte>();
            //return img.Convert<Bgr, Byte>();
            //var contours = img.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, RETR_TYPE.CV_RETR_EXTERNAL);
            //while (contours != null)
            //{
            //    img.Draw(contours, new Gray(255), 1);
            //}
            return img.Convert<Bgr,Byte>();
        }

        private IEnumerable<Seq<Point>> DetectBlobs(Image<Bgr, Byte> img, out List<Contour<Point>> allContours )
        {
            img = img.Erode(1);
            img = img.Dilate(1);
            _bg.Update(img);

            var fg = _bg.ForegroundMask;

            fg = fg.Erode(2);
            fg = fg.Dilate(1);

            allContours = null;
            Contour<Point> contours = fg.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_NONE, RETR_TYPE.CV_RETR_EXTERNAL);

            if (contours != null)
            {
                allContours = new List<Contour<Point>>();
                while (contours != null)
                {
                    allContours.Add(contours);
                    contours = contours.HNext;
                }

                allContours.Sort((a, b) => b.Total.CompareTo(a.Total));

                var biggest = allContours.Take(2);
                return biggest.Select(c => c.GetConvexHull(ORIENTATION.CV_CLOCKWISE));
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
