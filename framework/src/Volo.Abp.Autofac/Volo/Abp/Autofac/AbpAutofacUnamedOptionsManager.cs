using System.Threading;
using Microsoft.Extensions.Options;

namespace Volo.Abp.Autofac;

/// <summary>
/// The <see cref="AbpAutofacUnnamedOptionsManager"/> class is intended to replace the default
/// <see cref="Microsoft.Extensions.Options.UnnamedOptionsManager"/> in Microsoft.Extensions.Options.
/// The default implementation leads to deadlocks with Autofac when resolving <see cref="IOptions{TOptions}"/>
/// that have dependencies resolved by Autofac.
/// </summary>
/// <typeparam name="TOptions">Specifies the type of options being managed.</typeparam>
public sealed class AbpAutofacUnnamedOptionsManager<TOptions> : IOptions<TOptions>
    where TOptions : class
{
    private readonly IOptionsFactory<TOptions> _factory;
    private volatile TOptions? _value;

    public AbpAutofacUnnamedOptionsManager(IOptionsFactory<TOptions> factory)
    {
        _factory = factory;
    }

    public TOptions Value
    {
        get
        {
            if (_value is TOptions value)
            {
                return value;
            }

            // The following code synchronizes the concurrent creation of a new instance of TOptions without using a
            // pessimistic lock. Instead, it employs an atomic operation to avoid deadlocks that can occur with the
            // default UnnamedOptionsManager implementation when resolving IOptions<TOptions> with dependencies in
            // Autofac.

            var newValue = _factory.Create(Microsoft.Extensions.Options.Options.DefaultName);
            var oldValue = Interlocked.CompareExchange(ref _value, newValue, null);
            return oldValue ?? newValue;
        }
    }
}