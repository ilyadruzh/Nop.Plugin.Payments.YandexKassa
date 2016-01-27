using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Services.Payments;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using Nop.Plugin.Payments.YandexKassa.Models;
using Nop.Core.Domain.Payments;
using Nop.Services.Orders;

namespace Nop.Plugin.Payments.YandexKassa.Controllers
{
    public class PaymentYandexKassaController : BasePaymentController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly PaymentSettings _paymentSettings;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IWebHelper _webHelper;
        private readonly YandexKassaPaymentSettings _yandexKassaSettings;

        public PaymentYandexKassaController(IWorkContext workContext,
            PaymentSettings paymentSettings, IPaymentService paymentService,
            IStoreService storeService,
            ISettingService settingService,
            ILocalizationService localizationService, 
            IOrderService orderService, 
            IOrderProcessingService orderProcessingService,
            IWebHelper webHelper,
            YandexKassaPaymentSettings yandexKassaSettings)
        {
            this._workContext = workContext;
            this._paymentSettings = paymentSettings;
            this._paymentService = paymentService;
            this._storeService = storeService;
            this._settingService = settingService;
            this._localizationService = localizationService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._webHelper = webHelper;
            this._yandexKassaSettings = yandexKassaSettings;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var yandexKassaPaymentSettings = _settingService.LoadSetting<YandexKassaPaymentSettings>(storeScope);

            var model = new ConfigurationModel();
            model.UseSandbox = Convert.ToBoolean(yandexKassaPaymentSettings.UseSandbox);
            model.BusinessEmail = yandexKassaPaymentSettings.BusinessEmail;
            model.ShopId = Convert.ToString(yandexKassaPaymentSettings.ShopId);
            model.Scid = Convert.ToString(yandexKassaPaymentSettings.Scid);

            model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {
                model.UseSandbox_OverrideForStore = _settingService.SettingExists(yandexKassaPaymentSettings, x => x.UseSandbox, storeScope);
                model.BusinessEmail_OverrideForStore = _settingService.SettingExists(yandexKassaPaymentSettings, x => x.BusinessEmail, storeScope);
                model.ShopId_OverrideForStore = _settingService.SettingExists(yandexKassaPaymentSettings, x => x.ShopId, storeScope);
                model.Scid_OverrideForStore = _settingService.SettingExists(yandexKassaPaymentSettings, x => x.Scid, storeScope);
            }

            return View("~/Plugins/Payments.YandexKassa/Views/PaymentYandexKassa/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var yandexKassaPaymentSettings = _settingService.LoadSetting<YandexKassaPaymentSettings>(storeScope);

            yandexKassaPaymentSettings.UseSandbox = model.UseSandbox;
            yandexKassaPaymentSettings.BusinessEmail = model.BusinessEmail;
            yandexKassaPaymentSettings.ShopId = Convert.ToInt32(model.ShopId);
            yandexKassaPaymentSettings.Scid = Convert.ToInt32(model.Scid);

            ///* We do not clear cache after each setting update.
            // * This behavior can increase performance because cached settings will not be cleared 
            // * and loaded from database after each update */

            if (model.UseSandbox_OverrideForStore || storeScope == 0)
               _settingService.SaveSetting(yandexKassaPaymentSettings, x => x.UseSandbox, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(yandexKassaPaymentSettings, x => x.UseSandbox, storeScope);

            if (model.BusinessEmail_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(yandexKassaPaymentSettings, x => x.BusinessEmail, storeScope, false);
            else if (storeScope > 0)
               _settingService.DeleteSetting(yandexKassaPaymentSettings, x => x.BusinessEmail, storeScope);

            if (model.ShopId_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(yandexKassaPaymentSettings, x => x.ShopId, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(yandexKassaPaymentSettings, x => x.ShopId, storeScope);

            if (model.Scid_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(yandexKassaPaymentSettings, x => x.Scid, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(yandexKassaPaymentSettings, x => x.Scid, storeScope);

            // now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var yandexKassaPaymentSettings = _settingService.LoadSetting<YandexKassaPaymentSettings>(storeScope);
            var model = new PaymentInfoModel();
            //model.modeAB = _settingService.SettingExists(yandexKassaPaymentSettings, x => x.modeAB, storeScope);

            return View("~/Plugins/Payments.YandexKassa/Views/PaymentyandexKassa/PaymentInfo.cshtml", model);
        }

        public override ProcessPaymentRequest GetPaymentInfo (FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            //paymentInfo.CreditCardName = form["modePayment"];
            //_yandexKassaMode = paymentInfo.CreditCardName.ToString();
            paymentInfo.CustomValues.Add("yandexKassaMode", form["yandexKassaMode"]);
            // paymentInfo.CreditCardNumber = form["yandexKassaMode"];
            return paymentInfo;
        }

        [ValidateInput(false)]
        public ActionResult Return(FormCollection form)
        {
            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.YandexKassa") as YandexKassaPaymentProcessor;
            if (processor == null ||
                !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("Модуль Яндекс.Кассы не может быть загружен");

            return RedirectToAction("Index", "Home", new { area = "" });


            var order = _orderService.GetOrderById(_webHelper.QueryString<int>("Order_IDP"));
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return new HttpUnauthorizedResult();

            return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
        }

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

    }
}
