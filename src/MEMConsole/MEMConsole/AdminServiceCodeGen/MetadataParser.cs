using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;

namespace MEMConsole.AdminServiceCodeGen
{
    internal class MetadataParser
    {
        private readonly XmlDocument _xmlDoc;
        private List<XmlNode> _schemaList;
        private Dictionary<string, XmlNode> _entityTypes;
        private Dictionary<string, string> _entityTypeNameReference;
        private readonly string _cmVersion;
        private readonly string _apiType;
        private readonly List<string> _includes;
        private readonly HashSet<string> _entityTypesToInclude = new();
        internal MetadataParser(AdminServiceSourceFile xmlFileInfo, Dictionary<string, List<string>> includes)
        {
            _xmlDoc = new XmlDocument();
            _xmlDoc.LoadXml(xmlFileInfo.Source);
            var fileBaseName = xmlFileInfo.FileName.Replace(".xml", "");
            var splitFileBaseName = fileBaseName.Split('.');
            _cmVersion = splitFileBaseName[0];
            _apiType = splitFileBaseName[1];
            _includes = includes[_apiType];
            LoadSchema();
            LoadEntityTypes();
            LoadEntitySets();
            CreateEntityTypeFiles();
        }
        /// <summary>
        /// Returns a namespace name to store the code in
        /// </summary>
        public string NamespaceName
        {
            get
            {
                return "v" + _cmVersion + "." + _apiType;
            }
        }

        public List<AdminServiceSourceFile> MetadataTypeFiles { get; set; } = new List<AdminServiceSourceFile>();

