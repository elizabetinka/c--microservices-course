using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using System.Transactions;
using Task3.Models.ForDatabase;
using Task3.Models.ForServices;
using Task3.Models.Payloads;
using Task3.Repository.Interfaces;
using Task3.Services.Interfaces;

namespace Task3.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IHistoryOrderRepository _historyOrderRepository;
    private readonly IOrderItemsRepository _orderItemsRepository;
    private readonly int _pageSize;

    public OrderService(IOrderRepository orderRepository, IHistoryOrderRepository historyOrderRepository, IOrderItemsRepository orderItemsRepository, IOptions<PostgresOptions> options)
    {
        _orderRepository = orderRepository;
        _historyOrderRepository = historyOrderRepository;
        _orderItemsRepository = orderItemsRepository;
        _pageSize = options.Value.PageSize;
    }

    public async Task AddHistoryItemAsync(PayloadBaseModel payload, CancellationToken cancellationToken)
    {
        var historyItem = new HistoryItem(payload.OrderId, DateTime.Now, HistoryType.Created, payload);
        await _historyOrderRepository.AddAsync(
           historyItem, cancellationToken);
    }

    public async Task<Order> CreateAsync(CreateOrder createdOrder, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        Order order = await _orderRepository.AddAsync(
            new Order(OrderStatus.OrderCreated, createdOrder.CreatedAt, createdOrder.CreatedBy), cancellationToken);

        await _historyOrderRepository.AddAsync(new HistoryItem(order.Id, DateTime.Now, HistoryType.Created, new CreatedOrder(order.Id, order.CreatedAt, order.CreatedBy)), cancellationToken);

        transaction.Complete();
        return order;
    }

    public async IAsyncEnumerable<Order> CreateAsync(IEnumerable<CreateOrder> createdOrder, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        IList<CreateOrder> createdOrderList = createdOrder.ToList();

        IAsyncEnumerable<Order> orders = _orderRepository.AddAsync(
            createdOrderList.Select(x => new Order(OrderStatus.OrderCreated, x.CreatedAt, x.CreatedBy)), cancellationToken);

        await foreach (Order order in orders)
        {
            await _historyOrderRepository.AddAsync(
                new HistoryItem(order.Id, DateTime.Now, HistoryType.Created, new CreatedOrder(order.Id, order.CreatedAt, order.CreatedBy)), cancellationToken);

            yield return order;
        }

        transaction.Complete();
    }

    public async Task<OrderItem?> AddProductAsync(AddItem item, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        Order? order = await _orderRepository.FindByIdAsync(item.OrderId, cancellationToken);

        if (order is not { State: OrderStatus.OrderCreated })
        {
            return null;
        }

        OrderItem added =
            await AddProductAsyncWithSameOrderId([item], cancellationToken).SingleAsync(cancellationToken);

        transaction.Complete();

        return added;
    }

    public async IAsyncEnumerable<OrderItem?> AddProductAsync(IEnumerable<AddItem> items, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        IList<AddItem> itemsList = items.ToList();

        IAsyncEnumerable<Order> orders = _orderRepository.FindByFiltersAsync(
            itemsList.Select(x => x.OrderId).ToArray(), [], [], 0, itemsList.Count, cancellationToken);

        await foreach (Order order in orders)
        {
            if (order is not { State: OrderStatus.OrderCreated })
            {
                yield return null;
            }

            IList<AddItem> products = itemsList.Where(x => x.OrderId == order.Id).ToList();

            IAsyncEnumerable<OrderItem> added = AddProductAsyncWithSameOrderId(products, cancellationToken);

            await foreach (OrderItem orderItem in added) yield return orderItem;
        }

        transaction.Complete();
    }

    public async Task DeleteProductAsync(RemoveItem product, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        Order? order = await _orderRepository.FindByIdAsync(product.OrderId, cancellationToken);

        if (order is not { State: OrderStatus.OrderCreated })
        {
            return;
        }

        await DeleteProductAsyncWithSameOrderId(order.Id, [product], cancellationToken);

        transaction.Complete();
    }

    public async Task DeleteProductAsync(IEnumerable<RemoveItem> products, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        var productsList = products.ToList();

        IAsyncEnumerable<Order> orders = _orderRepository.FindByFiltersAsync(
            productsList.Select(x => x.OrderId).ToArray(), [OrderStatus.OrderCreated], [], 0, productsList.Count, cancellationToken);

        await foreach (Order order in orders)
        {
            IEnumerable<RemoveItem> removeItems = productsList.Where(x => x.OrderId == order.Id);

            await DeleteProductAsyncWithSameOrderId(order.Id, removeItems, cancellationToken);
        }

        transaction.Complete();
    }

    public async Task ToProcessingStatusAsync(long orderId, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        Order? order = await _orderRepository.FindByIdAsync(orderId, cancellationToken);
        if (order == null)
        {
            return;
        }

        await _orderRepository.ChangeStatusByIdAsync(orderId, OrderStatus.InProcessing, cancellationToken);

        await _historyOrderRepository.AddAsync(
            new HistoryItem(orderId, DateTime.Now, HistoryType.StateChanged, new StateChanged(orderId, OrderStatus.InProcessing)), cancellationToken);

        transaction.Complete();
    }

    public async Task ToProcessingStatusAsync(long[] orderId, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        IEnumerable<Order> orders = await _orderRepository.FindByFiltersAsync(orderId, [], [], 0, orderId.Length, cancellationToken).ToListAsync(cancellationToken);

        await _orderRepository.ChangeStatusByIdAsync(orders.Select(x => x.Id).ToArray(), OrderStatus.InProcessing, cancellationToken);

        await _historyOrderRepository.AddAsync(
            orders.Select(x => new HistoryItem(x.Id, DateTime.Now, HistoryType.StateChanged, new StateChanged(x.Id, OrderStatus.InProcessing))), cancellationToken).ToListAsync(cancellationToken);

        transaction.Complete();
    }

    public async Task ToCreatedStatusAsync(long[] orderId, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        IEnumerable<Order> orders = await _orderRepository.FindByFiltersAsync(orderId, [OrderStatus.InProcessing], [], 0, orderId.Length, cancellationToken).ToListAsync(cancellationToken);

        await _orderRepository.ChangeStatusByIdAsync(orders.Select(x => x.Id).ToArray(), OrderStatus.OrderCreated, cancellationToken);

        await _historyOrderRepository.AddAsync(
            orders.Select(x => new HistoryItem(x.Id, DateTime.Now, HistoryType.StateChanged, new StateChanged(x.Id, OrderStatus.OrderCreated))), cancellationToken).ToListAsync(cancellationToken);

        transaction.Complete();
    }

    public async Task ToCompletedAsync(long orderId, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        Order? order = await _orderRepository.FindByIdAsync(orderId, cancellationToken);

        if (order == null)
        {
            return;
        }

        await _orderRepository.ChangeStatusByIdAsync(orderId, OrderStatus.OrderCompleted, cancellationToken);

        await _historyOrderRepository.AddAsync(
            new HistoryItem(orderId, DateTime.Now, HistoryType.StateChanged, new StateChanged(orderId, OrderStatus.OrderCompleted)), cancellationToken);

        transaction.Complete();
    }

    public async Task ToCompletedAsync(long[] orderId, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        IEnumerable<Order> orders = await _orderRepository.FindByFiltersAsync(
            orderId, [], [], 0, orderId.Length, cancellationToken).ToListAsync(cancellationToken);

        await _orderRepository.ChangeStatusByIdAsync(
            orders.Select(x => x.Id).ToArray(), OrderStatus.OrderCompleted, cancellationToken);

        await _historyOrderRepository.AddAsync(
            orders.Select(x => new HistoryItem(x.Id, DateTime.Now, HistoryType.StateChanged, new StateChanged(x.Id, OrderStatus.OrderCompleted))), cancellationToken).ToListAsync(cancellationToken);

        transaction.Complete();
    }

    public async Task ToCancelledAsync(long orderId, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        Order? order = await _orderRepository.FindByIdAsync(orderId, cancellationToken);

        if (order == null)
        {
            return;
        }

        if (order.State != OrderStatus.OrderCreated)
        {
            return;
        }

        await _orderRepository.ChangeStatusByIdAsync(orderId, OrderStatus.OrderCancelled, cancellationToken);

        await _historyOrderRepository.AddAsync(
            new HistoryItem(orderId, DateTime.Now, HistoryType.StateChanged, new StateChanged(orderId, OrderStatus.OrderCancelled)), cancellationToken);

        transaction.Complete();
    }

    public async Task ToCancelledAsync(long[] orderId, CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        IEnumerable<Order> orders = await _orderRepository.FindByFiltersAsync(
            orderId, [], [], 0, orderId.Length, cancellationToken).ToListAsync(cancellationToken);

        orders = orders.Where(x => x.State == OrderStatus.OrderCreated).ToList();
        await _orderRepository.ChangeStatusByIdAsync(
            orders.Select(x => x.Id).ToArray(), OrderStatus.OrderCancelled, cancellationToken);

        await _historyOrderRepository.AddAsync(
            orders.Select(x => new HistoryItem(x.Id, DateTime.Now, HistoryType.StateChanged, new StateChanged(x.Id, OrderStatus.OrderCancelled))), cancellationToken).ToListAsync(cancellationToken);

        transaction.Complete();
    }

    public async IAsyncEnumerable<HistoryItem> FindInHistoryAsyncInfinity(
        long id,
        HistoryType[] types,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        int cursor = 0;
        while (true)
        {
            IAsyncEnumerable<HistoryItem> ans = _historyOrderRepository.FindByFiltersAsync(id, types, cursor, _pageSize, cancellationToken);
            await foreach (HistoryItem item in ans)
            {
                yield return item;
            }

            cursor += _pageSize;
        }
    }

    public async IAsyncEnumerable<HistoryItem> FindInHistoryAsync(
        long id,
        HistoryType[] types,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        IAsyncEnumerable<HistoryItem> ans = _historyOrderRepository.FindByFiltersAsync(id, types, 0, int.Max(types.Length, _pageSize), cancellationToken);
        await foreach (HistoryItem item in ans)
        {
            yield return item;
        }
    }

    private async IAsyncEnumerable<OrderItem> AddProductAsyncWithSameOrderId(IEnumerable<AddItem> products, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        IList<AddItem> productsList = products.ToList();

        IAsyncEnumerable<OrderItem> orderItems = _orderItemsRepository.AddAsync(
            productsList.Select(x => new OrderItem(x.OrderId, x.ProductId, x.Quantity, false)), cancellationToken);

        await foreach (OrderItem item in orderItems)
        {
            await _historyOrderRepository.AddAsync(
                new HistoryItem(item.OrderId, DateTime.Now, HistoryType.Created, new ItemAdded(item.OrderId, item.ProductId, item.Quantity)), cancellationToken);

            yield return item;
        }
    }

    private async Task DeleteProductAsyncWithSameOrderId(long orderId, IEnumerable<RemoveItem> products, CancellationToken cancellationToken)
    {
        IList<RemoveItem> removeItems = products.ToList();

        IAsyncEnumerable<OrderItem> items = _orderItemsRepository.FindByFiltersAsync(
            removeItems.Select(x => x.ProductId).ToArray(), [orderId], false, 0, removeItems.Count,  0, cancellationToken);

        // delete if quantity >= quantity in database
        List<OrderItem> forDelete = await items.Where(item =>
        {
            long diff = item.Quantity -
                        removeItems.First(x => x.ProductId == item.ProductId).Quantity;
            return diff <= 0;
        }).ToListAsync(cancellationToken);

        await _orderItemsRepository.SafeDeleteAsync(forDelete.Select(x => x.ProductId).ToArray(), orderId, cancellationToken);

        // update if quantity < quantity in database
        await foreach (OrderItem item in items)
        {
            int diff = item.Quantity -
                       removeItems.First(x => x.ProductId == item.ProductId).Quantity;
            if (diff > 0)
            {
                await _orderItemsRepository.UpdateAsync(item.ProductId, diff, cancellationToken);
            }
        }

        await _historyOrderRepository.AddAsync(removeItems.Select(x => new HistoryItem(orderId, DateTime.Now, HistoryType.ItemRemoved, new ItemRemoved(orderId, x.ProductId, x.Quantity))).ToList(), cancellationToken).ToListAsync(cancellationToken);
    }
}