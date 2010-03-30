using System.Collections.Generic;
    
namespace Metsys.Bson
{    
    internal class CollectionWrapper<T> : ListWrapper
    {
        private ICollection<T> _list;
        
        protected override void SetContainer(object container)
        {
            _list = (ICollection<T>)container;
        }
        public override object Collection
        {
            get { return _list; }
        }

        public override void Add(object value)
        {
            _list.Add((T)value);
        }
    }   
}