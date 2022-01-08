using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DemoGenerator
{
    public class ViewModelGeneratorSyntaxReceiver
        : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> ClassesToAugment { get; } = new List<ClassDeclarationSyntax>();
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax cds)
            //if (syntaxNode is ClassDeclarationSyntax cds && cds.Identifier.ValueText == "Books")
            {
                // && cds.BaseList.Types.Select(x => x.Type).Any(x => x.ToString() == "IGeneratable"))
                if(cds.BaseList != null)
                {
                    if(cds.BaseList.Types.Select(x => x.Type).Any(x => x.ToString() == "IGeneratable"))
                    {
                        var text = cds.GetText();
                        ClassesToAugment.Add(cds);
                    }       
                }
            }
        }
    }
}
