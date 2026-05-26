// ============================================================================
// 📝 C# 14 Field Keyword Examples - Refactoring eShop Order.cs
// ============================================================================

namespace eShop.Ordering.Domain.Examples;

// ============================================================================
// EXAMPLE 1: Basic Field Keyword Usage
// ============================================================================

/// <summary>
/// Demonstrates the basic field keyword for automatic backing field generation
/// </summary>
public class SimplePropertyExample
{
    // ❌ Traditional backing field (old way)
    private string _name;
    public string NameOld
    {
        get => _name;
        set => _name = value;
    }

    // ✅ C# 14: Field keyword (new way)
    public string Name
    {
        get => field;
        set => field = value;
    }

    // ✅ C# 14: Field keyword with validation
    private decimal _price;
    public decimal Price
    {
        get => field;
        set => field = value >= 0 ? value : throw new ArgumentException("Price must be positive");
    }
}

// ============================================================================
// EXAMPLE 2: Refactored Order.cs with Field Keyword
// ============================================================================

/// <summary>
/// Refactored Order class using C# 14 field keyword
/// Compared to original eShop Order.cs
/// </summary>
public class OrderRefactored
{
    // ✅ Field keyword with lazy initialization
    private List<INotification> _domainEvents;
    public IReadOnlyCollection<INotification> DomainEvents
    {
        get => (field ??= new()).AsReadOnly();
    }

    public void AddDomainEvent(INotification eventItem)
    {
        field ??= new List<INotification>();
        field.Add(eventItem);
    }

    // ✅ Field keyword with validation
    private Address _address;
    public Address Address
    {
        get => field;
        private set => field = value ?? throw new ArgumentNullException(nameof(value), "Address cannot be null");
    }

    // ✅ Field keyword for OrderDate with internal setter
    public DateTime OrderDate
    {
        get => field;
        private set => field = value;
    }

    // ✅ Field keyword with description
    private string _description;
    public string Description
    {
        get => field;
        private set => field = value;
    }

    // ✅ Field keyword for status changes
    private OrderStatus _status;
    public OrderStatus OrderStatus
    {
        get => field;
        private set
        {
            var oldStatus = field;
            field = value;
            // Can log state changes here
            Console.WriteLine($"Order status changed from {oldStatus} to {value}");
        }
    }

    // ✅ Backing field with collection view
    private readonly List<OrderItem> _orderItems = new();
    public IReadOnlyCollection<OrderItem> OrderItems => field.AsReadOnly();

    public void AddOrderItem(OrderItem item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        field.Add(item);
    }
}

// ============================================================================
// EXAMPLE 3: Change Notification Pattern with Field Keyword
// ============================================================================

/// <summary>
/// Demonstrates INotifyPropertyChanged with field keyword
/// </summary>
public class OrderWithNotification : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    // ✅ Field keyword with change notification
    private string _productName;
    public string ProductName
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged(nameof(ProductName));
            }
        }
    }

    // ✅ Field keyword with derived property
    private decimal _unitPrice;
    private int _quantity;

    public decimal UnitPrice
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged(nameof(UnitPrice));
                OnPropertyChanged(nameof(TotalPrice));
            }
        }
    }

    public int Quantity
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(TotalPrice));
            }
        }
    }

    // Computed property using other field keywords
    public decimal TotalPrice => UnitPrice * Quantity;

    protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

// ============================================================================
// EXAMPLE 4: Lazy Initialization with Field Keyword
// ============================================================================

/// <summary>
/// Demonstrates lazy initialization using field keyword
/// </summary>
public class OrderServiceWithLazyInit
{
    private IEnumerable<Order> _cachedOrders;
    public IEnumerable<Order> Orders
    {
        get => field ??= LoadOrdersFromDatabase();
    }

    private List<OrderHistory> _history;
    public IReadOnlyCollection<OrderHistory> History
    {
        get => (field ??= new()).AsReadOnly();
    }

    public void AddHistoryEntry(OrderHistory entry)
    {
        field ??= new List<OrderHistory>();
        field.Add(entry);
    }

    private IEnumerable<Order> LoadOrdersFromDatabase()
    {
        Console.WriteLine("Loading orders from database...");
        // Simulate database call
        return new List<Order>();
    }
}

// ============================================================================
// EXAMPLE 5: Advanced Pattern - Computed Properties with Caching
// ============================================================================

/// <summary>
/// Advanced pattern combining field keyword with caching
/// </summary>
public class OrderWithCachedTotal
{
    private decimal? _cachedTotal;
    private readonly List<OrderItem> _items = new();

    public decimal Total
    {
        get
        {
            // Cache the computed total
            field ??= _items.Sum(item => item.GetSubtotal());
            return field.Value;
        }
    }

    public void AddItem(OrderItem item)
    {
        _items.Add(item);
        // Invalidate cache
        _cachedTotal = null;
    }

    public void InvalidateCache() => _cachedTotal = null;
}

// ============================================================================
// SUPPORTING TYPES
// ============================================================================

public record Address(string Street, string City, string Country, string ZipCode);

public record OrderItem(int ProductId, string ProductName, decimal UnitPrice, int Quantity)
{
    public decimal GetSubtotal() => UnitPrice * Quantity;
}

public enum OrderStatus { Pending, Processing, Shipped, Delivered, Cancelled }

public interface INotification { }

public record OrderNotification(string Message) : INotification;

public record OrderHistory(DateTime Timestamp, string Action, string Details);

// ============================================================================
// USAGE EXAMPLES
// ============================================================================

public class FieldKeywordUsageExamples
{
    public static void Main()
    {
        // Example 1: Simple property
        var example1 = new SimplePropertyExample();
        example1.Name = "John Doe";
        example1.Price = 100m;
        Console.WriteLine($"Name: {example1.Name}, Price: {example1.Price}");

        // Example 2: Refactored Order
        var order = new OrderRefactored
        {
            Address = new Address("123 Main St", "NYC", "USA", "10001")
        };
        order.OrderDate = DateTime.UtcNow;
        order.AddDomainEvent(new OrderNotification("Order created"));

        // Example 3: Order with notification
        var notifyOrder = new OrderWithNotification();
        notifyOrder.PropertyChanged += (s, e) =>
            Console.WriteLine($"Property changed: {e.PropertyName}");

        notifyOrder.ProductName = "Laptop";
        notifyOrder.UnitPrice = 999.99m;
        notifyOrder.Quantity = 2;

        // Example 4: Lazy loading
        var service = new OrderServiceWithLazyInit();
        var orders = service.Orders; // Loads on first access
        var history = service.History; // Initializes empty list

        // Example 5: Cached properties
        var cachedOrder = new OrderWithCachedTotal();
        cachedOrder.AddItem(new OrderItem(1, "Mouse", 25m, 2));
        cachedOrder.AddItem(new OrderItem(2, "Keyboard", 75m, 1));
        Console.WriteLine($"Total: {cachedOrder.Total}");
    }
}

