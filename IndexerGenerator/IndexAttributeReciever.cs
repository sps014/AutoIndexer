using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace IndexerGenerator
{
    internal class IndexAttributeReciever : ISyntaxReceiver
    {
        public HashSet<AttributeSyntax> AutoIndexerAttribute { get;private set; } = new();
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if(syntaxNode is AttributeSyntax attribute)
                if(attribute.Name.ToString() == nameof(AutoIndexerAttribute).Replace(nameof(Attribute),string.Empty))
                    AutoIndexerAttribute.Add(attribute);
        }
    }
}
