using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using HumanRemote.Controller;
using OpenCvSharp;

namespace HumanRemote.Processor
{
    class HandDetectProcessor : IImageProcessor
    {
        public HandDetectProcessor(CameraController controller)
        {

        }

        public IplImage ProcessImage(IplImage frame)
        {
            using (IplImage skin = DetectSkin(frame))
            {
                ExtractContourAndHull(frame, skin);
            }


            return frame;
        }

    private void ExtractContourAndHull(IplImage frame, IplImage skin)
    {
        using (CvMemStorage mem = new CvMemStorage())
        {
            CvSeq<CvPoint> contours = FindContours(skin, mem);
            if (contours != null)
            {
                Cv.DrawContours(frame, contours, CvColor.Red, CvColor.Green, 0, 3, LineType.AntiAlias);

                int[] hull;
                Cv.ConvexHull2(contours, out hull, ConvexHullOrientation.Clockwise);

                // Draw Convex hull
                CvPoint pt0 = contours[hull.Last()].Value;
                foreach (int idx in hull)
                {
                    CvPoint pt = contours[idx].Value;
                    Cv.Line(frame, pt0, pt, new CvColor(255, 255, 255));
                    pt0 = pt;
                }

                //var defect = Cv.ConvexityDefects(contours, hull);
                //foreach (CvConvexityDefect item in defect)
                //{
                //    CvPoint p1 = item.Start, p2 = item.End;
                //    CvPoint2D64f mid = new CvPoint2D64f((p1.X + p2.X) / 2.0, (p1.Y + p2.Y) / 2.0);
                //    frame.DrawLine(p1, p2, CvColor.White, 3);
                //    frame.DrawCircle(item.DepthPoint, 10, CvColor.Green, -1);
                //    frame.DrawLine(mid, item.DepthPoint, CvColor.White, 1);
                //}
            }
        }
    }

        private CvSeq<CvPoint> FindContours(IplImage skin, CvMemStorage mem)
        {
            CvSeq<CvPoint> contours;
            using (IplImage clone = skin.Clone())
            {
                Cv.FindContours(clone, mem, out contours);
                if (contours == null)
                {
                    return null;
                }
            }

            CvSeq<CvPoint> max = contours;
            for (CvSeq<CvPoint> c = contours; c != null; c = c.HNext)
            {
                if (max.Total < c.Total)
                {
                    max = c;
                }
            }

            return Cv.ApproxPoly(max, CvContour.SizeOf, mem, ApproxPolyMethod.DP, 3, true);
        }

        private IplImage DetectSkin(IplImage frame)
        {
            IplImage yCrCbFrame = frame.Clone();
            Cv.CvtColor(yCrCbFrame, yCrCbFrame, ColorConversion.BgrToCrCb);

            IplImage skin = new IplImage(frame.Size, BitDepth.U8, 1);
            var YCrCb_min = new CvScalar(0, 131, 80);
            var YCrCb_max = new CvScalar(255, 185, 135);

            Cv.InRangeS(yCrCbFrame, YCrCb_min, YCrCb_max, skin);
            Cv.Erode(skin, skin, new IplConvKernel(12, 12, 6, 6, ElementShape.Rect), 1);
            Cv.Dilate(skin, skin, new IplConvKernel(6, 6, 3, 3, ElementShape.Rect), 2);

            return skin;
        }


        public void Dispose()
        {
        }
    }
}
