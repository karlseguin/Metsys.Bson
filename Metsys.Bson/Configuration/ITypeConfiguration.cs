using System;
using System.Linq.Expressions;

namespace Metsys.Bson.Configuration
{
    public interface ITypeConfiguration<T>
    {
        ITypeConfiguration<T> UseAlias(Expression<Func<T, object>> expression, string alias);
    }
    
    internal class TypeConfiguration<T> : ITypeConfiguration<T>
    {
        private readonly BsonConfiguration _configuration;
        
        internal TypeConfiguration(BsonConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ITypeConfiguration<T> UseAlias(Expression<Func<T, object>> expression, string alias)
        {
            var member = expression.GetMemberExpression();
            _configuration.AddMap<T>(member.GetName(), alias);
            return this;
        }
    }
}