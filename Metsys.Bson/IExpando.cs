using System.Collections.Generic;

namespace Metsys.Bson
{
    public interface IExpando
    {
        IDictionary<string, object> Expando { get; }
    }
}