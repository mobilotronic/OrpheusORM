using OrpheusInterfaces.Interfaces.Attributes;

namespace OrpheusAttributes
{
    /// <summary>
    /// Unique key constraint attribute.
    /// Decorate a property with attribute to create a unique key constraint on a schema object.
    /// </summary>
    public class UniqueKey : OrpheusBaseAttribute, IUniqueKey
    { 
    }
}
