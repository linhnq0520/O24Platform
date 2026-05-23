namespace O24OpenAPI.Core.ComponentModel;

/// <summary>
/// The reader write lock type enum
/// </summary>
public enum ReaderWriteLockType
{
    /// <summary>
    /// The read reader write lock type
    /// </summary>
    Read,

    /// <summary>
    /// The write reader write lock type
    /// </summary>
    Write,

    /// <summary>
    /// The upgradeable read reader write lock type
    /// </summary>
    UpgradeableRead,
}
