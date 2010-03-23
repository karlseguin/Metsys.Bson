namespace Metsys.Bson
{
    public class ScopedCode
    {
        public string CodeString { get; set; }        
        public object Scope { get; set; }
    }

    public class ScopedCode<T> : ScopedCode
    {        
        public new T Scope { get; set; }
    }
}