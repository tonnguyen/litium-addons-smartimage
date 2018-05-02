using System;
using System.Collections.Generic;

namespace Litium.AddOns.SmartImage
{
    public class AnalysisResponse
    {
        public Guid SystemId { get; set; }
        public IEnumerable<string> Tags { get; set; }
    }
}
