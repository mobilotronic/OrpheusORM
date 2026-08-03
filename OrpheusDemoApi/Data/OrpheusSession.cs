using OrpheusInterfaces.Core;

namespace OrpheusDemoApi.Data;

/// <summary>
/// A per-request handle on a connected <see cref="IOrpheusDatabase"/>.
/// </summary>
/// <remarks>
/// This is the one piece of glue a web application needs, and it is worth understanding.
/// <para>
/// <c>AddOrpheusPostgreSql</c> registers <see cref="IOrpheusDatabase"/> as <b>transient</b>, and a
/// freshly resolved instance is not connected — something has to call <c>ConnectAsync</c> before the
/// database can be used. Doing that inline in every endpoint would be noise, and caching a single
/// connected instance in a singleton would serialise every request onto one ADO.NET connection.
/// </para>
/// <para>
/// So this type is registered <b>scoped</b>: one per HTTP request. It resolves one transient
/// database, connects it lazily on first use, and lets the request scope dispose it at the end of
/// the request — <c>OrpheusDatabase.Dispose</c> closes and disposes the underlying connection, which
/// hands it back to the Npgsql connection pool rather than tearing down a real socket. Concurrent
/// requests therefore get concurrent pooled connections, which is exactly what you want.
/// </para>
/// <para>
/// Endpoints inject <c>OrpheusSession</c>, never <see cref="IOrpheusDatabase"/> directly.
/// </para>
/// </remarks>
public sealed class OrpheusSession(IOrpheusDatabase database)
{
    /// <summary>
    /// Returns the connected database, connecting on the first call within this request.
    /// </summary>
    public async Task<IOrpheusDatabase> ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (!database.Connected)
        {
            await database.ConnectAsync(cancellationToken: cancellationToken);
        }

        return database;
    }
}
