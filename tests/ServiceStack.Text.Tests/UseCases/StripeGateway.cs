// Copyright (c) Service Stack LLC. All Rights Reserved.
// License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using ServiceStack.Text;

namespace ServiceStack.Stripe
{
    public class StripeGateway
    {
        private const string BaseUrl = "https://api.stripe.com/v1";

        public TimeSpan Timeout { get; set; }

        public ICredentials Credentials { get; set; }
        private string UserAgent { get; set; }

        public StripeGateway(string apiKey)
        {
            Credentials = new NetworkCredential(apiKey, "");
            Timeout = TimeSpan.FromSeconds(60);
            UserAgent = "servicestack .net stripe v1";
            JsConfig.InitStatics();
        }

        protected virtual string Send(string relativeUrl, string method, string body)
        {
            try
            {
                var url = BaseUrl.CombineWith(relativeUrl);
                var response = url.SendStringToUrl(method: method, requestBody: body, requestFilter: req =>
                {
                    req.Accept = MimeTypes.Json;
                    req.UserAgent = UserAgent;
                    req.Credentials = Credentials;
                    req.PreAuthenticate = true;
                    req.Timeout = (int)Timeout.TotalMilliseconds;
                    if (method == HttpMethods.Post || method == HttpMethods.Put)
                        req.ContentType = MimeTypes.FormUrlEncoded;
                });

                return response;
            }
            catch (WebException ex)
            {
                var errorBody = ex.GetResponseBody();
                var errorStatus = ex.GetStatus() ?? HttpStatusCode.BadRequest;
                if (ex.IsAny400())
                {
                    var result = errorBody.FromJson<StripeErrors>();
                    throw new StripeException(result.Error) { StatusCode = errorStatus };
                }

                throw;
            }
        }

        class ConfigScope : IDisposable
        {
            private readonly WriteComplexTypeDelegate holdQsStrategy;
            private readonly JsConfigScope scope;

            public ConfigScope()
            {
                scope = JsConfig.With(dateHandler: DateHandler.UnixTime,
                                      propertyConvention: PropertyConvention.Lenient,
                                      emitLowercaseUnderscoreNames: true);

                holdQsStrategy = QueryStringSerializer.ComplexTypeStrategy;
                QueryStringSerializer.ComplexTypeStrategy = QueryStringStrategy.FormUrlEncoded;
            }

            public void Dispose()
            {
                QueryStringSerializer.ComplexTypeStrategy = holdQsStrategy;
                scope.Dispose();
            }
        }

        private T Send<T>(IReturn<T> request, string method, bool sendRequestBody = true)
        {
            using (new ConfigScope())
            {
                var relativeUrl = request.ToUrl(method);
                var body = sendRequestBody ? QueryStringSerializer.SerializeToString(request) : null;

                var json = Send(relativeUrl, method, body);

                var response = json.FromJson<T>();
                return response;
            }
        }

        public T Get<T>(IReturn<T> request)
        {
            return Send(request, HttpMethods.Get, sendRequestBody: false);
        }

        public T Post<T>(IReturn<T> request)
        {
            return Send(request, HttpMethods.Post);
        }

        public T Put<T>(IReturn<T> request)
        {
            return Send(request, HttpMethods.Put);
        }

        public T Delete<T>(IReturn<T> request)
        {
            return Send(request, HttpMethods.Delete, sendRequestBody: false);
        }
    }

    [Route("/customers/{Id}")]
    public class GetCustomer : IReturn<StripeCustomer>
    {
        public string Id { get; set; }
    }

    [Route("/customers")]
    public class CreateStripeCustomer : IReturn<StripeCustomer>
    {
        public int AccountBalance { get; set; }
        public CreateStripeCard Card { get; set; }
        public string Coupon { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public string Plan { get; set; }
        public int? Quantity { get; set; }
        public DateTime? TrialEnd { get; set; }
    }

    [Route("/customers/{Id}")]
    public class UpdateStripeCustomer : IReturn<StripeCustomer>
    {
        [IgnoreDataMember]
        public string Id { get; set; }
        public int AccountBalance { get; set; }
        public StripeCard Card { get; set; }
        public string Coupon { get; set; }
        public string DefaultCard { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
    }

