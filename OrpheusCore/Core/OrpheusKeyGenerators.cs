using OrpheusInterfaces.Core;
using System;

namespace OrpheusCore
{
    /// <summary>
    /// The default <see cref="IOrpheusKeyGenerator"/>. Produces RFC 9562 UUIDv7 values, which carry a
    /// 48-bit millisecond timestamp in their leading bytes and so sort in creation order.
    /// </summary>
    /// <remarks>
    /// See <see cref="IOrpheusKeyGenerator"/> for the SQL Server sort-order caveat.
    /// </remarks>
    public sealed class SequentialGuidKeyGenerator : IOrpheusKeyGenerator
    {
        /// <inheritdoc/>
        public Guid NewKey() => Guid.CreateVersion7();
    }

    /// <summary>
    /// Generates random version 4 <see cref="Guid"/> values — the behaviour Orpheus had before the
    /// key generator became pluggable. Register it with
    /// <c>AddOrpheusKeyGenerator&lt;RandomGuidKeyGenerator&gt;()</c> to opt back in.
    /// </summary>
    public sealed class RandomGuidKeyGenerator : IOrpheusKeyGenerator
    {
        /// <inheritdoc/>
        public Guid NewKey() => Guid.NewGuid();
    }
}
