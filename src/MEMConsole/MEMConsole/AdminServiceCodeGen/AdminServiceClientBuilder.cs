using System;
using System.Collections.Generic;
using System.Text;

namespace MEMConsole.AdminServiceCodeGen
{
    internal class AdminServiceClientBuilder
    {
        public Dictionary<string, string[]> TypeKeys { get; set; } = new Dictionary<string, string[]>();
        public string GetAdminServiceClientPartial()
        {
            var returnSb = new StringBuilder();
            returnSb.AppendLine("namespace MEMConsole.Client.AdminServiceModels");
            returnSb.AppendLine("{");
            returnSb.AppendLine("    public partial class AdminserviceClient");
            returnSb.AppendLine("    {");
            returnSb.AppendLine("        public string[] GetKeys<T>()");
            returnSb.AppendLine("        {");
            returnSb.AppendLine("            switch (typeof(T).Name)");
            returnSb.AppendLine("            {");
            foreach(var key in TypeKeys.Keys)
            {
                var typeSb = new StringBuilder();
                var count = 0;
                foreach(var ty in TypeKeys[key])
                {
                    if(count > 0)
                    {
                        typeSb.Append(", ");
                    }
                    count++;
                    typeSb.Append($"\"{ty}\"");
                }
                returnSb.AppendLine($"            case \"{key}\":");
                returnSb.AppendLine($"                return new string[] {{ {typeSb} }};");
            }
            returnSb.AppendLine("            default:");
            returnSb.AppendLine("                return Array.Empty<string>();");
            returnSb.AppendLine("            }");
            returnSb.AppendLine("        }");
            returnSb.AppendLine("    }");
            returnSb.AppendLine("}");
            return returnSb.ToString();
        }
    }
}
