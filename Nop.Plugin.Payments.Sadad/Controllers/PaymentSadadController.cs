using Ir.Shaparak.Sadad;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.Sadad.Factories;
using Nop.Plugin.Payments.Sadad.Models;
using Nop.Plugin.Payments.Sadad.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;

namespace Nop.Plugin.Payments.Sadad.Controllers
{
    public class PaymentSadadController : BasePaymentController
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IPermissionService _permissionService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IStoreContext _storeContext;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;
        private readonly IDataProtector _dataProtector;
        private readonly ISadadGatewayPaymentService _sadadGatewayPaymentService;
        private readonly ISadadGatewayPaymentModelFactory _sadadGatewayPaymentModelFactory;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly MerchantUtilitySoapClient _merchant;

        #endregion

        #region Ctor

        public PaymentSadadController(IWorkContext workContext,
            ISettingService settingService,
            IPaymentService paymentService,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            IPermissionService permissionService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IStoreContext storeContext,
            ILogger logger,
            IWebHelper webHelper,
            IDataProtectionProvider dataProtectionProvider,
            ISadadGatewayPaymentService sadadGatewayPaymentService,
            ISadadGatewayPaymentModelFactory sadadGatewayPaymentModelFactory,
            ShoppingCartSettings shoppingCartSettings)
        {
            _workContext = workContext;
            _settingService = settingService;
            _paymentService = paymentService;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _permissionService = permissionService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _storeContext = storeContext;
            _logger = logger;
            _webHelper = webHelper;
            _dataProtector = dataProtectionProvider.CreateProtector(NopSmartDefaults.SecretDataProvider);
            _sadadGatewayPaymentService = sadadGatewayPaymentService;
            _sadadGatewayPaymentModelFactory = sadadGatewayPaymentModelFactory;
            _shoppingCartSettings = shoppingCartSettings;
            _merchant = new MerchantUtilitySoapClient(MerchantUtilitySoapClient.EndpointConfiguration.MerchantUtilitySoap);
        }

        #endregion

        #region Utilities

        private bool CheckRequestIsSuccessful(string appstatus, int statusCode)
        {
            return (appstatus == "COMMIT" && statusCode == 0);
        }

        #endregion

