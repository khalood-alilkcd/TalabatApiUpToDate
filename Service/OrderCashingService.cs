using Contracts;
using Entities.Models;
using Entities.Order_Aggregate;
using Service.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class OrderCashingService : IOrderCashingService
    {
        private readonly IBasketRepository _basketRepo;
        private readonly IUnitOfWork _unitOfWork;
       
        public OrderCashingService(
            IBasketRepository basketRepo,
            IUnitOfWork unitOfWork
        )
        {
            _basketRepo=basketRepo;
            _unitOfWork=unitOfWork;
        }
        public async Task<Order> CreateOrUpdateOrderCashing(string buyerEmail, string basketId, int deliveryMethodId, ShappingAddress shappingAddress)
        {
            // 1. Get Basket From Baskets Repo
            var basket = await _basketRepo.GetBasketAsync(basketId);
            // 2. Get Selected Items at Basket From Products Repo
            var orderItems = new List<OrderItem>();
            foreach (var item in basket.Items)
            {
                // - productItem id to get product by selecting the product
                var product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.Id);
                // and new intailizing the productItemOrder to set id and name and pictureurl own product onto productItemOrder
                var productItem = new ProductItemOrder(product.Id, product.Name, product.PictureUrl);
                // then create orderItem 
                var orderItem = new OrderItem(productItem, product.Price, item.Quantity);
                orderItems.Add(orderItem);
            }
            // 3. Calculate SubTotal
            var subTotal = orderItems.Sum(item => item.Price * item.Quantity);
            // 4. Get Delivery Method From DeliveryMethods Repo
            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(deliveryMethodId);
            /*// 5. check if order is exist by paymentId 
            var existingOrder = await _unitOfWork.Repository<Order>().FindById
                (o => o.BuyerEmail.Equals(basket.BuyerEmail), trackChanges: false);
            // 6. then delete the order and create another payment intent id by basket id  
            if (existingOrder != null)
            {
                _unitOfWork.Repository<Order>().Delete(existingOrder);
                await _paymentService.CreateOrUpdatePaymentIntent(basketId);
            }*/
            // 7. initailize new instance of Order to create
            var order = new Order(buyerEmail, shappingAddress, deliveryMethod, subTotal, orderItems);
            // 8.then Create Order 
            await _unitOfWork.Repository<Order>().CreateAsync(order);
            // 9. Save to Database [TODO]
            var result = await _unitOfWork.complete();
            if (result<=0)
            {
                return null;
            }
            return order;
        }
    }
}
