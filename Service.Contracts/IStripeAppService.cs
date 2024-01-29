
using Entities.Models;
using Entities.Order_Aggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Contracts
{
    public interface IStripeAppService
    {
        Task<CustomerBasket> CreateOrUpdatePaymentIntent(string basketId);
        Task<Order> UpdateOrderPaymentFailed(string paymentIntentId);
        Task<Order> UpdateOrderPaymentSucceeded(string paymentIntentId);
    }
}
