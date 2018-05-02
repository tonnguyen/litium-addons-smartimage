using Litium.Runtime.DependencyInjection;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Litium.AddOns.SmartImage
{
    [Service(ServiceType = typeof(ISmartImageAnalyzer))]
    public interface ISmartImageAnalyzer
    {
        IEnumerable<AnalysisResponse> Process(ConcurrentQueue<ImageQueue> queues);
    }
}
