using Microsoft.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.CodeDom.Compiler;
using System.IO;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics.Contracts;

namespace IndexerGenerator
{
    [Generator]
    public class AutoIndexerGenerator:ISourceGenerator
    {

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(()=>new IndexAttributeReciever());
        }

        public void Execute(GeneratorExecutionContext context)
        {

            if (context.SyntaxReceiver is not IndexAttributeReciever reciever)
                return;

            Dictionary<MethodDeclarationSyntax, HashSet<ParameterSyntax>> GroupedParameters = new();
            Dictionary<ClassDeclarationSyntax,HashSet<MethodDeclarationSyntax>> GroupedMethods= new();

            foreach (var att in reciever.AutoIndexerAttribute)
            {
                var attributelist = ParentOfType<AttributeListSyntax,AttributeSyntax>(att);
                if (attributelist == null) continue;

                var param = ParentOfType<ParameterSyntax,AttributeListSyntax>(attributelist);
                if (param == null) continue;

                var paramList = ParentOfType<ParameterListSyntax,ParameterSyntax>(param);
                if (paramList == null) continue;

                var method=ParentOfType<MethodDeclarationSyntax,ParameterListSyntax>(paramList);
                if (method == null) continue;


                var @class=ParentOfType<ClassDeclarationSyntax,MethodDeclarationSyntax>(method);
                if (@class == null) continue;

                var @namespace=GetNamespace(@class);
                if (string.IsNullOrWhiteSpace(@namespace)) continue;


                if (GroupedMethods.ContainsKey(@class))
                    GroupedMethods[@class].Add(method);
                else
                    GroupedMethods.Add(@class, new HashSet<MethodDeclarationSyntax> { method });

                if (GroupedParameters.ContainsKey(method))
                    GroupedParameters[method].Add(param);
                else
                    GroupedParameters.Add(method, new HashSet<ParameterSyntax> { param });

            }

            GenerateForClass(GroupedMethods,GroupedParameters,context);
            
        }

        private void GenerateForClass(IReadOnlyDictionary<ClassDeclarationSyntax,HashSet<MethodDeclarationSyntax>> classes
            ,IReadOnlyDictionary<MethodDeclarationSyntax,HashSet<ParameterSyntax>> indexedParameters
            ,GeneratorExecutionContext context)
        {
            int i = 0;
            foreach (var @class in classes)
            {
                using StringWriter sw = new StringWriter();
                using IndentedTextWriter writer = new IndentedTextWriter(sw);

                var @namespace = GetNamespace(@class.Key);
                string className = $"{@class.Key.Identifier}Extn{i++}";
                string oldClassName = GetClassName(@class.Key);
                //add namespace
                writer.WriteLine($"namespace {@namespace};");
                //add class
                writer.WriteLine($"public static class {className}");
                writer.WriteLine("{");
                writer.Indent++;


                //add extension methods
                foreach(var method in @class.Value)
                {
                    var methodName = method.Identifier.ValueText;
                    string returnType=method.ReturnType.GetText().ToString();

                    var parametersNew = GetNewParameters(method, indexedParameters[method],oldClassName);
                    //create function signature
                    writer.Write($"public static {returnType} {methodName}{GetGenericTypes(@class.Key,method)}(");
                    writer.Write(parametersNew);
                    writer.Write(")");
                    writer.WriteLine("=>");
                    writer.Indent++;
                    //write Body of method
                    writer.Write($"@type.{methodName}(");
                    var actualParams = GetNewActualParameters(method, indexedParameters[method]);
                    writer.Write(actualParams);
                    writer.WriteLine(");");
                    writer.Indent--;
                    writer.WriteLine();
                }

                writer.Indent--;
                writer.WriteLine("}");

                Console.WriteLine(sw.ToString());
                context.AddSource(className, SourceText.From(sw.ToString(), Encoding.UTF8));
            }
        }
        private string GetNewParameters(MethodDeclarationSyntax method,HashSet<ParameterSyntax> indexedParameters,string className)
        {
            string param = $"this {className} @type,";
            int ct = method.ParameterList.Parameters.Count;
            int i = 0;
            foreach (var p in method.ParameterList.Parameters)
            {
                if (indexedParameters.Contains(p))
                    param += $"Index {p.Identifier.ValueText}";
                else
                    param += $"{p.Type.GetText()} {p.Identifier.ValueText}";

                if (i != ct - 1)
                    param += ", ";

                i++;
            }

            return param;
        }
        private string GetNewActualParameters(MethodDeclarationSyntax method, HashSet<ParameterSyntax> indexedParameters)
        {
            string param = "";
            int ct = method.ParameterList.Parameters.Count;
            int i = 0;
            foreach (var p in method.ParameterList.Parameters)
            {
                if (indexedParameters.Contains(p))
                    param += $"{p.Identifier.ValueText}.GetOffset(@type.Count)";
                else
                    param += $"{p.Identifier.ValueText}";

                if (i != ct - 1)
                    param += ", ";

                i++;
            }

            return param;
        }
        private string GetNamespace(ClassDeclarationSyntax node)
        {
            var parent = node.Parent;
            while (parent.IsKind(SyntaxKind.ClassDeclaration))
            {
                parent = parent.Parent;
            }
            if(parent is NamespaceDeclarationSyntax ns)
                return ns.Name.ToString();
            return null;
        }
        public string GetClassName(ClassDeclarationSyntax @class)
        {
            return @class.Identifier.Text+@class.TypeParameterList.ToString();
        }
        public string GetGenericTypes(ClassDeclarationSyntax @class,MethodDeclarationSyntax method)
        {
            string res = "<";
            if(@class.TypeParameterList != null)
            foreach (var item in @class.TypeParameterList.Parameters)
            {
                res += item.Identifier.ValueText+",";
            }
            if (method.TypeParameterList != null)
                foreach (var item in method.TypeParameterList.Parameters)
                res += item.Identifier.ValueText + ",";
            res=res.TrimEnd(',');
            if(res.Length > 1)
                return res+">";
            return string.Empty;
        }

        private T ParentOfType<T,S>(S node) where T:SyntaxNode where S:SyntaxNode
        {
            if(node.Parent is T parent)
                return parent;
            return null;
        }


        public record IndexGeneratable
        {
            public ClassDeclarationSyntax Class { get; set; } 
            public MethodDeclarationSyntax Method { get; set; }
            public IReadOnlyList<ParameterSyntax> AttrbutedParameters { get; set; }
        }
    }
}