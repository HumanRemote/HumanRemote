using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;

namespace HumanRemote.Controls
{
    public class ImageWidget : System.Windows.Controls.Image
    {
        private BitmapSource _bitmap;
        private Image<Bgr, Byte> _image;
        public Image<Bgr, Byte> IplImage
        {
            get { return _image; }
            private set
            {
                _image = value;
            }
        }

        public virtual void RefreshImage(Image<Bgr, Byte> image)
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
            _bitmap = ToBitmapSource(_image);
            Source = _bitmap;
            UpdateLayout();
        }

        /// <summary>
        /// Delete a GDI object
        /// </summary>
        /// <param name="o">The poniter to the GDI object to be deleted</param>
        /// <returns></returns>
        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        /// <summary>
        /// Convert an IImage to a WPF BitmapSource. The result can be used in the Set Property of Image.Source
        /// </summary>
        /// <param name="image">The Emgu CV Image</param>
        /// <returns>The equivalent BitmapSource</returns>
        public static BitmapSource ToBitmapSource(IImage image)
        {
            using (Bitmap source = image.Bitmap)
            {
                IntPtr ptr = source.GetHbitmap(); //obtain the Hbitmap

                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(ptr); //release the HBitmap
                return bs;
            }
        }
    }
}
