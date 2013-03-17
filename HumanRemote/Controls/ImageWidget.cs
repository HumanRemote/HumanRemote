using System;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace HumanRemote.Controls
{
    public class ImageWidget : System.Windows.Controls.Image
    {
        private WriteableBitmap _bitmap;
        private IplImage _image;
        public IplImage IplImage
        {
            get { return _image; }
            private set
            {
                _image = value;
            }
        }

        public virtual void RefreshImage(IplImage image)
        {
            IplImage = image;
            if (!MainWindow.GuiDispatcher.CheckAccess())
            {
                MainWindow.GuiDispatcher.BeginInvoke(new Action(UpdateImage));
            }
            else
            {
                UpdateImage();
            }
        }

        private void UpdateImage()
        {
            _bitmap = _image.ToWriteableBitmap();
            Source = _bitmap;
            UpdateLayout();
        }
    }
}
