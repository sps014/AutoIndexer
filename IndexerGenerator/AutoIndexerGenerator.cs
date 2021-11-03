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


                if (GroupedMethods.ContainsKey(@class))
                    GroupedMethods[@class].Add(method);
                else
                    GroupedMethods.Add(@class, new HashSet<MethodDeclarationSyntax> { method });

                if (GroupedParameters.ContainsKey(method))
                    GroupedParameters[method].Add(param);
                else
                    GroupedParameters.Add(method, new HashSet<ParameterSyntax> { param });

            }

            GenerateForClass(GroupedMethods, context);
            
        }

        private void GenerateForClass(IReadOnlyDictionary<ClassDeclarationSyntax,HashSet<MethodDeclarationSyntax>> classes,GeneratorExecutionContext context)
        {
            foreach(var @class in classes)
            {
                Console.WriteLine(GetNamespace(context, @class.Key));
            }
        }
        private string GetNamespace(GeneratorExecutionContext context,ClassDeclarationSyntax node)
        {
            var parent = node.Parent;
            while (parent.IsKind(SyntaxKind.ClassDeclaration))
            {
                var parentClass = parent as ClassDeclarationSyntax;
                parent = parent.Parent;
            }
            if(parent is NamespaceDeclarationSyntax ns)
                return ns.Name.ToString();
            return null;
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