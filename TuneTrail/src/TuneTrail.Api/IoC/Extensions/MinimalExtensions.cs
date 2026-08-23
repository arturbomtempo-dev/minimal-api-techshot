using System.Reflection;
using TuneTrail.Api.Contract;

namespace TuneTrail.Api.IoC.Extensions;

public static class MinimalExtensions
{
    public static void RegisterModules(this WebApplication app)
    {
        var moduleDefinitions = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t =>
                t.IsAssignableTo(typeof(IRegisterModule)) && !t.IsAbstract && !t.IsInterface
            )
            .Select(Activator.CreateInstance)
            .Cast<IRegisterModule>();

        foreach (var module in moduleDefinitions)
        {
            module.RegisterModule(app);
        }
    }
}
