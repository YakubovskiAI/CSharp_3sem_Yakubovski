using System;
using System.Reflection;
using System.Xml;

namespace ServiceLib
{
    class ParserXML : IParser
    {
        private readonly string path;

        private readonly Type mainType;

        public ParserXML(string path, Type mainType)
        {
            this.path = path;
            this.mainType = mainType;
        }

        public T GetOptions<T>()
        {
            object result = Activator.CreateInstance(typeof(T));

            if (result is null)
            {
                throw new ArgumentNullException($"{nameof(result)} is null");
            }

            PropertyInfo[] properties = typeof(T).GetProperties();

            foreach (PropertyInfo param in properties)
            {
                DeserializeRecursive(param, result, FindNode<T>());
            }

            return (T)result;
        }

        private void DeserializeRecursive(PropertyInfo param, object parent, XmlNode parentNode)
        {
            foreach (XmlNode node in parentNode.ChildNodes)
            {
                if (node.Name == param.Name)
                {
                    if (param.PropertyType.IsPrimitive || param.PropertyType == typeof(string))
                    {
                        param.SetValue(parent, Convert.ChangeType(node.InnerText, param.PropertyType));
                    }
                    else if (param.PropertyType.IsEnum)
                    {
                        param.SetValue(parent, Enum.Parse(param.PropertyType, node.InnerText));
                    }
                    else
                    {
                        Type subType = param.PropertyType;
                        object subObj = Activator.CreateInstance(subType);

                        param.SetValue(parent, subObj);

                        PropertyInfo[] subparams = subType.GetProperties();
                        foreach (PropertyInfo sparam in subparams)
                        {
                            DeserializeRecursive(sparam, subObj, node);
                        }
                    }
                }
            }
        }

        private XmlNode FindNode<T>()
        {
            XmlDocument doc = new XmlDocument();

            doc.Load(path);

            if (typeof(T) == mainType)
            {
                return doc.DocumentElement;
            }

            PropertyInfo[] properties = mainType.GetProperties();

            XmlNode result = null;

            foreach (PropertyInfo param in properties)
            {
                FindNodeRecursive<T>(param, doc.DocumentElement, ref result);
            }

            if (result is null)
            {
                throw new ArgumentNullException($"{nameof(result)} is null");
            }

            return result;
        }

        private void FindNodeRecursive<T>(PropertyInfo param, XmlNode parentNode, ref XmlNode result)
        {
            foreach (XmlNode node in parentNode.ChildNodes)
            {
                if (node.Name == param.Name && param.PropertyType == typeof(T) && result == null)
                {
                    result = node;

                    if (!param.PropertyType.IsPrimitive && !(param.PropertyType == typeof(string)))
                    {
                        Type subType = param.PropertyType;

                        PropertyInfo[] subparams = subType.GetProperties();
                        foreach (PropertyInfo sparam in subparams)
                        {
                            FindNodeRecursive<T>(sparam, node, ref result);
                        }
                    }
                }
            }
        }
    }

}