    [Route("/charges")]
    public class ChargeStripeCustomer : IReturn<StripeCharge>
    {
        public int Amount { get; set; }
        public string Currency { get; set; }
        public string Customer { get; set; }
        public string Card { get; set; }
        public string Description { get; set; }
        public bool? Capture { get; set; }
        public int? ApplicationFee { get; set; }
    }

    [Route("/customers/{CustomerId}/subscription")]
    public class SubscribeStripeCustomer : IReturn<StripeSubscription>
    {
        [IgnoreDataMember]
        public string CustomerId { get; set; }
        public string Plan { get; set; }
        public string Coupon { get; set; }
        public bool? Prorate { get; set; }
        public DateTime? TrialEnd { get; set; }
        public string Card { get; set; }
        public int? Quantity { get; set; }
        public int? ApplicationFeePercent { get; set; }
    }

    [Route("/customers/{CustomerId}/subscription")]
    public class CancelStripeSubscription : IReturn<StripeSubscription>
    {
        public string CustomerId { get; set; }
        public bool AtPeriodEnd { get; set; }
    }

    [Route("/plans/{Id}")]
    public class GetStripePlan : IReturn<StripePlan>
    {
        public string Id { get; set; }
    }

    [Route("/plans")]
    public class CreateStripePlan : IReturn<StripePlan>
    {
        public string Id { get; set; }
        public int Amount { get; set; }
        public string Currency { get; set; }
        public StripePlanInterval Interval { get; set; }
        public int? IntervalCount { get; set; }
        public string Name { get; set; }
        public int? TrialPeriodDays { get; set; }
    }

    [Route("/plans")]
    public class GetStripePlans : IReturn<StripeResults<StripePlan>>
    {
        public int? Count { get; set; }
        public int? Offset { get; set; }
    }

    [Route("/plans/{Id}")]
    public class DeleteStripePlan : IReturn<StripeReference>
    {
        public string Id { get; set; }
    }

    [Route("/coupons/{Id}")]
    public class GetStripeCoupon : IReturn<StripeCoupon>
    {
        public string Id { get; set; }
    }

    [Route("/coupons")]
    public class CreateStripeCoupon : IReturn<StripeCoupon>
    {
        public string Id { get; set; }
        public StripeCouponDuration Duration { get; set; }
        public int? AmountOff { get; set; }
        public string Currency { get; set; }
        public int? DurationInMonths { get; set; }
        public int? MaxRedemptions { get; set; }
        public int? PercentOff { get; set; }
        public DateTime? RedeemBy { get; set; }
    }

    [Route("/coupons")]
    public class GetStripeCoupons : IReturn<StripeResults<StripeCoupon>>
    {
        public int? Count { get; set; }
        public int? Offset { get; set; }
    }

    [Route("/coupons/{Id}")]
    public class DeleteStripeCoupon : IReturn<StripeReference>
    {
        public string Id { get; set; }
    }

    [Route("/customers/{CustomerId}/discount")]
    public class DeleteStripeDiscount : IReturn<StripeReference>
    {
        public string CustomerId { get; set; }
    }

    [Route("/invoices")]
    public class CreateStripeInvoice : IReturn<StripeInvoice>
    {
        public string Customer { get; set; }
        public int? ApplicationFee { get; set; }
    }

    [Route("/invoices/{Id}/pay")]
    public class PayStripeInvoice : IReturn<StripeInvoice>
    {
        [IgnoreDataMember]
        public string Id { get; set; }
    }

    [Route("/invoices/upcoming")]
    public class GetUpcomingStripeInvoice : IReturn<StripeInvoice>
    {
        public string Customer { get; set; }
    }

    public class StripeErrors
    {
        public StripeError Error { get; set; }
    }

    public class StripeError
    {
        public string Type { get; set; }
        public string Message { get; set; }
        public string Code { get; set; }
        public string Param { get; set; }
    }

    public class StripeException : Exception
    {
        public StripeException(StripeError error)
            : base(error.Message)
        {
            Code = error.Code;
            Param = error.Param;
        }

