using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.YandexKassa
{
    public class YandexKassaPaymentSettings : ISettings
    {
        // Использовать плагин в тестовом режиме
        public bool UseSandbox { get; set; }

        // Email
        public string BusinessEmail { get; set; }
        public int ShopId { get; set; }
        public int Scid { get; set; }
        public string YandexKassaMode { get; set; }

        public string modePC { get; set; }
        public string modeAC { get; set; }
        public string modeMC { get; set; }
        public string modeGP { get; set; }
        public string modeWM { get; set; }
        public string modeSB { get; set; }
        public string modeMP { get; set; }
        public string modeAB { get; set; }
        public string modeMA { get; set; }
        public string modePB { get; set; }
        public string modeQW { get; set; }
        public string modeKV { get; set; }
    }
}
