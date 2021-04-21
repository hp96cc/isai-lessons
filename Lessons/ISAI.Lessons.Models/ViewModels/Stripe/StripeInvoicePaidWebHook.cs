using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ISAI.Lessons.EntityFramework.ViewModels.Stripe
{


    public partial class StripeInvoicePaidWebHook
    {
        [JsonProperty("created")]
        public long Created { get; set; }

        [JsonProperty("livemode")]
        public bool Livemode { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("request")]
        public object Request { get; set; }

        [JsonProperty("pending_webhooks")]
        public long PendingWebhooks { get; set; }

        [JsonProperty("api_version")]
        public DateTimeOffset ApiVersion { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public partial class Data
    {
        [JsonProperty("object")]
        public Object Object { get; set; }
    }

    public partial class Object
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("object")]
        public string ObjectObject { get; set; }

        [JsonProperty("account_country")]
        public string AccountCountry { get; set; }

        [JsonProperty("account_name")]
        public string AccountName { get; set; }

        [JsonProperty("account_tax_ids")]
        public object AccountTaxIds { get; set; }

        [JsonProperty("amount_due")]
        public long AmountDue { get; set; }

        [JsonProperty("amount_paid")]
        public long AmountPaid { get; set; }

        [JsonProperty("amount_remaining")]
        public long AmountRemaining { get; set; }

        [JsonProperty("application_fee_amount")]
        public object ApplicationFeeAmount { get; set; }

        [JsonProperty("attempt_count")]
        public long AttemptCount { get; set; }

        [JsonProperty("attempted")]
        public bool Attempted { get; set; }

        [JsonProperty("auto_advance")]
        public bool AutoAdvance { get; set; }

        [JsonProperty("billing_reason")]
        public string BillingReason { get; set; }

        [JsonProperty("charge")]
        public string Charge { get; set; }

        [JsonProperty("collection_method")]
        public string CollectionMethod { get; set; }

        [JsonProperty("created")]
        public long Created { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("custom_fields")]
        public object CustomFields { get; set; }

        [JsonProperty("customer")]
        public string Customer { get; set; }

        [JsonProperty("customer_address")]
        public object CustomerAddress { get; set; }

        [JsonProperty("customer_email")]
        public string CustomerEmail { get; set; }

        [JsonProperty("customer_name")]
        public object CustomerName { get; set; }

        [JsonProperty("customer_phone")]
        public object CustomerPhone { get; set; }

        [JsonProperty("customer_shipping")]
        public object CustomerShipping { get; set; }

        [JsonProperty("customer_tax_exempt")]
        public string CustomerTaxExempt { get; set; }

        [JsonProperty("customer_tax_ids")]
        public List<object> CustomerTaxIds { get; set; }

        [JsonProperty("default_payment_method")]
        public object DefaultPaymentMethod { get; set; }

        [JsonProperty("default_source")]
        public object DefaultSource { get; set; }

        [JsonProperty("default_tax_rates")]
        public List<object> DefaultTaxRates { get; set; }

        [JsonProperty("description")]
        public object Description { get; set; }

        [JsonProperty("discount")]
        public object Discount { get; set; }

        [JsonProperty("discounts")]
        public List<object> Discounts { get; set; }

        [JsonProperty("due_date")]
        public object DueDate { get; set; }

        [JsonProperty("ending_balance")]
        public long EndingBalance { get; set; }

        [JsonProperty("footer")]
        public object Footer { get; set; }

        [JsonProperty("hosted_invoice_url")]
        public Uri HostedInvoiceUrl { get; set; }

        [JsonProperty("invoice_pdf")]
        public Uri InvoicePdf { get; set; }

        [JsonProperty("last_finalization_error")]
        public object LastFinalizationError { get; set; }

        [JsonProperty("lines")]
        public Lines Lines { get; set; }

        [JsonProperty("livemode")]
        public bool Livemode { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("next_payment_attempt")]
        public object NextPaymentAttempt { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("on_behalf_of")]
        public object OnBehalfOf { get; set; }

        [JsonProperty("paid")]
        public bool Paid { get; set; }

        [JsonProperty("payment_intent")]
        public object PaymentIntent { get; set; }

        [JsonProperty("payment_settings")]
        public PaymentSettings PaymentSettings { get; set; }

        [JsonProperty("period_end")]
        public long PeriodEnd { get; set; }

        [JsonProperty("period_start")]
        public long PeriodStart { get; set; }

        [JsonProperty("post_payment_credit_notes_amount")]
        public long PostPaymentCreditNotesAmount { get; set; }

        [JsonProperty("pre_payment_credit_notes_amount")]
        public long PrePaymentCreditNotesAmount { get; set; }

        [JsonProperty("receipt_number")]
        public object ReceiptNumber { get; set; }

        [JsonProperty("starting_balance")]
        public long StartingBalance { get; set; }

        [JsonProperty("statement_descriptor")]
        public object StatementDescriptor { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("status_transitions")]
        public StatusTransitions StatusTransitions { get; set; }

        [JsonProperty("subscription")]
        public string Subscription { get; set; }

        [JsonProperty("subtotal")]
        public long Subtotal { get; set; }

        [JsonProperty("tax")]
        public object Tax { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("total_discount_amounts")]
        public List<object> TotalDiscountAmounts { get; set; }

        [JsonProperty("total_tax_amounts")]
        public List<object> TotalTaxAmounts { get; set; }

        [JsonProperty("transfer_data")]
        public object TransferData { get; set; }

        [JsonProperty("webhooks_delivered_at")]
        public long WebhooksDeliveredAt { get; set; }

        [JsonProperty("closed")]
        public bool Closed { get; set; }
    }

    public partial class Lines
    {
        [JsonProperty("data")]
        public List<Datum> Data { get; set; }

        [JsonProperty("has_more")]
        public bool HasMore { get; set; }

        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public partial class Datum
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("amount")]
        public long Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("discount_amounts")]
        public List<object> DiscountAmounts { get; set; }

        [JsonProperty("discountable")]
        public bool Discountable { get; set; }

        [JsonProperty("discounts")]
        public List<object> Discounts { get; set; }

        [JsonProperty("livemode")]
        public bool Livemode { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("period")]
        public Period Period { get; set; }

        [JsonProperty("price")]
        public Price Price { get; set; }

        [JsonProperty("proration")]
        public bool Proration { get; set; }

        [JsonProperty("quantity")]
        public long Quantity { get; set; }

        [JsonProperty("subscription")]
        public string Subscription { get; set; }

        [JsonProperty("subscription_item")]
        public string SubscriptionItem { get; set; }

        [JsonProperty("tax_amounts")]
        public List<object> TaxAmounts { get; set; }

        [JsonProperty("tax_rates")]
        public List<object> TaxRates { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public partial class Metadata
    {
    }

    public partial class Period
    {
        [JsonProperty("end")]
        public long End { get; set; }

        [JsonProperty("start")]
        public long Start { get; set; }
    }

    public partial class Price
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("billing_scheme")]
        public string BillingScheme { get; set; }

        [JsonProperty("created")]
        public long Created { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("livemode")]
        public bool Livemode { get; set; }

        [JsonProperty("lookup_key")]
        public object LookupKey { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("product")]
        public string Product { get; set; }

        [JsonProperty("recurring")]
        public Recurring Recurring { get; set; }

        [JsonProperty("tiers_mode")]
        public object TiersMode { get; set; }

        [JsonProperty("transform_quantity")]
        public object TransformQuantity { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("unit_amount")]
        public long UnitAmount { get; set; }

        [JsonProperty("unit_amount_decimal")]
        public string UnitAmountDecimal { get; set; }
    }

    public partial class Recurring
    {
        [JsonProperty("aggregate_usage")]
        public object AggregateUsage { get; set; }

        [JsonProperty("interval")]
        public string Interval { get; set; }

        [JsonProperty("interval_count")]
        public long IntervalCount { get; set; }

        [JsonProperty("usage_type")]
        public string UsageType { get; set; }
    }

    public partial class PaymentSettings
    {
        [JsonProperty("payment_method_options")]
        public object PaymentMethodOptions { get; set; }

        [JsonProperty("payment_method_types")]
        public object PaymentMethodTypes { get; set; }
    }

    public partial class StatusTransitions
    {
        [JsonProperty("finalized_at")]
        public long FinalizedAt { get; set; }

        [JsonProperty("marked_uncollectible_at")]
        public object MarkedUncollectibleAt { get; set; }

        [JsonProperty("paid_at")]
        public long PaidAt { get; set; }

        [JsonProperty("voided_at")]
        public object VoidedAt { get; set; }
    }
}