        public string Code { get; set; }
        public string Param { get; set; }
        public HttpStatusCode StatusCode { get; set; }

    }

    public class StripeReference
    {
        public string Id { get; set; }
        public bool Deleted { get; set; }
    }

    public class StripeEntity
    {
        public StripeType Object { get; set; }
    }

    public class StripeId : StripeEntity
    {
        public string Id { get; set; }
    }

    public enum StripeType
    {
        Unknown,
        Account,
        Card,
        Charge,
        Coupon,
        Customer,
        Discount,
        Dispute,
        Event,
        InvoiceItem,
        Invoice,
        Line_Item,
        Plan,
        Subscription,
        Token,
        Transfer,
        List,
    }

    public class StripeInvoice : StripeId
    {
        public DateTime Date { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public StripeResults<StripeLineItem> Lines { get; set; }
        public int Subtotal { get; set; }
        public int Total { get; set; }
        public string Customer { get; set; }
        public bool Attempted { get; set; }
        public bool Closed { get; set; }
        public bool Paid { get; set; }
        public bool Livemode { get; set; }
        public int AttemptCount { get; set; }
        public int AmountDue { get; set; }
        public string Currency { get; set; }
        public int StartingBalance { get; set; }
        public int? EndingBalance { get; set; }
        public DateTime? NextPaymentAttempt { get; set; }
        public string Charge { get; set; }
        public StripeDiscount Discount { get; set; }
        public int? ApplicationFee { get; set; }
    }

    public class StripeResults<T> : StripeId
    {
        public string Url { get; set; }
        public int Count { get; set; }
        public List<T> Data { get; set; }
    }

    public class StripeLineItem : StripeId
    {
        public string Type { get; set; }
        public bool Livemode { get; set; }
        public int Amount { get; set; }
        public string Currency { get; set; }
        public bool Proration { get; set; }
        public StripePeriod Period { get; set; }
        public int? Quantity { get; set; }
        public StripePlan Plan { get; set; }
        public string Description { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }

    public class StripePlan : StripeId
    {
        public bool Livemode { get; set; }
        public int Amount { get; set; }
        public string Currency { get; set; }
        public string Identifier { get; set; }
        public StripePlanInterval Interval { get; set; }
        public string Name { get; set; }
        public int? TrialPeriodDays { get; set; }
    }

    public class StripePeriod
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }

    public enum StripePlanInterval
    {
        month,
        year
    }

    public class StripeDiscount : StripeId
    {
        public string Customer { get; set; }
        public StripeCoupon Coupon { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
    }

    public class StripeCoupon : StripeId
    {
        public int? PercentOff { get; set; }
        public int? AmountOff { get; set; }
        public string Currency { get; set; }
        public bool Livemode { get; set; }
        public StripeCouponDuration Duration { get; set; }
        public DateTime? RedeemBy { get; set; }
        public int? MaxRedemptions { get; set; }
        public int TimesRedeemed { get; set; }
        public int? DurationInMonths { get; set; }
    }

    public enum StripeCouponDuration
    {
        forever,
        once,
        repeating
    }

