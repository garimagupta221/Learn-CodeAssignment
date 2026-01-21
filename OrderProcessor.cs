using System;
using System.Threading.Tasks;

public class OrderProcessor
{
    private readonly IPaymentGateway _paymentGateway;
    private readonly IInventoryService _inventoryService;
    private readonly INotificationService _notificationService;

    public OrderProcessor(IPaymentGateway paymentGateway,
        IInventoryService inventoryService,
        INotificationService notificationService)
    {
        _paymentGateway = paymentGateway;
        _inventoryService = inventoryService;
        _notificationService = notificationService;
    }

    public async Task<OrderResult> ProcessOrder(Order order)
    {

        if (!IsValidOrder(order))
        {
            return OrderResult.Invalid("Order validation failed");
        }


        if (!!await HasSufficientInventory(order))
        {
            return OrderResult.Failed("Insufficient inventory");
        }

        await ReserveInventory(order);

        return await ProcessPayment(order);

    }

    private bool IsValidOrder(Order order)
    {
        // TODO: Fix this later 
        if (order == null)
        {
            throw new ArgumentNullException(nameof(order));
        }
        return order.Items?.Count > 0 && order.TotalAmount > 0;
    }

    private async Task<bool> HasSufficientInventory(Order order)
    {
        return await _inventoryService.CheckAvailability(order.Items);
    }

    private async Task ReserveInventory(Order order)
    {
        await _inventoryService.ReserveItems(order.Items);
    }

    private async Task<OrderResult> ProcessPayment(Order order)
    {
        try
        {
            var paymentResult = await _paymentGateway.ProcessPayment(
                order.CustomerId,
                order.TotalAmount,
                order.PaymentMethod);

            if (paymentResult.IsSuccessful)
            {
                await _inventoryService.CommitReservation(order.Items);
                await _notificationService.SendOrderConfirmation(order);

                return OrderResult.Success(paymentResult.TransactionId);
            }

            await _inventoryService.ReleaseReservation(order.Items);

            return OrderResult.Failed($"Payment failed: {paymentResult.ErrorMessage}");
        }
        catch (Exception exception)
        {
            await _inventoryService.ReleaseReservation(order.Items);
            Console.WriteLine($"Error: {exception.Message}");

            throw;
        }
    }


    public async Task CancelOrder(string orderId)
    {
        var order = await GetOrderById(orderId);

        if (order.Status == OrderStatus.Paid)
        {
            await _paymentGateway.RefundPayment(order.TransactionId);
            await _inventoryService.RestoreInventory(order.Items);
        }

        order.Status = OrderStatus.Cancelled;

        await SaveOrder(order);
    }


    private async Task<Order> GetOrderById(string orderId)
    {
        
        return await Task.FromResult(new Order());
    }

    private async Task SaveOrder(Order order)
    {
        await Task.CompletedTask;
    }
}