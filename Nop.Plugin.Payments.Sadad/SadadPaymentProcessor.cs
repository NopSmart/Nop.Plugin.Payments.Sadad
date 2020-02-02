using Ir.Shaparak.Sadad;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.Sadad.Data;
using Nop.Plugin.Payments.Sadad.Domain;
using Nop.Plugin.Payments.Sadad.Extensions;
using Nop.Plugin.Payments.Sadad.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Tax;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Nop.Plugin.Payments.Sadad
{
    /// <summary>
    /// Sadad payment processor
    /// </summary>
    public class SadadPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly CurrencySettings _currencySettings;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICurrencyService _currencyService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly IPaymentService _paymentService;
        private readonly ISettingService _settingService;
        private readonly ITaxService _taxService;
        private readonly IWebHelper _webHelper;
        private readonly IDataProtector _dataProtector;
        private readonly PaymentObjectContext _context;
        private readonly ISadadGatewayPaymentService _sadadGatewayPaymentService;
        private readonly SadadPaymentSettings _sadadPaymentSettings;
        private readonly MerchantUtilitySoapClient _merchant;

        #endregion

        #region Ctor

        public SadadPaymentProcessor(CurrencySettings currencySettings,
            ICheckoutAttributeParser checkoutAttributeParser,
            ICurrencyService currencyService,
            IGenericAttributeService genericAttributeService,
            IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            IPaymentService paymentService,
            ISettingService settingService,
            ITaxService taxService,
            IWebHelper webHelper,
            PaymentObjectContext context,
            IDataProtectionProvider dataProtectionProvider,
            ISadadGatewayPaymentService sadadGatewayPaymentService,
            SadadPaymentSettings sadadPaymentSettings)
        {
            this._currencySettings = currencySettings;
            this._checkoutAttributeParser = checkoutAttributeParser;
            this._currencyService = currencyService;
            this._genericAttributeService = genericAttributeService;
            this._httpContextAccessor = httpContextAccessor;
            this._localizationService = localizationService;
            this._paymentService = paymentService;
            this._settingService = settingService;
            this._taxService = taxService;
            this._webHelper = webHelper;
            this._context = context;
            this._dataProtector = dataProtectionProvider.CreateProtector(NopSmartDefaults.SecretDataProvider);
            this._sadadGatewayPaymentService = sadadGatewayPaymentService;
            this._sadadPaymentSettings = sadadPaymentSettings;
            this._merchant = new MerchantUtilitySoapClient(MerchantUtilitySoapClient.EndpointConfiguration.MerchantUtilitySoap);
        }

        #endregion

        #region Utilities

        private string CalcRequestKey(string CardAcqID, string TransacionKey, long OrderId, string RequestFP, string Timestamp)
        {
            string textInput = string.Concat(CardAcqID, OrderId.ToString(), RequestFP, TransacionKey);
            MD5 hash = MD5.Create();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] Input = encoding.GetBytes(textInput);
            byte[] result = hash.ComputeHash(Input);
            string RequestKey = Timestamp + BitConverter.ToString(result);
            RequestKey = RequestKey.Replace("-", "").ToLower();
            return RequestKey;
        }

        private string CalcFpOrder(string CardAcqID, decimal AmountTrans, string TransacionKey, long OrderId, string Timestamp)
        {
            string textInput = string.Concat(CardAcqID, OrderId.ToString(), AmountTrans.ToString("0"), TransacionKey, Timestamp);
            MD5 hash = MD5.Create();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] Input = encoding.GetBytes(textInput);
            byte[] result = hash.ComputeHash(Input);
            string Fp = BitConverter.ToString(result);
            return Fp;
        }

        private byte[] GetClientIPAddress()
        {
            var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress ?? null;

            if (ipAddress == null)
            {
                Microsoft.Extensions.Primitives.StringValues ipStrVal;
                if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue("X-Forwarded-For", out ipStrVal))
                {
                    System.Net.IPAddress netIPAddress;
                    if (System.Net.IPAddress.TryParse(ipStrVal, out netIPAddress) || IPAddressExtensions.TryParseIPAddress(ipStrVal, out netIPAddress))
                    {
                        ipAddress = netIPAddress;
                    }
                }
            }

            if (ipAddress == null)
            {
                return null;
            }

            return ipAddress.GetAddressBytes();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult();
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            ////create common query parameters for the request
            //var queryParameters = CreateQueryParameters(postProcessPaymentRequest);

            ////whether to include order items in a transaction
            //if (_paypalStandardPaymentSettings.PassProductNamesAndTotals)
            //{
            //    //add order items query parameters to the request
            //    var parameters = new Dictionary<string, string>(queryParameters);
            //    AddItemsParameters(parameters, postProcessPaymentRequest);

            //    //remove null values from parameters
            //    parameters = parameters.Where(parameter => !string.IsNullOrEmpty(parameter.Value))
            //        .ToDictionary(parameter => parameter.Key, parameter => parameter.Value);

            //    //ensure redirect URL doesn't exceed 2K chars to avoid "too long URL" exception
            //    var redirectUrl = QueryHelpers.AddQueryString(GetPaypalUrl(), parameters);
            //    if (redirectUrl.Length <= 2048)
            //    {
            //        _httpContextAccessor.HttpContext.Response.Redirect(redirectUrl);
            //        return;
            //    }
            //}

            ////or add only an order total query parameters to the request
            //AddOrderTotalParameters(queryParameters, postProcessPaymentRequest);

            ////remove null values from parameters
            //queryParameters = queryParameters.Where(parameter => !string.IsNullOrEmpty(parameter.Value))
            //    .ToDictionary(parameter => parameter.Key, parameter => parameter.Value);

            //var url = QueryHelpers.AddQueryString(GetPaypalUrl(), queryParameters);
            //_httpContextAccessor.HttpContext.Response.Redirect(url);

            //_behPardakhtPaymentSettings.SaveTransaction
            decimal totalAmount = Math.Round(postProcessPaymentRequest.Order.OrderTotal, 0);
            //decimal totalAmount = 100;
            if (_sadadPaymentSettings.CurrencyIsToman)
            {
                totalAmount *= 10;
            }
            var payment = new SadadGatewayPayment
            {
                Amount = totalAmount,
                CreatedOnUtc = DateTime.UtcNow,
                Description = null,
                ClientIPAddress = this.GetClientIPAddress(),
                OrderId = postProcessPaymentRequest.Order.Id,
                MerchantId = _sadadPaymentSettings.MerchantId,
                TerminalId = _sadadPaymentSettings.TerminalId,
                TransacionKey = _sadadPaymentSettings.TransacionKey
            };
            _sadadGatewayPaymentService.Insert(payment);

            string cardAcqID = payment.MerchantId; //merchant Id - username
            string terminalId = payment.TerminalId;
            string transacionKey = payment.TransacionKey;

            string timeStamp = _merchant.CalcTimeStampAsync().Result;
            string fp = this.CalcFpOrder(cardAcqID, totalAmount, transacionKey, payment.Id, timeStamp);
            string requestKey = this.CalcRequestKey(cardAcqID, transacionKey, payment.Id, fp, timeStamp);

            if (!string.IsNullOrEmpty(requestKey))
            {
                payment.RequestedKeyOnUtc = DateTime.UtcNow;
                payment.RequestKey = requestKey;
                payment.MerchantAdditionalData = postProcessPaymentRequest.Order.Id.ToString();
                payment.TimeStamp = timeStamp;
                payment.FingerPrint = fp;
                _sadadGatewayPaymentService.Update(payment);
                _httpContextAccessor.HttpContext.Response.Redirect($"{_webHelper.GetStoreLocation()}Plugins/PaymentSadad/RedirectToBank/{_dataProtector.Protect(payment.Id.ToString())}");
            }
            else
            {
                _httpContextAccessor.HttpContext.Response.Redirect($"{_webHelper.GetStoreLocation()}Plugins/PaymentSadad/RedirectToBank/{NopSmartDefaults.ErrorCode1}");
            }
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return _paymentService.CalculateAdditionalFee(cart,
                _sadadPaymentSettings.AdditionalFee, _sadadPaymentSettings.AdditionalFeePercentage);
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            return new CapturePaymentResult { Errors = new[] { "Capture method not supported" } };
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            return new RefundPaymentResult { Errors = new[] { "Refund method not supported" } };
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            return new VoidPaymentResult { Errors = new[] { "Void method not supported" } };
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //let's ensure that at least 5 seconds passed after order is placed
            //P.S. there's no any particular reason for that. we just do it
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 5)
                return false;

            return true;
        }

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>List of validating errors</returns>
        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            return new List<string>();
        }

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>Payment info holder</returns>
        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            return new ProcessPaymentRequest();
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentSadad/Configure";
        }

        /// <summary>
        /// Gets a name of a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <returns>View component name</returns>
        public string GetPublicViewComponentName()
        {
            return "PaymentSadad";
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //settings
            _settingService.SaveSetting(new SadadPaymentSettings
            {
                MerchantId = string.Empty,
                TerminalId = string.Empty,
                TransacionKey = string.Empty,
                CurrencyIsToman = true
            });

            //locales
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Sadad.Fields.AdditionalFee", "هزینه های اضافی");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Sadad.Fields.AdditionalFee.Hint", "هزینه های اضافی برای مطالبه از مشتریان خود وارد نمایید");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Sadad.Fields.AdditionalFeePercentage", "هزینه اضافی. از درصد استفاده نمایید");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Sadad.Fields.AdditionalFeePercentage.Hint", "تعیین اینکه آیا  درصد هزینه های اضافی به کل سفارش اعمال شود. اگر فعال نشود ، یک مقدار ثابت استفاده می شود");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Sadad.Fields.MerchantId", "شماره پذیرنده");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Sadad.Fields.MerchantId.Hint", "شماره پذیرنده (مرچنت آی دی) درگاه سداد می باشد.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Sadad.Fields.TerminalId", "شماره ترمینال");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Sadad.Fields.TerminalId.Hint", "شماره ترمینال درگاه سداد می باشد.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Sadad.Fields.TransacionKey", "کد تراکنش");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Sadad.Fields.TransacionKey.Hint", "کد تراکنش درگاه سداد می باشد.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Sadad.Fields.CurrencyIsToman", "واحد پولی تومان");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Sadad.Fields.CurrencyIsToman.Hint", "درصورتی که واحد پولی شما تومان می باشد این آیتم باید فعال باشد.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Sadad.Instructions", "درگاه پرداخت اینترنتی شرکت پرداخت الکترونیک سداد");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Sadad.PaymentMethodDescription", "پرداخت الکترونیک سداد");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Sadad.Error", "درخواست شما نا معتبر است.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Sadad.Fields.RedirectToBank", "در حال انتقال به سایت بانک...");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Sadad.Fields.RedirectionTip", "جهت پرداخت به سایت بانک هدایت می شوید.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Sadad.VerifySucceeded", "عملیات پرداخت بانکی با موفقیت انجام شده است.<br />شماره پیگیری: {0}");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Sadad.VerifyError", "نتیجه تراکنش پرداخت بانک شما ناموفق می باشد.");

            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Sadad.PaymentList", "لیست تراکنش ها");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Sadad.Fields.Id", "شماره تراکنش");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Sadad.Fields.OrderId", "شماره سفارش");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Sadad.Fields.Amount", "مبلغ تراکنش");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Sadad.Fields.ClientIPAddress", "آی پی کاربر");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Sadad.Fields.CreatedOn", "زمان درخواست");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Sadad.Fields.RequestedKeyOn", "زمان گرفتن توکن");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Sadad.Fields.ReturnedFromBankSiteOn", "زمان بازگشت از سایت بانک");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Sadad.Fields.PaymentVerificationOn", "زمان گرفتن تایید پرداخت");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Sadad.Fields.PaymentSucceed", "وضعیت تراکنش");

            //database objects
            this._context.Install();

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<SadadPaymentSettings>();

            //locales
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Sadad.Fields.AdditionalFee");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Sadad.Fields.AdditionalFee.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Sadad.Fields.AdditionalFeePercentage");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Sadad.Fields.AdditionalFeePercentage.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Sadad.Fields.MerchantId");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Sadad.Fields.MerchantId.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Sadad.Fields.TerminalId");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Sadad.Fields.TerminalId.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Sadad.Fields.TransacionKey");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Sadad.Fields.TransacionKey.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Sadad.Fields.CurrencyIsToman");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Sadad.Fields.CurrencyIsToman.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Sadad.Instructions");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Sadad.PaymentMethodDescription");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Sadad.Error");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Sadad.Fields.RedirectToBank");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Sadad.Fields.RedirectionTip");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Sadad.VerifySucceeded");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Sadad.VerifyError");

            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Sadad.PaymentList");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Sadad.Fields.Id");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Sadad.Fields.OrderId");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Sadad.Fields.Amount");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Sadad.Fields.ClientIPAddress");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Sadad.Fields.CreatedOn");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Sadad.Fields.RequestedKeyOn");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Sadad.Fields.ReturnedFromBankSiteOn");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Sadad.Fields.PaymentVerificationOn");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Sadad.Fields.PaymentSucceed");

            //database objects
            this._context.Uninstall();

            base.Uninstall();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get { return RecurringPaymentType.NotSupported; }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get { return PaymentMethodType.Redirection; }
        }

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public string PaymentMethodDescription
        {
            //return description of this payment method to be display on "payment method" checkout step. good practice is to make it localizable
            //for example, for a redirection payment method, description may be like this: "You will be redirected to PayPal site to complete the payment"
            get { return _localizationService.GetResource("Plugins.Payments.Sadad.PaymentMethodDescription"); }
        }

        #endregion
    }
}