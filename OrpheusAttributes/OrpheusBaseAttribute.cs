using OrpheusInterfaces.Interfaces.Attributes;
using System;

namespace OrpheusAttributes
{
    /// <summary>
    /// Orpheus base attribute.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    /// <seealso cref="OrpheusInterfaces.Interfaces.Attributes.IOrpheusBaseAttribute" />
    public class OrpheusBaseAttribute :Attribute, IOrpheusBaseAttribute
    {
    }
}
