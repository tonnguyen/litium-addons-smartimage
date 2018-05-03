using Litium.Application.Media;
using Litium.Blobs;
using Litium.Foundation.Modules.ExtensionMethods;
using Litium.Studio.Plugins.ImageResizer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Litium.AddOns.SmartImage
{
    public abstract class BaseSmartImageAnalyzer : ISmartImageAnalyzer
    {
        protected readonly BlobService _blobService;
        protected readonly IImageResizer _imageResizer;
        protected readonly MimeExtensionHelper _mimeExtensionHelper;

        protected BaseSmartImageAnalyzer(BlobService blobService, IImageResizer imageResizer, MimeExtensionHelper mimeExtensionHelper)
        {
            _blobService = blobService;
            _imageResizer = imageResizer;
            _mimeExtensionHelper = mimeExtensionHelper;
        }

        public abstract IEnumerable<AnalysisResponse> Process(ConcurrentQueue<ImageQueue> queues);

        /// <summary>
        /// Opens the image stream to read. If the <paramref name="maxSize"/> is not null, it will
        /// also resize the image if it is larger than the max Size.
        /// </summary>
        /// <param name="image">The image to read.</param>
        /// <param name="maxSize">The optional maximum image's dimension.</param>
        /// <returns></returns>
        protected virtual Stream OpenRead(ImageQueue image, Size? maxSize = null)
        {
            var blobContainer = _blobService.Get(image.BlobUri);
            var blobStream = blobContainer.GetDefault().OpenRead();
            if (!maxSize.HasValue)
            {
                return blobStream;
            }
            return EnsureMaxImageSize(blobStream, maxSize.Value, image);
        }

        protected virtual Stream EnsureMaxImageSize(Stream blobStream, Size maxSize, ImageQueue image)
        {
            using (var bitmap = Image.FromStream(blobStream))
            {
                // if the image's dimension if larger than max size, we resize it to:
                // save the bandwidth
                // and also to prevent any quota/litmitation error might have
                if (bitmap.Width > maxSize.Width || bitmap.Height > maxSize.Height)
                {
                    double heightWidthRatio = ((double)bitmap.Height) / bitmap.Width;
                    var newSize = new Size(maxSize.Width, Convert.ToInt32(maxSize.Width * heightWidthRatio));
                    if (newSize.Height > maxSize.Height)
                    {
                        newSize = new Size(Convert.ToInt32(maxSize.Height / heightWidthRatio), maxSize.Height);
                    }
                    var imageFormat = FileExtensions.GetImageFormat(_mimeExtensionHelper.FindMime(Path.GetExtension(image.FileName)), true);
                    return _imageResizer.Resize(newSize, true, blobStream, imageFormat).Result;
                }
                return blobStream;
            }
        }
    }
}
