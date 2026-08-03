namespace OrpheusDemoApi.Contracts;

/*
 * Request/response shapes for the HTTP surface, kept separate from the persistence models so that
 * changing one does not silently change the other.
 */

public record CustomerRequest(string Code, string Name, string Email);

public record ProductRequest(string Code, string Description, double Price);

public record BulkProductResult(int Inserted, int BatchSize, long ElapsedMilliseconds);

public record SalesOrderLineRequest(Guid ProductId, double Quantity, double Price);

public record CreateSalesOrderRequest(Guid CustomerId, string OrderNumber, List<SalesOrderLineRequest> Lines);

public record SalesOrderLineResponse(
    Guid SalesOrderLineId,
    Guid SalesOrderId,
    Guid ProductId,
    double Quantity,
    double Price,
    double TotalPrice);

public record SalesOrderResponse(
    Guid SalesOrderId,
    Guid CustomerId,
    string OrderNumber,
    DateTime OrderDate,
    List<SalesOrderLineResponse> Lines);

public record SchemaResponse(string Description, double Version, IReadOnlyList<string> Tables);

/// <summary>
/// Projection target for the raw-SQL report. Orpheus materialises query results into any class whose
/// property names match the returned column names — no attributes and no corresponding table needed,
/// because a model used only to receive query results does not require a key.
/// </summary>
public class CustomerTotal
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public double TotalValue { get; set; }
}
