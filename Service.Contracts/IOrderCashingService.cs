using Entities.Order_Aggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Contracts
{
    public interface IOrderCashingService
    {
        Task<Order> CreateOrUpdateOrderCashing(string buyerEmail, string basketId, int deliveryMethodId, ShappingAddress shappingAddress);
    }
}
