using System.Collections;

namespace Metsys.Bson
{
    internal class ListWrapper : BaseWrapper
    {
        private IList _list;
        protected override void SetContainer(object container)
        {
            _list = container == null ? new ArrayList() : (IList)container;
        }

        public override object Collection
        {
            get { return _list;}
        }

        public override void Add(object value)
        {
            _list.Add(value);
        }
    }
}