using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.YandexKassa.Controllers;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Tax;
using Nop.Services.Customers;
using System.Globalization;
using Nop.Core.Domain.Payments;
using Nop.Web.Framework;
using Nop.Services.Localization;

namespace Nop.Plugin.Payments.YandexKassa
{
    public class YandexKassaPaymentProcessor : BasePlugin, IPaymentMethod
    {

        #region Fields

        private readonly YandexKassaPaymentSettings _yandexKassaSettings;
        private readonly ISettingService _settingService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly ICustomerService _customerService;
        private readonly IWebHelper _webHelper;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ITaxService _taxService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly HttpContextBase _httpContext;
        public PaymentMethodType PaymentMethodType {get{return PaymentMethodType.Redirection;}}
        public RecurringPaymentType RecurringPaymentType { get { return RecurringPaymentType.NotSupported; } }

        #endregion

        #region Ctor

        public YandexKassaPaymentProcessor(YandexKassaPaymentSettings yandexKassaPaymentSettings,
            ISettingService settingService, ICurrencyService currencyService,
            CurrencySettings currencySettings, ICustomerService customerService, IWebHelper webHelper,
            ICheckoutAttributeParser checkoutAttributeParser, ITaxService taxService, 
            IOrderTotalCalculationService orderTotalCalculationService, HttpContextBase httpContext)
        {
            this._yandexKassaSettings = yandexKassaPaymentSettings;
            this._settingService = settingService;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._customerService = customerService;
            this._webHelper = webHelper;
            this._checkoutAttributeParser = checkoutAttributeParser;
            this._taxService = taxService;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._httpContext = httpContext;
        }

        #endregion

        #region Methods

