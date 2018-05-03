using Litium.Administration.Media;
using Litium.Blobs;
using Litium.Data;
using Litium.Media;
using Litium.Runtime.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Timers;

namespace Litium.AddOns.SmartImage
{
    /// <summary>
    /// Inherits the FileMetadataExtractorServiceImpl to listen for File's metadata changed event to
    /// queue up image files that need to analysis.
    /// </summary>
    [Service(ServiceType = typeof(FileMetadataExtractorService))]
    public class SmartImageService : FileMetadataExtractorServiceImpl
    {
        private readonly ISmartImageAnalyzer _smartImageAnalyzer;
        private ConcurrentQueue<ImageQueue> _queue = new ConcurrentQueue<ImageQueue>();
        private readonly FileService _fileService;
        private readonly DataService _dataService;
        private readonly ILogger _logger;

        /// <summary>
        /// The timer to execute the Image Analysis process. By default it kicks in 10 seconds after an image is
        /// uploaded. If any image is uploaded during this time, the timer will be reset. This to make sure we
        /// don't submit many requests to the Image Analysis provider. Instead, we submit them in batch request.
        /// </summary>
        private static Timer _timer;
        private static object _lock = new object();
        private static bool _analyzing = false;

        public static string SettingKey = "__SmartImageStorate";
        public static string FieldName = "AddOnSmartImageTags";

        public SmartImageService(FieldDefinitionService fieldDefinitionService, 
                                BlobService blobService, 
                                FileService fileService,
                                DataService dataService,
                                ILogger<SmartImageService> logger,
                                ISmartImageAnalyzer smartImageAnalyzer) : base(fieldDefinitionService, blobService)
        {
            _fileService = fileService;
            _dataService = dataService;
            _logger = logger;
            _smartImageAnalyzer = smartImageAnalyzer;
        }

        public override void UpdateMetadata(FileFieldTemplate template, Media.File fileObject, Stream stream, Uri blobUri, bool forceClear = false)
        {
            base.UpdateMetadata(template, fileObject, stream, blobUri, forceClear);

            if (template.TemplateType != FileTemplateType.Image)
            {
                return;
            }
            
            _queue.Enqueue(new ImageQueue()
            {
                SystemId = fileObject.SystemId,
                BlobUri = blobUri,
                FileName = fileObject.Name,
            });
            if (_analyzing)
            {
                return;
            }

            if (_timer == null)
            {
                lock(_lock)
                {
                    if (_timer == null)
                    {
                        _timer = new Timer(10000);
                        _timer.Elapsed += _timer_Elapsed;
                        _timer.AutoReset = false;
                    }
                }
            }
            if (_timer.Enabled)
            {
                _timer.Stop();
            }
            if (!_timer.Enabled)
            {
                lock(_lock)
                {
                    if (!_timer.Enabled)
                    {
                        _timer.Start();
                    }
                }
            }
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _analyzing = true;
            try
            {
                Persist(_smartImageAnalyzer.Process(_queue));
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error when extracting smart data from image: {ex.Message}");
            }
            finally
            {
                _analyzing = false;
            }
        }

        private void Persist(IEnumerable<AnalysisResponse> responses)
        {
            using (var db = _dataService.CreateBatch())
            {
                foreach (var res in responses)
                {
                    var file = _fileService.Get(res.SystemId)?.MakeWritableClone();
                    if (file == null)
                    {
                        continue;
                    }
                    file.Fields.AddOrUpdateValue(FieldName, string.Join(", ", res.Tags));
                    db.Update(file);
                }
                db.Commit();
            }
        }
    }
}