    public class StripeCustomer : StripeId
    {
        public DateTime? Created { get; set; }
        public bool Livemode { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public bool? Deliquent { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
        public StripeSubscription Subscription { get; set; }
        public StripeDiscount Discount { get; set; }
        public int AccountBalance { get; set; }
        public StripeResults<StripeCard> Cards { get; set; }
        public bool Deleted { get; set; }
        public string DefaultCard { get; set; }
    }

    public class DeleteStripeCustomer
    {
        public string Id { get; set; }
    }

    public class GetAllStripeCustomers
    {
        public int? Count { get; set; }
        public int? Offset { get; set; }
        public StripeDateRange Created { get; set; }
    }

    public class StripeDateRange
    {
        public DateTime? Gt { get; set; }
        public DateTime? Gte { get; set; }
        public DateTime? Lt { get; set; }
        public DateTime? Lte { get; set; }
    }

    public class CreateStripeCard
    {
        public string Number { get; set; }
        public int ExpMonth { get; set; }
        public int ExpYear { get; set; }
        public string Cvc { get; set; }
        public string Name { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressZip { get; set; }
        public string AddressState { get; set; }
        public string AddressCountry { get; set; }
    }

    public class StripeCard : StripeId
    {
        public string Last4 { get; set; }
        public string Type { get; set; }
        public string Number { get; set; }
        public int ExpMonth { get; set; }
        public int ExpYear { get; set; }
        public string Cvc { get; set; }
        public string Name { get; set; }

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressCity { get; set; }
        public string AddressState { get; set; }
        public string AddressZip { get; set; }
        public string AddressCountry { get; set; }
        public StripeCvcCheck? CvcCheck { get; set; }
        public string AddressLine1Check { get; set; }
        public string AddressZipCheck { get; set; }

        public string Fingerprint { get; set; }
        public string Customer { get; set; }
        public string Country { get; set; }
    }

    public enum StripeCvcCheck
    {
        Unknown,
        Pass,
        Fail,
        Unchecked
    }

    public class StripeSubscription : StripeId
    {
        public DateTime? CurrentPeriodEnd { get; set; }
        public StripeSubscriptionStatus Status { get; set; }
        public StripePlan Plan { get; set; }
        public DateTime? CurrentPeriodStart { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? TrialStart { get; set; }
        public bool? CancelAtPeriodEnd { get; set; }
        public DateTime? TrialEnd { get; set; }
        public DateTime? CanceledAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public string Customer { get; set; }
        public int Quantity { get; set; }
    }

    public enum StripeSubscriptionStatus
    {
        Unknown,
        Trialing,
        Active,
        PastDue,
        Canceled,
        Unpaid
    }

    public class StripeCharge : StripeId
    {
        public bool LiveMode { get; set; }
        public int Amount { get; set; }
        public bool Captured { get; set; }
        public StripeCard Card { get; set; }
        public DateTime Created { get; set; }
        public string Currency { get; set; }
        public bool Paid { get; set; }
        public bool Refunded { get; set; }
        public List<StripeRefund> Refunds { get; set; }
        public int AmountRefunded { get; set; }
        public string BalanceTransaction { get; set; }
        public string Customer { get; set; }
        public string Description { get; set; }
        public StripeDispute Dispute { get; set; }
        public string FailureCode { get; set; }
        public string FailureMessage { get; set; }
        public string Invoice { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }

    public class CreateStripeCharge : StripeId
    {
        public int Amount { get; set; }
        public string Currency { get; set; }
        public string Customer { get; set; }
        public StripeCard Card { get; set; }
        public string Description { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
        public bool Capture { get; set; }
        public int? ApplicationFee { get; set; }
    }

    public class GetStripeCharge
    {
        public string Id { get; set; }
    }

    public class UpdateStripeCharge
    {
        public string Description { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }

    public class StripeRefund : StripeEntity
    {
        public int Amount { get; set; }
        public DateTime Created { get; set; }
        public string Currency { get; set; }
        public string BalanceTransaction { get; set; }
    }

    public class StripeDispute : StripeEntity
    {
        public StripeDisputeStatus Status { get; set; }
        public string Evidence { get; set; }
        public string Charge { get; set; }
        public DateTime? Created { get; set; }
        public string Currency { get; set; }
        public int Amount;
        public bool LiveMode { get; set; }
        public StripeDisputeReason Reason { get; set; }
        public DateTime? EvidenceDueBy { get; set; }
    }

    public class StripeFeeDetail
    {
        public string Type { get; set; }
        public string Currency { get; set; }
        public string Application { get; set; }
        public string Description { get; set; }
        public int Amount { get; set; }
    }

    public enum StripeDisputeStatus
    {
        Won,
        Lost,
        NeedsResponse,
        UnderReview
    }

    public enum StripeDisputeReason
    {
        Duplicate,
        Fraudulent,
        SubscriptionCanceled,
        ProductUnacceptable,
        ProductNotReceived,
        Unrecognized,
        CreditNotProcessed,
        General
    }

}