namespace Example;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTypeGenerator;

public class Configuration : IConfiguration
{
    public SubConfiguration Sub { get; set; } = new();
    public string? ToBeOmitted { get; set; }
    ISubConfiguration IConfiguration.Sub
    {
        get => Sub;
        set => Sub = (SubConfiguration)value;
    }
}

public class SubConfiguration : ISubConfiguration
{
    public NotAnInterface Prop { get; set; } = new();
    IMyInterface ISubConfiguration.Prop
    {
        get => Prop;
        set => Prop = (NotAnInterface)value;
    }
}

public class NotAnInterface : IMyInterface;

public interface IMyInterface;

[UtilityType("Omit<SubConfiguration, Prop>")]
public partial interface  ISubConfiguration
{
    public IMyInterface Prop { get; set; }
}

[UtilityType("Omit<Configuration, Sub | ToBeOmitted>")]
public partial interface IConfiguration
{
    public ISubConfiguration Sub { get; set; }
}

[UtilityType("Import<IConfiguration>")]
internal partial class InternalConfiguration : IConfiguration
{

}
