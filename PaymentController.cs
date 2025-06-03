using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Configuration;

using Microsoft.Extensions.Logging;

using Stripe;

using Stripe.Checkout;

using System;

using System.IO;

using System.Threading.Tasks;



#nullable enable



// DTOs

public class PaymentRequest

{

    public required string PaymentMethodId { get; set; }

    public long Amount { get; set; }

}



public class CheckoutViewModel

{

    public required string StripePublishableKey { get; set; }

}



public static class StripeEvents

{

    public const string PaymentIntentSucceeded = "payment_intent.succeeded";

    public const string PaymentIntentPaymentFailed = "payment_intent.payment_failed";

    public const string ChargeRefunded = "charge.refunded";

    public const string CheckoutSessionCompleted = "checkout.session.completed";

}



public class PaymentController : Controller

{

    private readonly IConfiguration _configuration;

    private readonly ILogger<PaymentController> _logger;



    public PaymentController(IConfiguration configuration, ILogger<PaymentController> logger)

    {

        _configuration = configuration;

        _logger = logger;

    }



    public IActionResult Checkout()

    {

        var model = new CheckoutViewModel

        {

            StripePublishableKey = _configuration["Stripe:PublishableKey"]

                     ?? throw new InvalidOperationException("Stripe Publishable Key is missing.")

        };

        return View(model);

    }



    [HttpPost]

    public async Task<IActionResult> CreatePaymentIntent([FromBody] PaymentRequest request)

    {

        if (!ModelState.IsValid)

        {

            _logger.LogWarning("Invalid PaymentRequest received.");

            foreach (var modelStateEntry in ModelState.Values)

            {

                foreach (var error in modelStateEntry.Errors)

                {

                    _logger.LogError("Model error: {ErrorMessage}", error.ErrorMessage ?? "null");

                }

            }

            return BadRequest(ModelState);

        }



        var service = new PaymentIntentService();



        try

        {

            var options = new PaymentIntentCreateOptions

            {

                Amount = request.Amount,

                Currency = "usd",

                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions

                {

                    Enabled = true

                }

            };



            var paymentIntent = await service.CreateAsync(options);



            switch (paymentIntent.Status)

            {

                case "requires_action" when paymentIntent.NextAction?.Type == "use_stripe_sdk":

                    _logger.LogInformation("PaymentIntent {Id} requires action (e.g., 3D Secure).", paymentIntent.Id ?? "null");

                    return Json(new { requiresAction = true, clientSecret = paymentIntent.ClientSecret });



                case "succeeded":

                    _logger.LogInformation("PaymentIntent {Id} succeeded.", paymentIntent.Id ?? "null");

                    return Json(new { success = true, clientSecret = paymentIntent.ClientSecret });



                case "requires_confirmation":

                    _logger.LogInformation("PaymentIntent {Id} requires confirmation. Returning client_secret for client-side confirmation.", paymentIntent.Id ?? "null");

                    return Json(new { requiresAction = true, clientSecret = paymentIntent.ClientSecret });



                default:

                    _logger.LogWarning("Unhandled PaymentIntent status: {Status}.", paymentIntent.Status ?? "null");

                    return Json(new { success = false, message = $"Unhandled status: {paymentIntent.Status}" });

            }

        }

        catch (StripeException e)

        {

            _logger.LogError(e, "Stripe error creating PaymentIntent: {Message}", e.Message ?? "null");

            return Json(new { success = false, message = e.Message });

        }

        catch (Exception e)

        {

            _logger.LogError(e, "Unexpected error creating PaymentIntent: {Message}", e.Message ?? "null");

            return Json(new { success = false, message = "An unexpected error occurred." });

        }

    }



    public IActionResult Success()

    {

        return View();

    }



    [HttpPost]
    [Route("api/stripe/webhook")]
    public async Task<IActionResult> StripeWebhook()
    {
        // Ensure we can read the request body multiple times
        Request.EnableBuffering();

        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        HttpContext.Request.Body.Position = 0; // Reset for potential future reads

        var stripeSignature = Request.Headers["Stripe-Signature"];
        var webhookSecret = _configuration["Stripe:WebhookSecret"];

        if (string.IsNullOrEmpty(webhookSecret))
        {
            _logger.LogError("Stripe Webhook Secret is not configured.");
            return StatusCode(500, "Webhook secret not configured.");
        }

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                stripeSignature,
                webhookSecret
            );

            _logger.LogInformation("✅ Stripe event parsed successfully: {Type}", stripeEvent.Type ?? "null");

            // Process the event
            switch (stripeEvent.Type)
            {
                case StripeEvents.PaymentIntentSucceeded:
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    _logger.LogInformation("Payment succeeded: {PaymentIntentId}", paymentIntent?.Id);
                    break;

                // Add other event types as needed
                default:
                    _logger.LogInformation("Unhandled event type: {EventType}", stripeEvent.Type);
                    break;
            }

            return Ok();
        }
        catch (StripeException e)
        {
            _logger.LogError(e, "❌ Stripe webhook signature verification failed");
            return BadRequest();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "❌ Unexpected error processing webhook");
            return StatusCode(500);
        }
    }
}