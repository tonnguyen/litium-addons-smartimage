using Litium.FieldFramework;
using Litium.Foundation;
using Litium.Media;
using Litium.Owin.Lifecycle;

namespace Litium.AddOns.SmartImage
{
    /// <summary>
    /// Create the field definition to store image tags on startup.
    /// </summary>
    internal class ImageFieldDefinitionSetup : IStartupTask
    {
        private readonly FieldDefinitionService _fieldDefinitionService;

        public ImageFieldDefinitionSetup(FieldDefinitionService fieldDefinitionService)
        {
            _fieldDefinitionService = fieldDefinitionService;
        }

        public void Start()
        {
            using (Solution.Instance.SystemToken.Use())
            {
                InitFields();
            }
        }

        private void InitFields()
        {
            var items = new[]
            {
                new FieldDefinition(SmartImageService.FieldName, SystemFieldTypeConstants.Text)
                {
                    CanBeGridColumn = true,
                    CanBeGridFilter = true,
                    Localizations =
                    {
                        ["sv-SE"] = {Name = "Smart Image Tags"},
                        ["en-US"] = {Name = "Smart Image Tags"},
                    }
                },
            };

            foreach (var item in items)
            {
                var currentField = _fieldDefinitionService.Get(item.Id);
                if (currentField != null)
                {
                    continue;
                }
                _fieldDefinitionService.Create(item);
            }
        }
    }
}
