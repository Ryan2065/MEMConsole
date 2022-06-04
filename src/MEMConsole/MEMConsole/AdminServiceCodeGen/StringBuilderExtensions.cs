using System;
using System.Collections.Generic;
using System.Text;

namespace MEMConsole.AdminServiceCodeGen
{
    internal static class StringBuilderExtensions
    {
        private static int indent = 0;
        private static string currentClassName = "";
        public static void StartNamespace(this StringBuilder sb, string nameSpaceName)
        {
            indent = 0;
            sb.AppendLineIndent($"namespace {nameSpaceName}");
            sb.AppendLineIndent("{");
            indent++;
        }
        public static void EndNamespace(this StringBuilder sb)
        {
            indent--;
            sb.AppendLineIndent("}");
        }
        public static void AddUsing(this StringBuilder sb, string usingName)
        {
            indent = 0;
            sb.AppendLineIndent($"using {usingName};");
        }
        public static void StartClass(this StringBuilder sb, string className, string additionalClassModifiers = "")
        {
            currentClassName = className;
            sb.AppendLineIndent($"public class {className} {additionalClassModifiers}");
            sb.AppendLineIndent("{");
            indent++;
        }
        public static void EndClass(this StringBuilder sb)
        {
            currentClassName = "";
            indent--;
            sb.AppendLineIndent("}");
        }
        public static void StartClassConstructor(this StringBuilder sb, string classParams = "")
        {
            sb.AppendLineIndent($"public {currentClassName}({classParams})");
            sb.AppendLineIndent("{");
            indent++;
        }
        public static void EndClassConstructor(this StringBuilder sb)
        {
            indent--;
            sb.AppendLineIndent("}");
        }
        public static void StartMethod(this StringBuilder sb, string methodName, string methodReturnType, string accessor = "public", string paramString = "")
        {
            sb.AppendLineIndent($"{accessor} {methodReturnType} {methodName}({paramString})");
            sb.AppendLineIndent("{");
            indent++;
        }
        public static void EndMethod(this StringBuilder sb)
        {
            indent--;
            sb.AppendLineIndent("}");
        }
        public static void AddField(this StringBuilder sb, string fieldName, string fieldType, bool readOnly = true, string defaultValue = "default!")
        {
            var readOnlyStr = String.Empty; 
            if(readOnly) { readOnlyStr = " readonly"; }
            sb.AppendLineIndent($"private{readOnlyStr} {fieldType} {fieldName}{GetDefaultValue(defaultValue)}");
        }
        public static void AddProperty(this StringBuilder sb, string propName, string propType, string defaultValue = "default!")
        {
            sb.AppendLineIndent($"public {propType} {propName} {{ get; set; }}{GetDefaultValue(defaultValue)}");
        }
        private static string GetDefaultValue(string defaultValue)
        {
            if (string.IsNullOrEmpty(defaultValue))
            {
                return ";";
            }
            else
            {
                return $" = {defaultValue};";
            }
        }
        public static void AppendLineIndent(this StringBuilder sb, string appendLine)
        {
            var indentSb = new StringBuilder();
            for(int i = indent; i != 0; i--)
            {
                indentSb.Append("    ");
            }
            sb.AppendLine($"{indentSb}{appendLine}");
        }
    }
}
