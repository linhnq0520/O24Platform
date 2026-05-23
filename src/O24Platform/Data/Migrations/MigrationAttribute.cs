using System.Globalization;
using FluentMigrator;

namespace O24OpenAPI.Data.Migrations;

/// <summary>
/// The 24 open api migration attribute class
/// </summary>
/// <seealso cref="MigrationAttribute"/>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class O24OpenAPIMigrationAttribute : MigrationAttribute
{
    /// <summary>
    /// Gets the version using the specified date time
    /// </summary>
    /// <param name="dateTime">The date time</param>
    /// <returns>The long</returns>
    private static long GetVersion(string dateTime)
    {
        return DateTime
            .ParseExact(
                (ReadOnlySpan<char>)dateTime,
                O24OpenAPIMigrationDefaults.DateFormats,
                (IFormatProvider)CultureInfo.InvariantCulture
            )
            .Ticks;
    }

    /// <summary>
    /// Gets the version using the specified date time
    /// </summary>
    /// <param name="dateTime">The date time</param>
    /// <param name="migrationType">The migration type</param>
    /// <returns>The long</returns>
    private static long GetVersion(string dateTime, UpdateMigrationType migrationType)
    {
        return O24OpenAPIMigrationAttribute.GetVersion(dateTime) + (long)migrationType;
    }

    /// <summary>
    /// Gets the description using the specified neptune version
    /// </summary>
    /// <param name="neptuneVersion">The neptune version</param>
    /// <param name="migrationType">The migration type</param>
    /// <returns>The string</returns>
    private static string GetDescription(
        string neptuneVersion,
        UpdateMigrationType migrationType
    )
    {
        return string.Format(
            O24OpenAPIMigrationDefaults.UpdateMigrationDescription,
            (object)neptuneVersion,
            (object)migrationType.ToString()
        );
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="O24OpenAPIMigrationAttribute"/> class
    /// </summary>
    /// <param name="dateTime">The date time</param>
    /// <param name="targetMigrationProcess">The target migration process</param>
    public O24OpenAPIMigrationAttribute(
        string dateTime,
        MigrationProcessType targetMigrationProcess = MigrationProcessType.NoMatter
    )
        : base(O24OpenAPIMigrationAttribute.GetVersion(dateTime), (string)null)
    {
        this.TargetMigrationProcess = targetMigrationProcess;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="O24OpenAPIMigrationAttribute"/> class
    /// </summary>
    /// <param name="dateTime">The date time</param>
    /// <param name="description">The description</param>
    /// <param name="targetMigrationProcess">The target migration process</param>
    public O24OpenAPIMigrationAttribute(
        string dateTime,
        string description,
        MigrationProcessType targetMigrationProcess = MigrationProcessType.NoMatter
    )
        : base(O24OpenAPIMigrationAttribute.GetVersion(dateTime), description)
    {
        this.TargetMigrationProcess = targetMigrationProcess;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="O24OpenAPIMigrationAttribute"/> class
    /// </summary>
    /// <param name="dateTime">The date time</param>
    /// <param name="neptuneVersion">The neptune version</param>
    /// <param name="migrationType">The migration type</param>
    /// <param name="targetMigrationProcess">The target migration process</param>
    public O24OpenAPIMigrationAttribute(
        string dateTime,
        string neptuneVersion,
        UpdateMigrationType migrationType,
        MigrationProcessType targetMigrationProcess = MigrationProcessType.NoMatter
    )
        : base(
            O24OpenAPIMigrationAttribute.GetVersion(dateTime, migrationType),
            O24OpenAPIMigrationAttribute.GetDescription(neptuneVersion, migrationType)
        )
    {
        this.TargetMigrationProcess = targetMigrationProcess;
    }

    /// <summary>
    /// Gets or sets the value of the target migration process
    /// </summary>
    public MigrationProcessType TargetMigrationProcess { get; set; }
}
