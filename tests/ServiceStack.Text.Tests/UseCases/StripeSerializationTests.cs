using System;
using NUnit.Framework;
using ServiceStack.Stripe;

namespace ServiceStack.Text.Tests.UseCases
{
    [TestFixture]
    public class StripeSerializationTests
    {
        public StripeSerializationTests()
        {
            JsConfig.DateHandler = DateHandler.UnixTime;
            JsConfig.PropertyConvention = PropertyConvention.Lenient;
            JsConfig.EmitLowercaseUnderscoreNames = true;
            QueryStringSerializer.ComplexTypeStrategy = QueryStringStrategy.FormUrlEncoded;
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            JsConfig.Reset();
        }

        [Test]
        public void Serializes_Customer()
        {
            var dto = new CreateStripeCustomer
            {
                Card = new CreateStripeCard
                {
                    Name = "Name",
                    Number = "4242424242424242",
                    Cvc = "123",
                    ExpMonth = 1,
                    ExpYear = 2,
                    AddressLine1 = "1 Address",
                    AddressLine2 = "2 Address",
                    AddressCountry = "Country",
                    AddressState = "State",
                    AddressZip = "12345",
                },
                Coupon = "Coupon",
                Plan = "Plan",
                AccountBalance = 10,
                Description = "Description",
                Email = "Email",
                Quantity = 1,
                TrialEnd = new DateTime(2014, 1, 1),
            };

            var qs = QueryStringSerializer.SerializeToString(dto);
            qs.Print();
        }

        [Test]
        public void QueryString_Params_uses_DataMember_alias()
        {
            var dto = new CancelStripeSubscription { CustomerId = "cid", AtPeriodEnd = true };

            Assert.That(dto.ToGetUrl(), Is.EqualTo("/customers/cid/subscription?at_period_end=True"));

            var dto2 = new GetUpcomingStripeInvoice { Customer = "cid" };

            Assert.That(dto2.ToGetUrl(), Is.EqualTo("/invoices/upcoming?customer=cid"));
        }

        [Test]
        public void Can_convert_Stripe_Invoice()
        {
            var dto = StripeJsonData.Invoice.FromJson<StripeInvoice>();
            dto.PrintDump();
        }

        [Test]
        public void Can_convert_Stripe_Customer()
        {
            var dto = StripeJsonData.Customer.FromJson<StripeCustomer>();
            dto.PrintDump();
        }

        [Test]
        public void Can_convert_Stripe_Coupon()
        {
            var dto = StripeJsonData.Coupon.FromJson<StripeCoupon>();
            dto.PrintDump();
        }

        [Test]
        public void Can_convert_Stripe_Card()
        {
            var dto = StripeJsonData.Card.FromJson<StripeCard>();
            dto.PrintDump();
        }

        [Test]
        public void Can_convert_Stripe_Charge()
        {
            var dto = StripeJsonData.Charge.FromJson<StripeCharge>();
            dto.PrintDump();
        }
    }
}