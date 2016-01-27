using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.YandexKassa.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.YandexKassa.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.YandexKassa.Fields.BusinessEmail")]
        public string BusinessEmail { get; set; }
        public bool BusinessEmail_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.YandexKassa.Fields.ShopId")]
        public string ShopId { get; set; }
        public bool ShopId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.YandexKassa.Fields.Scid")]
        public string Scid { get; set; }
        public bool Scid_OverrideForStore { get; set; }
    }
}