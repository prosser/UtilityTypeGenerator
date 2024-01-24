﻿namespace UtilityTypeGenerator;

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Microsoft.CodeAnalysis;

internal static class UtilityTypeHelper
{
    public static UtilityTypeSelector? Parse(string[] usingNamespaces, Compilation compilation, string input)
    {
        AntlrInputStream chars = new(input);
        UtilityTypesLexer lexer = new(chars);
        CommonTokenStream tokens = new(lexer);
        UtilityTypesParser parser = new(tokens)
        {
            BuildParseTree = true
        };

        IParseTree tree = parser.selector() ?? throw new FormatException("Failed to parse selector");
        UtilityTypesParserListener listener = new(usingNamespaces, compilation);
        ParseTreeWalker.Default.Walk(listener, tree);

        return listener.Selector;
    }
}
