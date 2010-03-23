using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Metsys.Bson
{        
    internal class TypeHelper
    {
        private static readonly IDictionary<Type, TypeHelper> _cachedTypeLookup = new Dictionary<Type, TypeHelper>();
        private readonly IDictionary<string, MagicProperty> _properties;

        public MagicProperty Expando { get; private set; }
        
        private TypeHelper(Type type)
        {
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            _properties = LoadMagicProperties(properties);
            if (typeof(IExpando).IsAssignableFrom(type))
            {
                Expando = _properties["Expando"];
            }
        }

        public ICollection<MagicProperty> GetProperties()
        {
            return _properties.Values;
        }
        public MagicProperty FindProperty(string name)
        {
            return _properties.ContainsKey(name) ? _properties[name] : null;
        }
        
        public static TypeHelper GetHelperForType(Type type)
        {
            TypeHelper helper;
            if (!_cachedTypeLookup.TryGetValue(type, out helper))
            {
                helper = new TypeHelper(type);
                _cachedTypeLookup[type] = helper;
            }

            return helper;
        }
        public static string FindProperty(LambdaExpression lambdaExpression)
        {
            Expression expressionToCheck = lambdaExpression;

            var done = false;

            while (!done)
            {
                switch (expressionToCheck.NodeType)
                {
                    case ExpressionType.Convert:
                        expressionToCheck = ((UnaryExpression)expressionToCheck).Operand;
                        break;

                    case ExpressionType.Lambda:
                        expressionToCheck = ((LambdaExpression)expressionToCheck).Body;
                        break;

                    case ExpressionType.MemberAccess:
                        var memberExpression = (MemberExpression)expressionToCheck;

                        if (memberExpression.Expression.NodeType != ExpressionType.Parameter &&
                            memberExpression.Expression.NodeType != ExpressionType.Convert)
                        {
                            throw new ArgumentException(
                                string.Format("Expression '{0}' must resolve to top-level member.", lambdaExpression),
                                "lambdaExpression");
                        }

                        return memberExpression.Member.Name;

                    default:
                        done = true;
                        break;
                }
            }

            return null;
        }
        public static PropertyInfo FindProperty(Type type, string name)
        {
            return type.GetProperties().Where(p => p.Name == name).First();
        }
        private static IDictionary<string, MagicProperty> LoadMagicProperties(IEnumerable<PropertyInfo> properties)
        {
            var magic = new Dictionary<string, MagicProperty>(StringComparer.CurrentCultureIgnoreCase);
            foreach (var property in properties)
            {
                if (property.GetIndexParameters().Length > 0)
                {
                    continue;
                }
                var name = property.Name;
                magic.Add(name, new MagicProperty(property));
            }

            return magic;
        }
    }
}