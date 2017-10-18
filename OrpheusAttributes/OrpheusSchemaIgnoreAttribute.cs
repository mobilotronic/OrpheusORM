using System;
using System.Collections.Generic;
using System.Text;

namespace OrpheusAttributes
{
    /// <summary>
    /// SchemaIgnore attribute. Decorate a model property to indicate
    /// that it's not part of the actual db table schema.
    /// Useful for dynamically calculated/defined model properties.
    /// </summary>
    public class SchemaIgnore :OrpheusBaseAttribute
    {
    }
}