        #region Methods

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var sadadPaymentSettings = _settingService.LoadSetting<SadadPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                MerchantId = sadadPaymentSettings.MerchantId,
                TerminalId = sadadPaymentSettings.TerminalId,
                TransacionKey = sadadPaymentSettings.TransacionKey,
                CurrencyIsToman = sadadPaymentSettings.CurrencyIsToman,
                AdditionalFee = sadadPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = sadadPaymentSettings.AdditionalFeePercentage,
                ActiveStoreScopeConfiguration = storeScope
            };
            if (storeScope > 0)
            {
                model.MerchantId_OverrideForStore = _settingService.SettingExists(sadadPaymentSettings, x => x.MerchantId, storeScope);
                model.TerminalId_OverrideForStore = _settingService.SettingExists(sadadPaymentSettings, x => x.TerminalId, storeScope);
                model.TransacionKey_OverrideForStore = _settingService.SettingExists(sadadPaymentSettings, x => x.TransacionKey, storeScope);
                model.CurrencyIsToman_OverrideForStore = _settingService.SettingExists(sadadPaymentSettings, x => x.CurrencyIsToman, storeScope);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(sadadPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(sadadPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
            }

            return View("~/Plugins/Payments.Sadad/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var sadadPaymentSettings = _settingService.LoadSetting<SadadPaymentSettings>(storeScope);

            //save settings
            sadadPaymentSettings.MerchantId = model.MerchantId;
            sadadPaymentSettings.TerminalId = model.TerminalId;
            sadadPaymentSettings.TransacionKey = model.TransacionKey;
            sadadPaymentSettings.CurrencyIsToman = model.CurrencyIsToman;
            sadadPaymentSettings.AdditionalFee = model.AdditionalFee;
            sadadPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(sadadPaymentSettings, x => x.MerchantId, model.MerchantId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(sadadPaymentSettings, x => x.TerminalId, model.TerminalId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(sadadPaymentSettings, x => x.TransacionKey, model.TransacionKey_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(sadadPaymentSettings, x => x.CurrencyIsToman, model.CurrencyIsToman_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(sadadPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(sadadPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [AuthorizeAdmin]
        [AdminAntiForgery]
        [Area(AreaNames.Admin)]
        public virtual IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //prepare model
            var model = _sadadGatewayPaymentModelFactory.PrepareGatewayPaymentSearchModel(new SadadGatewayPaymentSearchModel());
            return View("~/Plugins/Payments.Sadad/Views/List.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [Area(AreaNames.Admin)]
        public virtual IActionResult PaymentList(SadadGatewayPaymentSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedDataTablesJson();

            //prepare model
            var model = _sadadGatewayPaymentModelFactory.PrepareGatewayPaymentListModel(searchModel);

            return Json(model);
        }

        [HttpGet]
        public IActionResult Verify(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToRoute("HomePage");
            }

            string message = null;
            var idUnProtect = _dataProtector.Unprotect(id);
            if (Int32.TryParse(idUnProtect, out int paymentRequestId))
            {
                var paymentRequest = _sadadGatewayPaymentService.GetSadadGatewayPaymentById(paymentRequestId);
                if (paymentRequest == null)
                {
                    message = _localizationService.GetResource("Plugins.Payments.Sadad.VerifyError");
                }
                else if (paymentRequest.PaymentSucceed)
                {
                    return RedirectToRoute("CheckoutCompleted", new { orderId = paymentRequest.OrderId });
                }
                else
                {
                    ViewBag.RePayId = paymentRequest.OrderId;
                    message = _localizationService.GetResource("Plugins.Payments.Sadad.Error");
                }
            }
            else
            {
                message = _localizationService.GetResource("Plugins.Payments.Sadad.Error");
            }
            ViewBag.PaymentMessage = message;

            return View("~/Plugins/Payments.Sadad/Views/Verify.cshtml");
        }

        [HttpPost]
        [ActionName("Verify")]
        public IActionResult VerifyCallback(string id)
        {
            string message = null;
            if (Request.Form.ContainsKey("OrderId")
                && !string.IsNullOrEmpty(Request.Form["OrderId"]) && Int32.TryParse(Request.Form["OrderId"], out int paymentRequestId))
            {
                var paymentRequest = _sadadGatewayPaymentService.GetSadadGatewayPaymentById(paymentRequestId);
                if (paymentRequest == null)
                {
                    message = _localizationService.GetResource("Plugins.Payments.Sadad.VerifyError");
                }
                else
                {
                    ViewBag.RePayId = paymentRequest.OrderId;
                    if (!paymentRequest.PaymentSucceed && !paymentRequest.ReturnedFromBankSiteOnUtc.HasValue)
                    {
                        string cardAcqID = paymentRequest.MerchantId; //merchant Id - username
                        string terminalId = paymentRequest.TerminalId;
                        string transacionKey = paymentRequest.TransacionKey; //password

                        paymentRequest.ReturnedFromBankSiteOnUtc = DateTime.UtcNow;
                        _sadadGatewayPaymentService.Update(paymentRequest);

                        string retrivalRefNo = null;
                        string appstatus = null;
                        int? statusCode = null;
                        string realTransactionDateTime = null;

                        try
                        {
                            var sadadResult = _merchant.CheckRequestStatusWithRealTransactionDateTimeAsync(new CheckRequestStatusWithRealTransactionDateTimeRequest
                            {
                                OrderID = paymentRequestId,
                                CardAcqID = cardAcqID,
                                TerminalID = terminalId,
                                TransactionKey = transacionKey,
                                RequestKey = paymentRequest.RequestKey,
                                AmountTrans = Convert.ToInt64(paymentRequest.Amount)
                            }).Result;

                            retrivalRefNo = sadadResult.RetrivalRefNo;
                            appstatus = sadadResult.AppStatus;
                            realTransactionDateTime = sadadResult.RealTransactionDateTime;
                            statusCode = sadadResult.CheckRequestStatusWithRealTransactionDateTimeResult;

                            if (this.CheckRequestIsSuccessful(appstatus, statusCode.Value))
                            {
                                //عملیات خرید با موفقیت انجام شده است
                                paymentRequest.PaymentSucceed = true;
                                message = string.Format(_localizationService.GetResource("Plugins.Payments.Sadad.VerifySucceeded"), retrivalRefNo);
                            }
                            else
                            {
                                message = _localizationService.GetResource("Plugins.Payments.Sadad.VerifyError");
                            }
                        }
                        catch (Exception err)
                        {
                            _logger.Error("Error when verify payment (Sadad)!!!", err);
                            message = _localizationService.GetResource("Plugins.Payments.Sadad.VerifyError");
                        }

                        paymentRequest.RealTransactionDateTime = realTransactionDateTime;
                        paymentRequest.AppStatusCode = statusCode.HasValue ? Convert.ToInt16(statusCode.Value) : new short?();
                        paymentRequest.AppStatusDescription = appstatus;
                        paymentRequest.RetrivalRefNo = retrivalRefNo;
                        paymentRequest.PaymentVerificationOnUtc = DateTime.UtcNow;
                        _sadadGatewayPaymentService.Update(paymentRequest);

                        if (paymentRequest.PaymentSucceed)
                        {
                            Order order = this._orderService.GetOrderById(Convert.ToInt32(paymentRequest.OrderId));
                            if (_orderProcessingService.CanMarkOrderAsPaid(order))
                            {
                                order.AuthorizationTransactionResult = string.Format("[AppStatusCode = {0}, AppStatusDescription = {1}]", statusCode, appstatus);
                                order.AuthorizationTransactionId = retrivalRefNo;
                                _orderService.UpdateOrder(order);
                                _orderProcessingService.MarkOrderAsPaid(order);

                                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
                            }
                        }
                    }
                    else if (paymentRequest.PaymentSucceed)
                    {
                        message = _localizationService.GetResource("Plugins.Payments.Sadad.VerifySucceeded");
                    }
                    else
                    {
                        message = _localizationService.GetResource("Plugins.Payments.Sadad.VerifyError");
                    }
                }
            }
            else
            {
                message = _localizationService.GetResource("Plugins.Payments.Sadad.Error");
            }
            ViewBag.PaymentMessage = message;

            return View("~/Plugins/Payments.Sadad/Views/Verify.cshtml");
        }

        public IActionResult RedirectToBank(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToRoute("HomePage");
            }

            string message = null;
            if (id == NopSmartDefaults.ErrorCode1)
            {
                message = _localizationService.GetResource("Plugins.Payments.Sadad.Error");

            }
            else
            {
                var idUnProtect = _dataProtector.Unprotect(id);
                if (Int32.TryParse(idUnProtect, out int paymentRequestId))
                {
                    var paymentRequest = _sadadGatewayPaymentService.GetSadadGatewayPaymentById(paymentRequestId);
                    if (paymentRequest == null)
                        message = _localizationService.GetResource("Plugins.Payments.Sadad.Error");

                    if (!paymentRequest.PaymentSucceed && !paymentRequest.ReturnedFromBankSiteOnUtc.HasValue)
                    {
                        var backUrl = $"{_webHelper.GetStoreLocation()}Plugins/PaymentSadad/Verify/{_dataProtector.Protect(paymentRequest.Id.ToString())}";
                        message = $"<script language='javascript' type='text/javascript'> postSadadData('{paymentRequest.MerchantId}', '{Math.Round(paymentRequest.Amount, 0)}', '{paymentRequest.Id}', '{paymentRequest.TerminalId}', '{paymentRequest.TimeStamp}', '{paymentRequest.FingerPrint}', '{backUrl}', '{paymentRequest.MerchantAdditionalData}', '');</script> {_localizationService.GetResource("Plugins.Payments.Sadad.RedirectToBank")}";
                    }
                }
                else
                {
                    message = _localizationService.GetResource("Plugins.Payments.Sadad.Error");
                }
            }
            ViewBag.PaymentMessage = message;

            return View("~/Plugins/Payments.Sadad/Views/RedirectToBank.cshtml");
        }

        #endregion
    }
}