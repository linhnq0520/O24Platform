using System.Runtime.CompilerServices;
using FluentMigrator;
using FluentMigrator.Exceptions;
using FluentMigrator.Runner.Processors;
using O24OpenAPI.Data.Configuration;

namespace O24OpenAPI.Data.Migrations;

/// <summary>
/// The 24 open api processor accessor class
/// </summary>
/// <seealso cref="IProcessorAccessor"/>
public class O24OpenAPIProcessorAccessor : IProcessorAccessor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="O24OpenAPIProcessorAccessor"/> class
    /// </summary>
    /// <param name="processors">The processors</param>
    public O24OpenAPIProcessorAccessor(IEnumerable<IMigrationProcessor> processors)
    {
        this.ConfigureProcessor(processors.ToList<IMigrationProcessor>());
    }

    /// <summary>
    /// Configures the processor using the specified processors
    /// </summary>
    /// <param name="processors">The processors</param>
    /// <exception cref="ProcessorFactoryNotFoundException"></exception>
    /// <exception cref="ProcessorFactoryNotFoundException">No migration processor registered.</exception>
    protected virtual void ConfigureProcessor(IList<IMigrationProcessor> processors)
    {
        DataConfig dataConfig = DataSettingsManager.LoadSettings();
        if (processors.Count == 0)
        {
            throw new ProcessorFactoryNotFoundException("No migration processor registered.");
        }

        if (dataConfig == null)
        {
            this.Processor = processors.FirstOrDefault<IMigrationProcessor>();
        }
        else
        {
            DataProviderType dataProvider = dataConfig.DataProvider;
            IMigrationProcessor generator;
            switch (dataProvider)
            {
                case DataProviderType.SqlServer:
                    generator = FindGenerator(processors, "SqlServer");
                    break;
                case DataProviderType.MySql:
                    generator = FindGenerator(processors, "MySQL");
                    break;
                case DataProviderType.PostgreSQL:
                    generator = FindGenerator(processors, "Postgres");
                    break;
                case DataProviderType.Oracle:
                    generator = FindGenerator(processors, "Oracle12CManaged");
                    break;
                default:
                    DefaultInterpolatedStringHandler interpolatedStringHandler =
                        new DefaultInterpolatedStringHandler(64, 1);
                    interpolatedStringHandler.AppendLiteral(
                        "A migration generator for Data provider type "
                    );
                    interpolatedStringHandler.AppendFormatted<DataProviderType>(
                        dataConfig.DataProvider
                    );
                    interpolatedStringHandler.AppendLiteral(" couldn't be found.");
                    throw new ProcessorFactoryNotFoundException(
                        interpolatedStringHandler.ToStringAndClear()
                    );
            }
            this.Processor = generator;
        }
    }

    /// <summary>
    /// Finds the generator using the specified processors
    /// </summary>
    /// <param name="processors">The processors</param>
    /// <param name="processorsId">The processors id</param>
    /// <exception cref="ProcessorFactoryNotFoundException"></exception>
    /// <returns>The migration processor</returns>
    protected static IMigrationProcessor FindGenerator(
        IList<IMigrationProcessor> processors,
        string processorsId
    )
    {
        IMigrationProcessor generator = processors.FirstOrDefault<IMigrationProcessor>(p =>
            p.DatabaseType.Equals(processorsId, StringComparison.OrdinalIgnoreCase)
            || p.DatabaseTypeAliases.Any<string>(a =>
                a.Equals(processorsId, StringComparison.OrdinalIgnoreCase)
            )
        );
        if (generator != null)
        {
            return generator;
        }

        string str = string.Join(
            ", ",
            processors
                .Select<IMigrationProcessor, string>(p => p.DatabaseType)
                .Union<string>(
                    processors.SelectMany<IMigrationProcessor, string>(p =>
                        p.DatabaseTypeAliases
                    )
                )
        );
        throw new ProcessorFactoryNotFoundException(
            "A migration generator with the ID "
                + processorsId
                + " couldn't be found. Available generators are: "
                + str
        );
    }

    /// <summary>
    /// Gets or sets the value of the processor
    /// </summary>
    public IMigrationProcessor Processor { get; protected set; }
}
