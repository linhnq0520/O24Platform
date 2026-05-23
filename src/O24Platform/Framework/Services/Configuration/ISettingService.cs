using System.Linq.Expressions;
using O24OpenAPI.Core;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Framework.Models;

namespace O24OpenAPI.Framework.Services.Configuration;

/// <summary>Setting service interface</summary>
public interface ISettingService
{
    /// <summary>
    /// Create
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    Task<Setting> Create(SettingCreateModel model);

    /// <summary>
    /// Delete
    /// </summary>
    /// <param name="id"></param>
    /// <param name="referenceId"></param>
    /// <returns></returns>
    Task<Setting> Delete(int id, string referenceId = "");

    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<Setting> GetById(int id);

    /// <summary>
    /// Insert
    /// </summary>
    /// <param name="value"></param>
    /// <param name="referenceId"></param>
    /// <returns></returns>
    Task Insert(Setting value, string referenceId = "");

    /// <summary>
    /// Simple search
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    Task<IPagedList<Setting>> Search(SimpleSearchModel model);

    /// <summary>
    /// Advanced Search
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    Task<IPagedList<Setting>> Search(SettingSearchModel model);

    /// <summary>
    /// Update
    /// </summary>
    /// <param name="model"></param>
    /// <param name="referenceId"></param>
    /// <returns></returns>
    Task<SettingUpdateModel> Update(SettingUpdateModel model, string referenceId = "");

    /// <summary>
    /// View
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<Setting> View(int id);

    // <summary>
    /// GetByPrimaryKey
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    Task<Setting> GetByPrimaryKey(string name);

    /// <summary>Get setting by identifier</summary>
    /// <param name="settingId">Setting identifier</param>
    /// <returns></returns>
    Task<Setting> GetSettingById(int settingId);

    /// <summary>Delete a setting</summary>
    /// <param name="setting">Setting</param>
    /// <returns></returns>
    Task DeleteSetting(Setting setting);

    /// <summary>Deletes settings</summary>
    /// <param name="settings">Settings</param>
    /// <returns></returns>
    Task DeleteSettings(IList<Setting> settings);

    /// <summary>Get setting by key</summary>
    /// <param name="key">Key</param>
    /// <param name="organizationId">Organization identifier</param>
    /// <param name="loadSharedValueIfNotFound">A value indicating whether a shared (for all organizations) value should be loaded if a value specific for a certain is not found</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the setting
    /// </returns>
    Task<Setting> GetSetting(
        string key,
        int organizationId = 0,
        bool loadSharedValueIfNotFound = false
    );

    /// <summary>Set setting value</summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    /// <param name="organizationId">Organization identifier</param>
    /// <param name="clearCache">A value indicating whether to clear cache after setting update</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task SetSetting<T>(string key, T value, int organizationId = 0, bool clearCache = true);

    /// <summary>Gets all settings</summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the settings
    /// </returns>
    Task<IList<Setting>> GetAllSettings();

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    Task<IList<Setting>> SearchCommon(SimpleSearchModel model);

    /// <summary>
    ///
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    Task<IList<Setting>> SearchAdvance(AdvanceSearchParamModel model);

    /// <summary>
    ///
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    Task<T> Get<T>(string key, T defaultValue);

    /// <summary>Determines whether a setting exists</summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="TPropType">Property type</typeparam>
    /// <param name="settings">Settings</param>
    /// <param name="keySelector">Key selector</param>
    /// <param name="organizationId">Organization identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the rue -setting exists; false - does not exist
    /// </returns>
    Task<bool> SettingExists<T, TPropType>(
        T settings,
        Expression<Func<T, TPropType>> keySelector,
        int organizationId = 0
    )
        where T : ISettings, new();

    /// <summary>Load settings</summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="organizationId">Organization identifier for which settings should be loaded</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task<T> LoadSetting<T>(int organizationId = 0)
        where T : ISettings, new();

    /// <summary>Load settings</summary>
    /// <param name="type">Type</param>
    /// <param name="organizationId">Organization identifier for which settings should be loaded</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task<ISettings> LoadSetting(Type type, int organizationId = 0);

    /// <summary>Save settings object</summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="organizationId">Organization identifier</param>
    /// <param name="settings">Setting instance</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task SaveSetting<T>(T settings, int organizationId = 0)
        where T : ISettings, new();

    /// <summary>Save settings object</summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="TPropType">Property type</typeparam>
    /// <param name="settings">Settings</param>
    /// <param name="keySelector">Key selector</param>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="clearCache">A value indicating whether to clear cache after setting update</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task SaveSetting<T, TPropType>(
        T settings,
        Expression<Func<T, TPropType>> keySelector,
        int organizationId = 0,
        bool clearCache = true
    )
        where T : ISettings, new();

    /// <summary>Delete all settings</summary>
    /// <typeparam name="T">Type</typeparam>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task DeleteSetting<T>()
        where T : ISettings, new();

    /// <summary>Delete settings object</summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="TPropType">Property type</typeparam>
    /// <param name="settings">Settings</param>
    /// <param name="keySelector">Key selector</param>
    /// <param name="organizationId">Organization ID</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task DeleteSetting<T, TPropType>(
        T settings,
        Expression<Func<T, TPropType>> keySelector,
        int organizationId = 0
    )
        where T : ISettings, new();

    /// <summary>Clear cache</summary>
    /// <returns></returns>
    Task ClearCache();

    /// <summary>Get setging key (stored into database)</summary>
    /// <param name="settings">Settings</param>
    /// <param name="keySelector">Key selector</param>
    /// <typeparam name="TSettings">Type of settings</typeparam>
    /// <typeparam name="T">Property type</typeparam>
    /// <returns>Key</returns>
    string GetSettingKey<TSettings, T>(
        TSettings settings,
        Expression<Func<TSettings, T>> keySelector
    )
        where TSettings : ISettings, new();

    /// <summary>
    /// Updates the setting using the specified setting
    /// </summary>
    /// <param name="setting">The setting</param>
    /// <param name="clearCache">The clear cache</param>
    Task UpdateSetting(Setting setting, bool clearCache = true);

    /// <summary>
    /// Updates the by key using the specified key
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value</param>
    /// <returns>A task containing the setting</returns>
    Task<Setting> UpdateByKey(string key, string value);
}
