namespace Shared;

public static class Constants
{
    /// <summary>
    /// The name of the blob state.
    /// </summary>
    public const string BlobStateName = "blobstate";

    /// <summary>
    /// The name of the key/value state store.
    /// </summary>
    public const string KeyValueStateName = "kvstore";

    /// <summary>
    /// The name of the pub/sub connection.
    /// </summary>
    public const string PubSubName = "pubsub";



    /// <summary>
    /// The name of the pub/sub topic that stores the invoice data prior to being
    /// built into a file.
    /// </summary>
    public const string QueueBuildJobsTopicName = "buildqueue";
    
    /// <summary>
    /// The name of the pub/sub topic that stores the invoice numbers of the
    /// successfully built invoice files.
    /// </summary>
    public const string FileBuiltQueueTopicName = "invoicequeue";

    /// <summary>
    /// The name of the pub/sub subscription responsible for broadcasting email commands.
    /// </summary>
    public const string EmailCommandSubscriptionName = "email-command";

    /// <summary>
    /// The name of the topic used to broadcast commands to add a line item for a client.
    /// </summary>
    public const string AddLineItemTopicName = "add-line-item";

    /// <summary>
    /// The name of the dead-letter queue for the Add Line Item topic.
    /// </summary>
    public const string AddLineItemDeadletterTopicName = "add-line-item-deadletter";

    /// <summary>
    /// The name of the topic used to broadcast commands to generate an invoice.
    /// </summary>
    public const string GenerateInvoiceTopicName = "generate-invoice";

    /// <summary>
    /// The name of the dead-letter queue for the Generate Invoice topic.
    /// </summary>
    public const string GenerateInvoiceDeadletterTopicName = "generate-invoice-deadletter";
    
    /// <summary>
    /// The name of the SendGrid binding.
    /// </summary>
    public const string SendGridBindingName = "sendgrid";
    
    /// <summary>
    /// The name of the event used to notify the response to the manual invoice approval request.
    /// </summary>
    public const string InvoiceApprovalResponseEventName = "InvoiceApprovalResponse";

    /// <summary>
    /// Each of the environment variable names should be placed here and referenced by the solution.
    /// </summary>
    public static class EnvironmentVariableNames
    {
        //Note that each of these should be prefixed with "catalyst_" so they're properly picked up.
        public const string VariablePrefix = "catalyst_";

        /// <summary>
        /// Points to the administrator's email address.
        /// </summary>
        public const string AdminEmailAddress = "ADMIN_EMAIL_ADDRESS";

        /// <summary>
        /// The email address to send the email from.
        /// </summary>
        public const string FromEmailAddress = "FROM_EMAIL_ADDRESS";

        /// <summary>
        /// The email address of the "customer" that all customer-facing emails should be sent to.
        /// </summary>
        public const string CustomerEmailAddress = "CUSTOMER_EMAIL_ADDRESS";

        /// <summary>
        /// The validation code required to be present in emails for "authentication".
        /// </summary>
        public const string EmailValidationCode = "EMAIL_VALIDATION_CODE";

        /// <summary>
        /// The name to associate with the "From" email address.
        /// </summary>
        public const string FromEmailName = "FROM_EMAIL_NAME";

        /// <summary>
        /// The Dapr HTTP endpoint.
        /// </summary>
        public const string DaprHttpEndpoint = "DAPR_HTTP_ENDPOINT";

        /// <summary>
        /// The Dapr GRPC endpoint.
        /// </summary>
        public const string DaprGrpcEndpoint = "DAPR_GRPC_ENDPOINT";
        
        /// <summary>
        /// The Catalyst API token for the Builder API service.
        /// </summary>
        public const string DaprBuilderApiToken = "DAPR_BUILDER_API_TOKEN";

        /// <summary>
        /// The Catalyst API token for the Invoice API service.
        /// </summary>
        public const string DaprInvoiceApiToken = "DAPR_INVOICE_API_TOKEN";

        /// <summary>
        /// The Syncfusion license key.
        /// </summary>
        public const string SyncfusionLicenseKey = "SYNCFUSION_LICENSE_KEY";

        /// <summary>
        /// The domain used by the API service.
        /// </summary>
        public const string ApiBaseDomain = "API_BASE_DOMAIN";
    }
}