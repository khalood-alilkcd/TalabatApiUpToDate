using Contracts;
using Entities.Models;
using Entities.Order_Aggregate;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repository;
using Service.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class OrderService : IOrderService
    {
        private readonly IBasketRepository _basketRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStripeAppService _paymentService;

        public OrderService(
            IBasketRepository basketRepo,
            IUnitOfWork unitOfWork,
            IStripeAppService paymentService
        )
        {
            _basketRepo=basketRepo;
            _unitOfWork=unitOfWork;
            _paymentService=paymentService;
        }

        public async Task<Order> CreateOrderAsync
            (string buyerEmail, string basketId, int deliveryMethodId, ShappingAddress shappingAddress)
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
            // 5. check if order is ecsist by paymentId 
            var existingOrder = await _unitOfWork.Repository<Order>().FindById
                (o => o.PaymentIntentId.Equals(basket.PaymentIntentId), trackChanges:false);
            // 6. then delete the order and create another payment intent id by basket id  
            if (existingOrder != null)
            {
                _unitOfWork.Repository<Order>().Delete(existingOrder);
                await _paymentService.CreateOrUpdatePaymentIntent(basketId);
            }
            // 7. initailize new instance of Order to create
            var order = new Order(buyerEmail, shappingAddress, deliveryMethod, subTotal, orderItems, basket.PaymentIntentId);
            // 8.then Create Order 
            await _unitOfWork.Repository<Order>().CreateAsync(order);
            // 9. Save to Database [TODO]
            var result = await _unitOfWork.complete();
            if(result<=0)
            {
                return null;
            }
            return order;
        }

        public async Task<IReadOnlyList<DeliveryMethod>> GetDeliveryMethodsAsync()
        {
            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetAllAsync();
            return deliveryMethod;
        }

        public async Task<Order> GetOrderByIdForUserAsync(int orderId, string buyerEmail, bool trackChanges)
        {
            var order = await _unitOfWork.Repository<Order>().FindByCondition(o => o.BuyerEmail.Equals(buyerEmail) &&  o.Id.Equals(orderId), trackChanges, new List<Expression<Func<Order, object>>> { o => o.DeliveryMethod, o => o.Items }).SingleOrDefaultAsync();
            return order;
        }

        

        public async Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string buyerEmail, bool trackChanges)
        {
            var orders = await _unitOfWork.Repository<Order>().FindByCondition(o => o.BuyerEmail.Equals(buyerEmail),trackChanges, new List<Expression<Func<Order, object>>> { o => o.DeliveryMethod ,o => o.Items }).ToListAsync();
            return orders;
        }

        /*public async Task<Order> UpdatePaymentIntentToSucceededOrFailed(string paymentIntentId, bool IsSucceed)
        {
            var order = await _unitOfWork.Repository<Order>().FindById(o => o.PaymentIntentId == paymentIntentId, IsSucceed);
            if (IsSucceed)
                order.OrderStatus = OrderStatus.PaymentRecevied;
            else
                order.OrderStatus = OrderStatus.PeymentFailed;
            _unitOfWork.Repository<Order>().Update(order);
            await _unitOfWork.complete();
            return order;
        }*/
    }
}