        public bool SkipPaymentInfo {get{return false;}}
        public bool SupportCapture {get{return false;}}
        public bool SupportPartiallyRefund {get{return false;}}
        public bool SupportRefund {get{return false;}}
        public bool SupportVoid {get{return false;}}
        public CancelRecurringPaymentResult CancelRecurringPayment (CancelRecurringPaymentRequest cancelPaymentRequest){return new CancelRecurringPaymentResult ();}
        public bool CanRePostProcessPayment (Order order) {return false;}
        public CapturePaymentResult Capture (CapturePaymentRequest capturePaymentRequest) {return new CapturePaymentResult();}
        public decimal GetAdditionalHandlingFee (IList<ShoppingCartItem> cart) {var result = 0; return result;}
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart) { return false; }
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest) { return new ProcessPaymentResult(); }
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest) { return new RefundPaymentResult(); }
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest) { return new VoidPaymentResult(); }
        public Type GetControllerType () { return typeof(PaymentYandexKassaController); }

        // Routes
        public void GetPaymentInfoRoute (out string actionName, out string controllerName, out RouteValueDictionary routeValues) 
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentYandexKassa";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.YandexKassa.Controllers" }, { "area", null } };
        }
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentYandexKassa";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.YandexKassa.Controllers" }, { "area", null } };
        }

        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var customer = _customerService.GetCustomerById(postProcessPaymentRequest.Order.CustomerId);
            var post = new RemotePost();

            var processPaymentRequest = _httpContext.Session["OrderPaymentInfo"] as ProcessPaymentRequest;


            post.FormName = "YandexKassaPaymentForm";
            post.Url = GetYandexKassaUrl();
            post.Method = "POST";

            post.Add("shopId", _yandexKassaSettings.ShopId.ToString());
            post.Add("scid", _yandexKassaSettings.Scid.ToString());
            post.Add("sum", String.Format(CultureInfo.InvariantCulture, "{0:0.00}", postProcessPaymentRequest.Order.OrderTotal));
            post.Add("customerNumber", postProcessPaymentRequest.Order.BillingAddress.Email);
            post.Add("orderNumber", postProcessPaymentRequest.Order.Id.ToString());
            post.Add("shopSuccessURL", String.Format("{0}Plugins/PaymentYandexKassa/Return", _webHelper.GetStoreLocation(false)));
            post.Add("shopFailURL", String.Format("{0}Plugins/PaymentYandexKassa/Return", _webHelper.GetStoreLocation(false)) );
            post.Add("cps_email", customer.Email);
            post.Add("cps_phone", customer.BillingAddress.PhoneNumber);
            post.Add("paymentType", postProcessPaymentRequest.Order.SubscriptionTransactionId);
            post.Add("custName", postProcessPaymentRequest.Order.BillingAddress.LastName + postProcessPaymentRequest.Order.BillingAddress.FirstName);
            post.Add("custAddr", customer.BillingAddress.Address1);
            post.Add("custEMail", postProcessPaymentRequest.Order.BillingAddress.Email);
            post.Add("orderDetails", "without");

            post.Post();

        }

        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AllowStoringCreditCardNumber = true;
            result.SubscriptionTransactionId = processPaymentRequest.CustomValues["yandexKassaMode"].ToString();
            result.NewPaymentStatus = PaymentStatus.Pending;
            return result;
        }

        #endregion

        #region Install & Uninstall

        public override void Install()
        {
            var settings = new YandexKassaPaymentSettings()
            {
                UseSandbox = false,
                BusinessEmail = "",
                ShopId = 0,
                Scid = 0,
                YandexKassaMode = "",
            };
            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.UseSandbox", "Использовать для тестетирования");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.BusinessEmail", "Email");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.BusinessEmail.Hint", "Ваш email для уведомлений.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.ShopId", "Shop ID");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.ShopId.Hint", "Введите свой shop ID.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.Scid", "Scid витрины магазина");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.Scid.Hint", "Для реальных платежей");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.AboutYandexKassa", " Яндекс.Касса - это инструмент для приема платежей. Все популярные способы оплаты");

            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.PC", "Оплата из кошелька в Яндекс.Деньгах");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.AC", "Оплата с произвольной банковской карты");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.MC", "Платеж со счета мобильного телефона");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.GP", "Оплата наличными через кассы и терминалы");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.WM", "Оплата из кошелька в системе WebMoney");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.SB", "Оплата через Сбербанк: оплата по SMS или Сбербанк Онлайн");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.MP", "Оплата через мобильный терминал (mPOS)");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.AB", "Оплата через Альфа-Клик");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.MA", "Оплата через MasterPass");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.PB", "Оплата через Промсвязьбанк");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.QW", "Оплата через QIWI Wallet");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.KV", "Оплата через КупиВкредит (Тинькофф Банк)");



            base.Install();
        }

        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<YandexKassaPaymentSettings>();
            //locales
            this.DeletePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.UseSandbox");
            this.DeletePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.BusinessEmail");
            this.DeletePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.BusinessEmail.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.ShopId");
            this.DeletePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.ShopId.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.Scid");
            this.DeletePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.Scid.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.AboutYandexKassa");

            this.DeletePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.PC");
            this.DeletePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.AC");
            this.DeletePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.MC");
            this.DeletePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.GP");
            this.DeletePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.WM");
            this.DeletePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.SB");
            this.DeletePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.MP");
            this.DeletePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.AB");
            this.DeletePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.MA");
            this.DeletePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.PB");
            this.DeletePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.QW");
            this.DeletePluginLocaleResource("Plugins.Payments.YandexKassa.Fields.KV");

            base.Uninstall();
        }

        #endregion

        #region Utilities

        public string GetYandexKassaUrl()
        {
            return _yandexKassaSettings.UseSandbox ?
                "https://demomoney.yandex.ru/eshop.xml" :
                "https://money.yandex.ru/eshop.xml";
        }


        //public static HttpWebResponse PostToYandexKassa(string postedData, string postUrl)
        //{
        //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(postUrl);
        //    request.Method = "POST";
        //    request.Credentials = CredentialCache.DefaultCredentials;

        //    UTF8Encoding encoding = new UTF8Encoding();
        //    var bytes = encoding.GetBytes(postedData);

        //    request.ContentType = "application/x-www-form-urlencoded";
        //    request.ContentLength = bytes.Length;

        //    using (var newStream = request.GetRequestStream())
        //    {
        //        newStream.Write(bytes, 0, bytes.Length);
        //        newStream.Close();
        //    }
        //    return (HttpWebResponse)request.GetResponse();
        //}

        #endregion
    }
}
