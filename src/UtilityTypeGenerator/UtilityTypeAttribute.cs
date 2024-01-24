namespace UtilityTypeGenerator;

using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class UtilityTypeAttribute(string selector) : Attribute
{
    public string Selector { get; } = selector;
}