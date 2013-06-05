using Emgu.CV;
using Emgu.CV.Structure;

namespace HumanRemote.Server.Pipeline
{
    /// <summary>
    /// A simple image data container which provides an uncalibrated image.
    /// </summary>
    class SimpleImageData : IImageData
    {
        public IImageSource<IImageData> Source { get; private set; }
        public Image<Bgr, byte> OriginalImage { get; private set; }
        public Image<Bgr, byte> Image { get; set; }

        public SimpleImageData(IImageSource<IImageData> source, Image<Bgr, byte> image)
        { 
            Source = source;
            OriginalImage = image.Clone();
            Image = image;
        }

        public void Dispose()
        {
            if(Image != null) Image.Dispose();
            if (OriginalImage != null) OriginalImage.Dispose();
        }
    }
}
