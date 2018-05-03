using System;

namespace Litium.AddOns.SmartImage
{
    public class ImageQueue
    {
        public Guid SystemId { get; set; }
        public Uri BlobUri { get; set; }
        public string FileName { get; set; }
    }
}
