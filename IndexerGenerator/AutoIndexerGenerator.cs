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

namespace IndexerGenerator
{
    [Generator]
    public class AutoIndexerGenerator:ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            //get all the c# syntax trees from context compilation unit
            var allTrees = context.Compilation.SyntaxTrees
                .Where(t=>t.GetRoot().Language.Equals("C#"));

            foreach (var t in allTrees)
            {
                var classes = t.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();

                foreach (var c in classes)
                {
                    var methods=c.Members.OfType<MethodDeclarationSyntax>().ToList();
                    foreach (var method in methods)
                    {
                        var prms= method.ParameterList.Parameters;
                        List<ParameterSyntax> parameters= new List<ParameterSyntax>();
                        foreach (var p in prms)
                        {
                            var v=p.AttributeLists.Select(a=>a.Attributes.Where(t=>t.Name.ToString().Contains("AutoIndexer"))).ToList();
                            if(v.Count>=1)
                            parameters.Add(p);
                        }
                        var Info = new ParamInfo
                        {
                            Class=c,
                            Method=method,
                            Parameters=parameters
                        };
                    }
                }
            }

        }

        private IEnumerable<ClassDeclarationSyntax> GetAllClasses(SyntaxTree tree)
        {
            return tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();
        }
        private IEnumerable<MethodDeclarationSyntax> GetAllMethods(ClassDeclarationSyntax @class)
        {
            return @class.Members.OfType<MethodDeclarationSyntax>();
        }
        private SeparatedSyntaxList<ParameterSyntax> GetAllParams(MethodDeclarationSyntax method)
        {
            return method.ParameterList.Parameters;
        }

        public record ParamInfo
        {
            public ClassDeclarationSyntax Class { get; set; } 
            public MethodDeclarationSyntax Method { get; set; }
            public IReadOnlyList<ParameterSyntax> Parameters { get; set; }
        }
    }
}