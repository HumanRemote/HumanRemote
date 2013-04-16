using System;
using System.Collections.Generic;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using HumanRemote.Controller;
using System.Linq;

namespace HumanRemote.Processor
{
    class BodyExtractorProcessor : IImageProcessor
    {
        private Image<Bgr, byte> _currentColored;

        private Image<Gray, byte> _currentGrayScale;
        private Image<Gray, byte> _currentGrayScaleDilated;
        private Image<Gray, byte> _background;

        private int _counter;
        private int _pixelsChanged;
        private bool _calculateMotionLevel;

        public BodyExtractorProcessor(CameraController controller)
        {

        }

        public Image<Bgr, byte> ProcessImage(Image<Bgr, byte> img)
        {
            var h = img.Height;
            var w = img.Width;

            _currentColored = img.Clone();
            if (_background == null)
            {
                _background = new Image<Gray, byte>(_currentColored.Width, _currentColored.Height);
                _currentGrayScale = new Image<Gray, byte>(_currentColored.Width, _currentColored.Height);
                _currentGrayScaleDilated = new Image<Gray, byte>(_currentColored.Width, _currentColored.Height);
                PreprocessInputImage(_currentColored, _background);

                return img;
            }

            //
            // Preprocess
            PreprocessInputImage(_currentColored, _currentGrayScale);

            // 
            // Move Background towards current frame
            if (++_counter == 1)
            {
                _counter = 0;
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        var t = _currentGrayScale.Data[y, x, 0] - _background.Data[y, x, 0];
                        if (t > 0)
                        {
                            _background.Data[y, x, 0] += 2;
                        }
                        else if (t < 0)
                        {
                            _background.Data[y, x, 0] -= 2;
                        }
                    }
                }
            }


            // Difference and Thresholding
            _pixelsChanged = 0;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var t = _currentGrayScale.Data[y, x, 0] - _background.Data[y, x, 0];
                    if (t < 0)
                    {
                        t = -t;
                    }
                    if (t >= 15)
                    {
                        _pixelsChanged++;
                        _currentGrayScale.Data[y, x, 0] = 255;
                    }
                    else
                    {
                        _currentGrayScale.Data[y, x, 0] = 0;
                    }
                }
            }

            if (_calculateMotionLevel)
            {
                _pixelsChanged *= 64;
            }
            else
            {
                _pixelsChanged = 0;
            }

            // 
            // Dilation analogue for borders extending
            _currentGrayScaleDilated = _currentGrayScale.Dilate(1);

            Contour<Point> contours = _currentGrayScaleDilated.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE,
                                                                 RETR_TYPE.CV_RETR_LIST);

            List<Contour<Point>> allContours = new List<Contour<Point>>();
            while (contours != null)
            {
                allContours.Add(contours);
                contours = contours.HNext;
            }
            allContours.Sort((a, b) => b.Total - a.Total);

            var biggest = allContours.Take(2).Select(c => c.ApproxPoly(3));
            var result = _currentColored;
            foreach (Contour<Point> contour in biggest)
            {
                result.Draw(contour, new Bgr(Color.Red), 3);
            }
            //PostProcessInputImage(img, _currentGrayScaleDilated);

            return result;
        }


        public void InvokeAction()
        {
        }

        private void PreprocessInputImage(Image<Bgr, byte> data, Image<Gray, byte> buf)
        {
            // To special grayscale
            for (int y = 0; y < data.Height; y++)
            {
                for (int x = 0; x < data.Width; x++)
                {
                    buf.Data[y, x, 0] =
                        (byte)(0.2125f * data.Data[y, x, 2] + 0.7154f * data.Data[y, x, 1] + 0.0721f * data.Data[y, x, 0]);
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
