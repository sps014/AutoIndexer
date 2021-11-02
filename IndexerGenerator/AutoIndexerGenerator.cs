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
        public const string ATTRIBUTE = "AutoIndexer";
        public const string ATTRIBUTE_WITH_SYMBOL = "[AutoIndexer";

        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            //get all the c# syntax trees from context compilation unit
            var allTrees = context.Compilation.SyntaxTrees
                .Where(t=>t.GetRoot().Language.Equals("C#"));

            var classes=GetAllClasses(allTrees);
            var methods=GetAllMethods(classes);

            var @params=GetAllAttrbutedParamList(methods).ToList();
            Console.WriteLine(@params[0]);
        }

        private IEnumerable<ClassDeclarationSyntax> GetAllClasses(IEnumerable<SyntaxTree> trees)
        {
            List<ClassDeclarationSyntax> classes = new List<ClassDeclarationSyntax>();
            foreach(var t in trees)
            {
                if (!t.GetRoot().GetText().ToString().Contains(ATTRIBUTE_WITH_SYMBOL))
                    continue;
                classes.AddRange(t.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>());
            }
            return classes;
        }
        private IEnumerable<MethodDeclarationSyntax> GetAllMethods(IEnumerable<ClassDeclarationSyntax> @class)
        {
            var methods=new List<MethodDeclarationSyntax>();
            foreach(var c in @class)
                methods.AddRange(c.Members.OfType<MethodDeclarationSyntax>());
            return methods;
        }
        private IEnumerable<IndexGeneratable> GetAllAttrbutedParamList(IEnumerable<MethodDeclarationSyntax> methods)
        {
            var generatables= new List<IndexGeneratable>();

            foreach(var method in methods)
            {

                if (!method.GetText().ToString().Contains(ATTRIBUTE_WITH_SYMBOL))
                    continue;

                List<ParameterSyntax> parameters = new();

                foreach(var parameter in method.ParameterList.Parameters)
                {
                    var hasAttrbute = parameter.AttributeLists
                        .Any(a => a.Attributes
                              .Any(at=>at.Name.ToString().Contains(ATTRIBUTE_WITH_SYMBOL))
                              );
                    if (hasAttrbute)
                        parameters.Add(parameter);
                }

                generatables.Add(new IndexGeneratable
                {
                    Method = method,
                    Parameters = parameters,
                    Class=method.Parent as ClassDeclarationSyntax
                });

            }

            return generatables;
        }

        public record IndexGeneratable
        {
            public ClassDeclarationSyntax Class { get; set; } 
            public MethodDeclarationSyntax Method { get; set; }
            public IReadOnlyList<ParameterSyntax> Parameters { get; set; }
        }
    }
}