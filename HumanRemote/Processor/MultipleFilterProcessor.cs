using System.Collections.Generic;
using OpenCvSharp;

namespace HumanRemote.Processor
{
    public class MultipleFilterProcessor : IImageProcessor
    {
        private IEnumerable<IImageProcessor> Processors { get; set; }

        public MultipleFilterProcessor()
        {
            Processors = new List<IImageProcessor>();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
        }

        public IplImage ProcessImage(IplImage img)
        {
            foreach (var imageProcessor in Processors)
            {
                img =imageProcessor.ProcessImage(img);
            }

            return img;
        }

        public void InvokeAction()
        {
        }
    }
}