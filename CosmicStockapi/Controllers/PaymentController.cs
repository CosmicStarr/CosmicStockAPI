using System.Security.Claims;
using CosmicStockapi.ErrorHandling;
using Data.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Models;
using Stripe;

namespace CosmicStockapi.Controllers
{
    public class PaymentController:BaseController
    {
        private const string ActualStripeWebHook = "whsec_c7bbc6df23fb5f30cbfac69438fe21bec77690c27386ac37d01e33d320781168"; 
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentService paymentService,ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpPost("{Id}")]
        public async Task<ActionResult<ShoppingCart>> CreateOrUpdatePaymentIntent(string Id)
        {
            var user = HttpContext.User.FindFirstValue(ClaimTypes.Email);
            var PaymentInfo = await _paymentService.CreateOrUpdatePayment(Id,user);
            if(PaymentInfo == null) return BadRequest(new ApiErrorResponse(400,"There has been a problem retrieving your shopping cart!"));
            return Ok(PaymentInfo);
        }

        [HttpPost("Webhook")]
        public async Task<ActionResult> StripeWebHook()
        {
            var jsonData = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeEventInfo = EventUtility.ConstructEvent(jsonData,Request.Headers["Stripe-Signature"],ActualStripeWebHook);
            PaymentIntent intent;
            ActualOrder order;

            switch (stripeEventInfo.Type)
            {
                case "payment_intent.succeeded": intent = (PaymentIntent)stripeEventInfo.Data.Object;
                    _logger.LogInformation("Payment was successful!:", intent.Idff);
                    order = await _paymentService.UpdateOrderStatusPaymentSuccessful(intent.Id);
                      _logger.LogInformation("Order Status updated! It was a success!:", order.Id);
                    break;
                case "payment_intent.payment_failed": intent = (PaymentIntent)stripeEventInfo.Data.Object;
                    _logger.LogInformation("Payment failed:", intent.Id);
                      order = await _paymentService.UpdateOrderStatusPaymentFailed(intent.Id);
                       _logger.LogInformation("Order Status updated! Failed!:", order.Id);
                    break;
            }

            return new EmptyResult();
        }
    }
}