        /// <summary>
        /// Loads the Schema nodes from Edmx\DataServices\Schema
        /// </summary>
        private void LoadSchema()
        {
            _schemaList = new List<XmlNode>();
            foreach(XmlNode node in _xmlDoc.DocumentElement.ChildNodes)
            {
                if(node.Name == "DataServices")
                {
                    foreach(XmlNode dsChildNode in node.ChildNodes)
                    {
                        if(dsChildNode.Name == "Schema")
                        {
                            _schemaList.Add(dsChildNode);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Loads all Entitytypes and EnumTypes in the meatdata doc
        /// </summary>
        private void LoadEntityTypes()
        {
            _entityTypes = new Dictionary<string, XmlNode>();
            _entityTypeNameReference = new Dictionary<string, string>();
            foreach (XmlElement schema in _schemaList)
            {
                string keyStart = schema.GetAttribute("Namespace");
                
                foreach(XmlElement et in schema.GetElementsByTagName("EntityType"))
                {
                    _entityTypes.Add($"{keyStart}.{et.GetAttribute("Name")}", et);
                    _entityTypeNameReference.Add($"{keyStart}.{et.GetAttribute("Name")}", et.GetAttribute("Name"));
                    _entityTypeNameReference.Add($"Collection({keyStart}.{et.GetAttribute("Name")})", $"ICollection<{et.GetAttribute("Name")}>");
                }
                foreach (XmlElement et in schema.GetElementsByTagName("EnumType"))
                {
                    _entityTypes.Add($"{keyStart}.{et.GetAttribute("Name")}", et);
                    _entityTypeNameReference.Add($"{keyStart}.{et.GetAttribute("Name")}", et.GetAttribute("Name"));
                    _entityTypeNameReference.Add($"Collection({keyStart}.{et.GetAttribute("Name")})", $"ICollection<{et.GetAttribute("Name")}>");
                }
                foreach (XmlElement et in schema.GetElementsByTagName("ComplexType"))
                {
                    _entityTypes.Add($"{keyStart}.{et.GetAttribute("Name")}", et);
                    _entityTypeNameReference.Add($"{keyStart}.{et.GetAttribute("Name")}", et.GetAttribute("Name"));
                    _entityTypeNameReference.Add($"Collection({keyStart}.{et.GetAttribute("Name")})", $"ICollection<{et.GetAttribute("Name")}>");
                }
            }

        }

        private void CreateEntityTypeFiles()
        {
            while(_entityTypesToInclude.Count > 0)
            {
                var etToInclude = _entityTypesToInclude.First();
                var fileName = $"{NamespaceName}.{_entityTypeNameReference[etToInclude]}.cs";
                if (MetadataTypeFiles.Any(p => p.FileName.Equals(fileName))) { continue; }
                MetadataTypeFiles.Add(new AdminServiceSourceFile()
                {
                    FileName = fileName,
                    Source = ParseEntityType((XmlElement)_entityTypes[etToInclude])
                });
                _entityTypesToInclude.Remove(etToInclude);
            }
        }

        private void LoadEntitySets()
        {
            var adminServiceClient = new StringBuilder();
            adminServiceClient.AddUsing("System.Collections.Generic");
            adminServiceClient.AddUsing("System.Text.Json");
            adminServiceClient.StartNamespace($"MEMConsole.Client.AdminServiceModels.{NamespaceName}");
            adminServiceClient.StartClass($"AdminService{_apiType}");
            adminServiceClient.AddField("_httpClient", "HttpClient");
            adminServiceClient.StartClassConstructor("IHttpClientFactory clientFactory");
            
            SetASConstructorBody(adminServiceClient);
            adminServiceClient.EndClassConstructor();
            SetASProperties(adminServiceClient);
            adminServiceClient.EndClass();
            SetASPropertyTypes(adminServiceClient);
            adminServiceClient.EndNamespace();
            MetadataTypeFiles.Add(new AdminServiceSourceFile()
            {
                FileName = $"AdminService{_apiType}.cs",
                Source = adminServiceClient.ToString()
            });
        }

        private IEnumerable<XmlElement> GetEntitySets()
        {
            foreach (XmlElement schema in _schemaList)
            {
                foreach (XmlElement et in schema.GetElementsByTagName("EntitySet"))
                {
                    yield return et;
                }
            }
        }

        private void SetASProperties(StringBuilder sb)
        {
            foreach (XmlElement et in GetEntitySets())
            {
                var name = et.GetAttribute("Name");
                if (_includes.Contains(name))
                {
                    sb.AddProperty(name, $"AS{name}");
                }
            }
        }

        private void SetASPropertyTypes(StringBuilder sb)
        {
            sb.StartClass("ODataResponse", "<T> where T : class");
            sb.AddProperty("value", "List<T>");
            sb.EndClass();
            foreach (XmlElement et in GetEntitySets())
            {
                if (_includes.Contains(et.GetAttribute("Name")))
                {
                    SetASBaseClass(sb, et);
                }
            }
        }

        private void SetASConstructorBody(StringBuilder sb)
        {
            sb.AppendLineIndent("_httpClient = clientFactory.CreateClient(\"MEMClient\");");
            foreach (XmlElement et in GetEntitySets())
            {
                var name = et.GetAttribute("Name");
                if (_includes.Contains(name))
                {
                    sb.AppendLineIndent($"{name} = new AS{name}(_httpClient);");
                }
            }
        }

        private void SetASBaseClass(StringBuilder sb, XmlElement et)
        {
            string etType = GetEntityTypeNameRef(et.GetAttribute("EntityType"));
            sb.StartClass($"AS{et.GetAttribute("Name")}");
            sb.AddField("_httpClient", "HttpClient");
            sb.StartClassConstructor("HttpClient httpClient");
            sb.AppendLineIndent("_httpClient = httpClient;");
            sb.EndClassConstructor();
            sb.StartMethod("ConvertToType", $"List<{etType}>", "private", "string jsonResult");
            sb.AppendLineIndent($"var returnList = new List<{etType}>();");
            sb.AppendLineIndent("if (string.IsNullOrEmpty(jsonResult)) { return returnList; }");
            sb.AppendLineIndent($"var results = JsonSerializer.Deserialize<ODataResponse<{etType}>>(jsonResult);");
            sb.AppendLineIndent("if(results == null) { return returnList; }");
            sb.AppendLineIndent("if(results.value == null) { return returnList; }");
            sb.AppendLineIndent("return results.value;");
            sb.EndMethod();
            sb.StartMethod("Get", $"async Task<List<{etType}>>");
            sb.AppendLineIndent($"var stringValue = await _httpClient.GetStringAsync(\"{_apiType}/{etType}\");");
            sb.AppendLineIndent($"return ConvertToType(stringValue);");
            sb.EndMethod();
            sb.EndClass();
        }

        /*
        public string ParseEntitySet(string name)
        {
            var entitySet = GetEntitySet(name);
            if(entitySet == null) { return default!; }

            var entityType = GetEntityType((XmlElement)entitySet);
            if(entityType == null) { return default!; }

            return ParseEntityType((XmlElement)entityType);
        }

        private XmlNode GetEntitySet(string name)
        {
            foreach (var schema in _schemaList)
            {
                foreach (XmlElement es in ((XmlElement)schema).GetElementsByTagName("EntitySet"))
                {
                    if (string.Equals(es.GetAttribute("Name"), name, StringComparison.OrdinalIgnoreCase))
                    {
                        return es;
                    }
                }
            }
            return null;
        }
        private XmlNode GetEntityType(XmlElement entitySet)
        {
            var entityType = entitySet.GetAttribute("EntityType");
            if(_entityTypes.TryGetValue(entityType, out var val))
            {
                return val;
            }
            return null;
        }*/
        private string ParseEntityType(XmlElement entityType)
        {
            var keys = GetEntityTypeKeys(entityType);
            string objName = "class";
            if(entityType.Name == "EnumType")
            {
                objName = "enum";
            }
            string baseType = "";
            if (!string.IsNullOrEmpty(entityType.GetAttribute("BaseType")))
            {
                baseType = $" : {GetEntityTypeNameRef(entityType.GetAttribute("BaseType"))}";
            }
            var etClass = new StringBuilder();
            etClass.AppendLine("using System.Collections.Generic;");
            etClass.AppendLine($"namespace MEMConsole.Client.AdminServiceModels.{NamespaceName}");
            etClass.AppendLine("{");
            etClass.AppendLine($"    public {objName} {entityType.GetAttribute("Name")}{baseType}");
            etClass.AppendLine("    {");
            etClass.AppendLine(GetEntityTypeProperties(entityType));
            etClass.AppendLine("    }");
            etClass.AppendLine("}");
            return etClass.ToString();
        }
        private string[] GetEntityTypeKeys(XmlElement entityType)
        {
            var returnString = new List<string>();
            foreach(XmlNode chNode in entityType.ChildNodes)
            {
                if(chNode.Name == "Key")
                {
                    foreach(XmlElement propRefs in chNode.ChildNodes)
                    {
                        if(propRefs.Name == "PropertyRef")
                        {
                            returnString.Add(propRefs.GetAttribute("Name"));
                        }
                    }
                }
            }
            return returnString.ToArray();
        }
        private string GetEntityTypeProperties(XmlElement entityType)
        {
            var propDefSb = new StringBuilder();
            foreach(XmlElement chNode in entityType.ChildNodes)
            {
                if(chNode.Name == "Property")
                {
                    if (chNode.GetAttribute("Name").StartsWith("__")) { continue; }
                    bool nullable = true;
                    if (chNode.HasAttribute("Nullable") && string.Equals(chNode.GetAttribute("Nullable"), "false", StringComparison.OrdinalIgnoreCase))
                    {
                        nullable = false;
                    }
                    propDefSb.AppendLine($"        {GetCSharpProperty(chNode.GetAttribute("Name"), chNode.GetAttribute("Type"), nullable)}");
                }
                else if(chNode.Name == "Member")
                {
                    propDefSb.AppendLine($"        {chNode.GetAttribute("Name")} = {chNode.GetAttribute("Value")},");
                }
            }
            return propDefSb.ToString();
        }
        private string GetCSharpProperty(string name, string EdmType, bool nullable)
        {
            var sb = new StringBuilder();
            sb.Append("public ");
            sb.Append(ConvertTypeEdmToCSharp(EdmType));
            if (nullable)
            {
                sb.Append("?");
            }
            sb.Append($" {name} {{ get; set; }}");
            if (!nullable)
            {
                sb.Append(" = default!;");
            }
            return sb.ToString();
        }
        private string GetEntityTypeNameRef(string key)
        {
            _entityTypesToInclude.Add(key);
            return _entityTypeNameReference[key];
        }
        private string ConvertTypeEdmToCSharp(string edmType)
        {
            return edmType switch
            {
                "Edm.Int32" => "int",
                "Edm.String" => "string",
                "Collection(Edm.String)" => "ICollection<string>",
                "Edm.Int64" => "long",
                "Edm.Boolean" => "bool",
                "Collection(Edm.Int32)" => "ICollection<int>",
                "Edm.DateTimeOffset" => "DateTime",
                "Edm.Decimal" => "decimal",
                "Edm.Binary" => "byte[]",
                "Collection(Edm.Int64)" => "ICollection<long>",
                "Edm.Double" => "double",
                "Edm.Byte" => "byte",
                "Collection(Edm.DateTimeOffset)" => "ICollection<DateTime>",
                "Collection(Edm.Boolean)" => "ICollection<bool>",
                "Edm.Guid" => "Guid",
                "Edm.Int16" => "short",
                _ => GetEntityTypeNameRef(edmType),
            };
        }
    }
    public class AdminServiceSourceFile
    {
        public string FileName { get; set; } = default!;
        public string Source { get; set; } = default!;
    }
}
