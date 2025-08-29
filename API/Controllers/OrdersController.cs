using System;
using API.DTOs;
using API.Extensions;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class OrdersController(ICartService cartService, IUnitOfWork unit) : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(CreateOrderDto orderDto)
    {
        var email = User.GetEmail();

        var cart = await cartService.GetCartAsync(orderDto.CartId);

        if (cart == null) return BadRequest("Problem with your cart");

        if (cart.PaymentIntentId == null) return BadRequest("Payment Intent Id is null");

        var items = new List<OrderItem>();

        foreach (var item in cart.Items)
        {
            var productItem = await unit.Repository<Core.Entities.Product>().GetByIdAsync(item.ProductId);

            if (productItem == null) return BadRequest($"Product with id {item.ProductId} not found");

            var itemOrdered = new ProductItemOrdered
            {
                ProductId = productItem.Id,
                ProductName = productItem.Name,
                PictureUrl = productItem.PictureUrl
            };

            var orderItem = new OrderItem
            {
                ItemOrdered = itemOrdered,
                Price = item.Price,
                Quantity = item.Quantity
            };

            items.Add(orderItem);
        }
        var deliveryMethod = await unit.Repository<DeliveryMethod>().GetByIdAsync(orderDto.DeliveryMethodId);

        if (deliveryMethod == null) return BadRequest("Invalid delivery method");

        var order = new Order
        {
            OrderItems = items,
            DeliveryMethod = deliveryMethod,
            ShippingAddress = orderDto.ShippingAddress,
            Subtotal = items.Sum(x => x.Price * x.Quantity),
            PaymentSummary = orderDto.PaymentSummary,
            PaymentIntentId = cart.PaymentIntentId,
            BuyerEmail = email
        };

        unit.Repository<Order>().Add(order);

        if (await unit.Complete())
        {
            return order;
        }

        return BadRequest("Problem creating order");
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Order>>> GetOrdersForUser()
    {
        var email = User.GetEmail();

        var spec = new OrderSpecification(email);

        var orders = await unit.Repository<Order>().ListAsync(spec);

        var ordersToReturn = orders.Select(order => order.ToDto()).ToList();

        return Ok(ordersToReturn);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
        var email = User.GetEmail();

        var spec = new OrderSpecification(id, email);

        var order = await unit.Repository<Order>().GetEntityWithSpec(spec);

        if (order == null) return NotFound();

        return Ok(order.ToDto());
    }

}
