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

            // The following code creates a new instance of TOptions using an optimistic locking strategy
            // rather than the pessimistic locking strategy used by the default implementation, avoiding deadlocks

            var newValue = _factory.Create(Microsoft.Extensions.Options.Options.DefaultName);
            var oldValue = Interlocked.CompareExchange(ref _value, newValue, null);
            return oldValue ?? newValue;

        }
    }
}