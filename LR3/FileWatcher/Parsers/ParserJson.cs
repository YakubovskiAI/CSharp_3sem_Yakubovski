using System;
using System.Reflection;
using System.IO;
using System.Text.Json;

namespace FileWatcher
{
    class ParserJson : IParser
    {
        private readonly string path;

        private readonly Type mainType;

        public ParserJson(string path, Type mainType)
        {
            this.path = path;
            this.mainType = mainType;
        }

        public T GetOptions<T>()
        {
            string json;

            using (StreamReader sr = new StreamReader(path))
            {
                json = sr.ReadToEnd();
            }

            object result = Activator.CreateInstance(typeof(T));

            if (result is null)
            {
                throw new ArgumentNullException($"{nameof(result)} is null");
            }

            PropertyInfo[] properties = typeof(T).GetProperties();

            JsonElement rootNode = FindNode<T>(json);

            foreach (PropertyInfo param in properties)
            {
                DeserializeRecursive(param, result, rootNode);
            }

            return (T)result;
        }

        private void DeserializeRecursive(PropertyInfo param, object parent, JsonElement parentNode)
        {
            if (param.PropertyType.IsPrimitive || param.PropertyType == typeof(string))
            {
                param.SetValue(parent, Convert.ChangeType(parentNode.GetProperty(param.Name).GetRawText().Trim('"'), param.PropertyType));
            }
            else if (param.PropertyType.IsEnum)
            {
                param.SetValue(parent, Enum.Parse(param.PropertyType, parentNode.GetProperty(param.Name).GetRawText().Trim('"')));
            }
            else
            {
                Type subType = param.PropertyType;
                object subObj = Activator.CreateInstance(subType);

                param.SetValue(parent, subObj);

                PropertyInfo[] subparams = subType.GetProperties();
                foreach (PropertyInfo sparam in subparams)
                {
                    DeserializeRecursive(sparam, subObj, parentNode.GetProperty(param.Name));
                }
            }
        }

        private JsonElement FindNode<T>(string json)
        {
            JsonDocument jsonDocument = JsonDocument.Parse(json);

            JsonElement root = jsonDocument.RootElement;

            if (typeof(T) == mainType)
            {
                return root;
            }

            PropertyInfo[] properties = mainType.GetProperties();

            JsonElement? result = null;

            foreach (PropertyInfo param in properties)
            {
                FindNodeRecursive<T>(param, root, ref result);
            }

            if (result is null)
            {
                throw new ArgumentNullException($"{nameof(result)} is null");
            }

            return (JsonElement)result;
        }

        private void FindNodeRecursive<T>(PropertyInfo param, JsonElement parentNode, ref JsonElement? result)
        {
            if (result == null)
            {
                if (parentNode.TryGetProperty(param.Name, out JsonElement element) && param.PropertyType == typeof(T))
                {
                    result = (JsonElement?)element;
                    return;
                }
                else if (!param.PropertyType.IsPrimitive && param.PropertyType != typeof(string))
                {
                    Type subType = param.PropertyType;

                    PropertyInfo[] subparams = subType.GetProperties();
                    foreach (PropertyInfo sparam in subparams)
                    {
                        FindNodeRecursive<T>(sparam, parentNode.GetProperty(sparam.Name), ref result);
                    }
                }
            }
        }
    }
}
