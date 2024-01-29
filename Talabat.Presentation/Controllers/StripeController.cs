using Contracts;
using Entities.ErrorModel;
using Entities.Order_Aggregate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Service.Contracts;
using Shared.DataTransfierObject;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Presentation.Controllers
{
    [Route("api/payments")]
    [ApiController]
    [Authorize]
    public class StripeController : ControllerBase
    {
        private readonly IStripeAppService _stripeService;
        public readonly ILoggerManager _logger; 
        private const string WhSecret = "whsec_84172b4171119a95b918bcda6c5f0be112a9dfa0c759284b0d784c3c7dad0fc9";
        public StripeController(IStripeAppService stripeService, ILoggerManager logger)
        {
            _stripeService = stripeService;
            _logger=logger;
        }

        [HttpPost("{basketId}")]
        public async Task<ActionResult<CustomerBasketDto>> CreateOrUpdatePaymentIntent(string basketId)
        {
            var basket = await _stripeService.CreateOrUpdatePaymentIntent(basketId);
            if (basket == null) return BadRequest(new ApiResponse(400, "problem with your basket"));
            return Ok(basket);
        }


        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeEvent = EventUtility.ConstructEvent(json,
                Request.Headers["Stripe-Signature"], WhSecret);

            PaymentIntent intent;
            Order order;
            
            try
            {
                switch (stripeEvent.Type)
                {
                    case "payment_intent.payment_failed":
                        intent = (PaymentIntent)stripeEvent.Data.Object;
                        _logger.LogInfo("Payment Failed " +  intent.Id);
                        order = await _stripeService.UpdateOrderPaymentFailed(intent.Id);
                        _logger.LogInfo("Payment Failed " +  order.Id);
                        break;  
                    case "payment_intent.succeeded":
                        intent = (PaymentIntent)stripeEvent.Data.Object;
                        _logger.LogInfo("Payment Succeeded    " +  intent.Id);
                        order = await _stripeService.UpdateOrderPaymentSucceeded(intent.Id);
                        _logger.LogInfo("Order Updated to payment received: " +  order.Id);
                        break;
                }
                
            }
            catch (StripeException e)
            {
                _logger.LogError($"Something went wrong inside GetAllOwners action:{e}, {e.Message}");
            }
            return new EmptyResult();
        }

    }
}
