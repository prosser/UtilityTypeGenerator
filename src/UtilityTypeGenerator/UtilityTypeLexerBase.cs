namespace UtilityTypeGenerator;

using System.Collections.Generic;
using Antlr4.Runtime;

public abstract class UtilityTypeLexerBase : Lexer
{
    protected Stack<int> curlyLevels = new();

    protected int interpolatedStringLevel;

    protected Stack<bool> interpolatedVerbatiums = new();

    protected bool verbatium;

    protected UtilityTypeLexerBase(ICharStream input)
        : base(input)
    {
    }

    protected UtilityTypeLexerBase(ICharStream input, TextWriter output, TextWriter errorOutput)
        : base(input, output, errorOutput)
    {
    }

    protected bool IsRegularCharInside()
    {
        return !verbatium;
    }

    protected bool IsVerbatiumDoubleQuoteInside()
    {
        return verbatium;
    }

    protected void OnCloseBrace()
    {
        if (interpolatedStringLevel > 0)
        {
            curlyLevels.Push(curlyLevels.Pop() - 1);
            if (curlyLevels.Peek() == 0)
            {
                _ = curlyLevels.Pop();
                Skip();
                _ = PopMode();
            }
        }
    }

    protected void OnCloseBraceInside()
    {
        _ = curlyLevels.Pop();
    }

    protected void OnDoubleQuoteInside()
    {
        interpolatedStringLevel--;
        _ = interpolatedVerbatiums.Pop();
        verbatium = interpolatedVerbatiums.Count > 0 && interpolatedVerbatiums.Peek();
    }

    protected void OnInterpolatedRegularStringStart()
    {
        interpolatedStringLevel++;
        interpolatedVerbatiums.Push(false);
        verbatium = false;
    }

    protected void OnInterpolatedVerbatiumStringStart()
    {
        interpolatedStringLevel++;
        interpolatedVerbatiums.Push(true);
        verbatium = true;
    }

    protected void OnOpenBrace()
    {
        if (interpolatedStringLevel > 0)
        {
            curlyLevels.Push(curlyLevels.Pop() + 1);
        }
    }

    //protected void OnColon()
    //{
    //    if (interpolatedStringLevel > 0)
    //    {
    //        int ind = 1;
    //        bool switchToFormatString = true;
    //        while ((char)_input.La(ind) != '}')
    //        {
    //            if (_input.La(ind) == ':' || _input.La(ind) == ')')
    //            {
    //                switchToFormatString = false;
    //                break;
    //            }
    //            ind++;
    //        }
    //        if (switchToFormatString)
    //        {
    //            this.Mode(INTERPOLATION_FORMAT);
    //        }
    //    }
    //}

    protected void OpenBraceInside()
    {
        curlyLevels.Push(1);
    }
}