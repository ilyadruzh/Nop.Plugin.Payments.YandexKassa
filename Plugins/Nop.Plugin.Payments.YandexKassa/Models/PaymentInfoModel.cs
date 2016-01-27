using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.YandexKassa.Models
{
    public class PaymentInfoModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Payments.YandexKassa.Fields.yandexKassaMode")]
        [AllowHtml]
        public string yandexKassaMode { get; set; }

        [NopResourceDisplayName("Plugins.Payments.YandexKassa.Fields.modePC")]
        [AllowHtml]
        public string modePayment { get; set; }

        [NopResourceDisplayName("Plugins.Payments.YandexKassa.Fields.modePC")]
        [AllowHtml]
        public string modePC { get; set; }

        [NopResourceDisplayName("Plugins.Payments.YandexKassa.Fields.modeAC")]
        [AllowHtml]
        public string modeAC { get; set; }

        [NopResourceDisplayName("Plugins.Payments.YandexKassa.Fields.modeMC")]
        [AllowHtml]
        public string modeMC { get; set; }

        [NopResourceDisplayName("Plugins.Payments.YandexKassa.Fields.modeGP")]
        [AllowHtml]
        public string modeGP { get; set; }

        [NopResourceDisplayName("Plugins.Payments.YandexKassa.Fields.modeWM")]
        [AllowHtml]
        public string modeWM { get; set; }

        [NopResourceDisplayName("Plugins.Payments.YandexKassa.Fields.modeSB")]
        [AllowHtml]
        public string modeSB { get; set; }

        [NopResourceDisplayName("Plugins.Payments.YandexKassa.Fields.modeMP")]
        [AllowHtml]
        public string modeMP { get; set; }

        [NopResourceDisplayName("Plugins.Payments.YandexKassa.Fields.modeAB")]
        [AllowHtml]
        public string modeAB { get; set; }

        [NopResourceDisplayName("Plugins.Payments.YandexKassa.Fields.modeMA")]
        [AllowHtml]
        public string modeMA { get; set; }

        [NopResourceDisplayName("Plugins.Payments.YandexKassa.Fields.modePB")]
        [AllowHtml]
        public string modePB { get; set; }

        [NopResourceDisplayName("Plugins.Payments.YandexKassa.Fields.modeQW")]
        [AllowHtml]
        public string modeQW { get; set; }

        [NopResourceDisplayName("Plugins.Payments.YandexKassa.Fields.modeKV")]
        [AllowHtml]
        public string modeKV { get; set; }
    }
}