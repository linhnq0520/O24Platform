using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using O24OpenAPI.Core;
using O24OpenAPI.Core.Caching;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Extensions;
using O24OpenAPI.Core.Helper;
using O24OpenAPI.Core.SeedWork;
using O24OpenAPI.Data.System.Linq;
using O24OpenAPI.Framework.Models;

namespace O24OpenAPI.Framework.Services.Configuration;

public class SettingService(
    IRepository<Setting> settingRepository,
    IStaticCacheManager staticCacheManager
) : ISettingService
{
    private readonly IRepository<Setting> _settingRepository = settingRepository;
    private readonly IStaticCacheManager _staticCacheManager = staticCacheManager;

    protected virtual async Task<IDictionary<string, IList<Setting>>> GetAllSettingsDictionary()
    {
        Dictionary<string, IList<Setting>> settingsDictionary =
            await _staticCacheManager.GetOrSetAsync(
                Setting.GetCacheKeyAll(),
                async () =>
                {
                    IList<Setting> settings = await GetAllSettings();
                    Dictionary<string, IList<Setting>> dictionary = [];
                    foreach (Setting s in (IEnumerable<Setting>)settings)
                    {
                        string resourceName = s.Name.ToLowerInvariant();
                        Setting settingForCaching = new()
                        {
                            Id = s.Id,
                            Name = s.Name,
                            Value = s.Value,
                            OrganizationId = s.OrganizationId,
                        };
                        if (!dictionary.TryGetValue(resourceName, out IList<Setting> value))
                        {
                            dictionary.Add(resourceName, [settingForCaching]);
                        }
                        else
                        {
                            value.Add(settingForCaching);
                        }

                        resourceName = null;
                        settingForCaching = null;
                    }

                    Dictionary<string, IList<Setting>> settingsDictionary1 = dictionary;
                    settings = null;
                    dictionary = null;
                    return settingsDictionary1;
                }
            );
        return settingsDictionary;
    }

    protected virtual async Task SetSetting(
        Type type,
        string key,
        object value,
        int organizationId = 0,
        bool clearCache = true
    )
    {
        key =
            key != null
                ? key.Trim().ToLowerInvariant()
                : throw new ArgumentNullException(nameof(key));
        string valueStr = TypeDescriptor.GetConverter(type).ConvertToInvariantString(value);
        IDictionary<string, IList<Setting>> allSettings = await GetAllSettingsDictionary();
        Setting settingForCaching = allSettings.TryGetValue(key, out IList<Setting> settings)
            ? settings.FirstOrDefault(x => x.OrganizationId == organizationId)
            : null;
        if (settingForCaching != null)
        {
            Setting setting = await GetSettingById(settingForCaching.Id);
            setting.Value = valueStr;
            setting.OrganizationId = organizationId;
            await UpdateSetting(setting, clearCache);
            setting = null;
            valueStr = null;
            allSettings = null;
            settingForCaching = null;
        }
        else
        {
            Setting setting = new()
            {
                Name = key,
                Value = valueStr,
                OrganizationId = organizationId,
            };
            await InsertSetting(setting, clearCache);
            setting = null;
            valueStr = null;
            allSettings = null;
            settingForCaching = null;
        }
    }

    /// <summary>
    /// Inserts the setting using the specified setting
    /// </summary>
    /// <param name="setting">The setting</param>
    /// <param name="clearCache">The clear cache</param>
    public virtual async Task InsertSetting(Setting setting, bool clearCache = true)
    {
        await _settingRepository.InsertAsync(setting);
        if (!clearCache)
        {
            return;
        }

        await ClearCache();
    }

    /// <summary>
    /// Updates the setting using the specified setting
    /// </summary>
    /// <param name="setting">The setting</param>
    /// <param name="clearCache">The clear cache</param>
    public virtual async Task UpdateSetting(Setting setting, bool clearCache = true)
    {
        ArgumentNullException.ThrowIfNull(setting);
        await _settingRepository.Update(setting);
        if (!clearCache)
        {
            return;
        }

        await ClearCache();
    }

    /// <summary>
    /// Deletes the setting using the specified setting
    /// </summary>
    /// <param name="setting">The setting</param>
    public virtual async Task DeleteSetting(Setting setting)
    {
        await _settingRepository.Delete(setting);
        await ClearCache();
    }

    /// <summary>
    /// Deletes the settings using the specified settings
    /// </summary>
    /// <param name="settings">The settings</param>
    public virtual async Task DeleteSettings(IList<Setting> settings)
    {
        await _settingRepository.BulkDelete(settings);
        await ClearCache();
    }

    /// <summary>
    /// Gets the setting by id using the specified setting id
    /// </summary>
    /// <param name="settingId">The setting id</param>
    /// <returns>The by id</returns>
    public virtual async Task<Setting> GetSettingById(int settingId)
    {
        Setting byId = await _settingRepository.GetById(settingId, cache => null);
        return byId;
    }

    /// <summary>
    /// Gets the setting using the specified key
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="organizationId">The organization id</param>
    /// <param name="loadSharedValueIfNotFound">The load shared value if not found</param>
    /// <returns>The setting</returns>
    public virtual async Task<Setting> GetSetting(
        string key,
        int organizationId = 0,
        bool loadSharedValueIfNotFound = false
    )
    {
        if (string.IsNullOrEmpty(key))
        {
            return null;
        }

        IDictionary<string, IList<Setting>> settings = await GetAllSettingsDictionary();
        key = key.Trim().ToLowerInvariant();
        if (!settings.TryGetValue(key, out IList<Setting> settingsByKey))
        {
            return null;
        }

        Setting setting1 = settingsByKey.FirstOrDefault(x => x.OrganizationId == organizationId);
        if (
            (
                (setting1 != null ? 0 : (organizationId > 0 ? 1 : 0))
                & (loadSharedValueIfNotFound ? 1 : 0)
            ) != 0
        )
        {
            setting1 = settingsByKey.FirstOrDefault(x => x.OrganizationId == 0);
        }

        Setting setting2;
        if (setting1 != null)
        {
            setting2 = await GetSettingById(setting1.Id);
        }
        else
        {
            setting2 = null;
        }

        return setting2;
    }

    /// <summary>
    /// Gets the setting by key using the specified key
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="key">The key</param>
    /// <param name="defaultValue">The default value</param>
    /// <param name="organizationId">The organization id</param>
    /// <param name="loadSharedValueIfNotFound">The load shared value if not found</param>
    /// <returns>A task containing the</returns>
    public virtual async Task<T> GetSettingByKey<T>(
        string key,
        T defaultValue,
        int organizationId = 0,
        bool loadSharedValueIfNotFound = false
    )
    {
        if (string.IsNullOrEmpty(key))
        {
            return defaultValue;
        }

        IDictionary<string, IList<Setting>> settings = await GetAllSettingsDictionary();
        key = key.Trim().ToLowerInvariant();
        if (!settings.TryGetValue(key, out IList<Setting> value))
        {
            return defaultValue;
        }

        IList<Setting> settingsByKey = value;
        Setting setting = settingsByKey.FirstOrDefault(x => x.OrganizationId == organizationId);
        if (
            (
                (setting != null ? 0 : (organizationId > 0 ? 1 : 0))
                & (loadSharedValueIfNotFound ? 1 : 0)
            ) != 0
        )
        {
            setting = settingsByKey.FirstOrDefault(x => x.OrganizationId == 0);
        }

        return setting != null ? CommonHelper.To<T>(setting.Value) : defaultValue;
    }

    /// <summary>
    /// Sets the setting using the specified key
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="key">The key</param>
    /// <param name="value">The value</param>
    /// <param name="organizationId">The organization id</param>
    /// <param name="clearCache">The clear cache</param>
    public virtual async Task SetSetting<T>(
        string key,
        T value,
        int organizationId = 0,
        bool clearCache = true
    )
    {
        await SetSetting(typeof(T), key, value, organizationId, clearCache);
    }

    /// <summary>
    /// Gets the all settings
    /// </summary>
    /// <returns>A task containing a list of setting</returns>
    public virtual async Task<IList<Setting>> GetAllSettings()
    {
        return await _settingRepository.Table.ToListAsync();
    }

    /// <summary>
    /// Searches the common using the specified model
    /// </summary>
    /// <param name="model">The model</param>
    /// <returns>The setting list</returns>
    public virtual async Task<IList<Setting>> SearchCommon(SimpleSearchModel model)
    {
        IPagedList<Setting> settings = await _settingRepository.GetAllPaged(
            query =>
                query
                    .Where(s =>
                        s.Name.Contains(
                            model.SearchText,
                            StringComparison.InvariantCultureIgnoreCase
                        )
                        || s.Value.Contains(
                            model.SearchText,
                            StringComparison.InvariantCultureIgnoreCase
                        )
                    )
                    .OrderBy(s => s.Name),
            model.PageIndex,
            model.PageSize
        );
        IList<Setting> settingList = settings;
        settings = null;
        return settingList;
    }

    /// <summary>
    /// Searches the advance using the specified model
    /// </summary>
    /// <param name="model">The model</param>
    /// <returns>The setting list</returns>
    public virtual async Task<IList<Setting>> SearchAdvance(AdvanceSearchParamModel model)
    {
        IPagedList<Setting> settings = await _settingRepository.GetAllPaged(
            query =>
            {
                if (!string.IsNullOrEmpty(model.Name))
                {
                    // ISSUE: reference to a compiler-generated field
                    query = query.Where(s =>
                        s.Name.Contains(model.Name, StringComparison.InvariantCultureIgnoreCase)
                    );
                }

                if (!string.IsNullOrEmpty(model.Value))
                {
                    // ISSUE: reference to a compiler-generated field
                    query = query.Where(s =>
                        s.Value.Contains(model.Name, StringComparison.InvariantCultureIgnoreCase)
                    );
                }

                query = query.OrderBy(s => s.Name);
                return query;
            },
            model.PageIndex,
            model.PageSize
        );
        IList<Setting> settingList = settings;
        settings = null;
        return settingList;
    }

    /// <summary>
    /// Gets the key
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="key">The key</param>
    /// <param name="defaultValue">The default value</param>
    /// <returns>A task containing the</returns>
    public virtual async Task<T> Get<T>(string key, T defaultValue)
    {
        Setting setting = await _settingRepository
            .Table.Where(s => s.Name.Equals(key, StringComparison.CurrentCultureIgnoreCase))
            .FirstOrDefaultAsync();
        return (setting != null) ? CommonHelper.To<T>(setting.Value) : defaultValue;
    }

    /// <summary>
    /// Settings the exists using the specified settings
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <typeparam name="TPropType">The prop type</typeparam>
    /// <param name="settings">The settings</param>
    /// <param name="keySelector">The key selector</param>
    /// <param name="organizationId">The organization id</param>
    /// <returns>The flag</returns>
    public virtual async Task<bool> SettingExists<T, TPropType>(
        T settings,
        Expression<Func<T, TPropType>> keySelector,
        int organizationId = 0
    )
        where T : ISettings, new()
    {
        string key = GetSettingKey(settings, keySelector);
        string setting = await GetSettingByKey<string>(key, null, organizationId, false);
        bool flag = setting != null;
        return flag;
    }

    /// <summary>
    /// Loads the setting using the specified organization id
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="organizationId">The organization id</param>
    /// <returns>A task containing the</returns>
    public virtual async Task<T> LoadSetting<T>(int organizationId = 0)
        where T : ISettings, new()
    {
        ISettings settings = await LoadSetting(typeof(T), organizationId);
        return (T)settings;
    }

    /// <summary>
    /// Loads the setting using the specified type
    /// </summary>
    /// <param name="type">The type</param>
    /// <param name="organizationId">The organization id</param>
    /// <returns>The settings</returns>
    public virtual async Task<ISettings> LoadSetting(Type type, int organizationId = 0)
    {
        object settings = Activator.CreateInstance(type);
        try
        {
            PropertyInfo[] propertyInfoArray = type.GetProperties();
            for (int index = 0; index < propertyInfoArray.Length; ++index)
            {
                PropertyInfo prop = propertyInfoArray[index];
                if (prop.CanRead && prop.CanWrite)
                {
                    string key = type.Name + "." + prop.Name;
                    string setting = await GetSettingByKey<string>(key, null, organizationId, true);
                    if (
                        setting != null
                        && TypeDescriptor
                            .GetConverter(prop.PropertyType)
                            .CanConvertFrom(typeof(string))
                        && TypeDescriptor.GetConverter(prop.PropertyType).IsValid(setting)
                    )
                    {
                        object value = TypeDescriptor
                            .GetConverter(prop.PropertyType)
                            .ConvertFromInvariantString(setting);
                        prop.SetValue(settings, value, (object[])null);
                        key = null;
                        setting = null;
                        value = null;
                        prop = null;
                    }
                    else
                    {
                        try
                        {
                            object value = JsonSerializer.Deserialize(setting, prop.PropertyType);
                            prop.SetValue(settings, value);
                        }
                        catch { }
                    }
                }
            }

            propertyInfoArray = null;
        }
        catch { }

        ISettings settings1 = settings as ISettings;
        return settings1;
    }

    /// <summary>
    /// Saves the setting using the specified settings
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="settings">The settings</param>
    /// <param name="organizationId">The organization id</param>
    public virtual async Task SaveSetting<T>(T settings, int organizationId = 0)
        where T : ISettings, new()
    {
        PropertyInfo[] propertyInfoArray = typeof(T).GetProperties();
        for (int index = 0; index < propertyInfoArray.Length; ++index)
        {
            PropertyInfo prop = propertyInfoArray[index];
            if (
                prop.CanRead
                && prop.CanWrite
                && TypeDescriptor.GetConverter(prop.PropertyType).CanConvertFrom(typeof(string))
            )
            {
                string key = typeof(T).Name + "." + prop.Name;
                object value = prop.GetValue(settings, (object[])null);
                if (value != null)
                {
                    await SetSetting(prop.PropertyType, key, value, organizationId, false);
                }
                else
                {
                    await SetSetting(key, string.Empty, organizationId, false);
                }
            }
        }

        await ClearCache();
    }

    /// <summary>
    /// Saves the setting using the specified settings
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <typeparam name="TPropType">The prop type</typeparam>
    /// <param name="settings">The settings</param>
    /// <param name="keySelector">The key selector</param>
    /// <param name="organizationId">The organization id</param>
    /// <param name="clearCache">The clear cache</param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public virtual async Task SaveSetting<T, TPropType>(
        T settings,
        Expression<Func<T, TPropType>> keySelector,
        int organizationId = 0,
        bool clearCache = true
    )
        where T : ISettings, new()
    {
        if (!(keySelector.Body is MemberExpression member))
        {
            string errorMessage = $"Expression '{keySelector}' refers to a method, not a property.";
            throw new ArgumentException(errorMessage);
        }

        PropertyInfo propInfo = member.Member as PropertyInfo;
        if (propInfo == null)
        {
            string errorMessage = $"Expression '{keySelector}' refers to a field, not a property.";
            throw new ArgumentException(errorMessage);
        }

        string key = GetSettingKey(settings, keySelector);
        TPropType value = (TPropType)propInfo.GetValue(settings, null);
        if (value != null)
        {
            await SetSetting(key, value, organizationId, clearCache);
        }
        else
        {
            await SetSetting(key, string.Empty, organizationId, clearCache);
        }
    }

    /// <summary>
    /// Deletes the setting
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    public virtual async Task DeleteSetting<T>()
        where T : ISettings, new()
    {
        List<Setting> settingsToDelete = [];
        IList<Setting> allSettings = await GetAllSettings();
        PropertyInfo[] propertyInfoArray = typeof(T).GetProperties();
        for (int index = 0; index < propertyInfoArray.Length; ++index)
        {
            PropertyInfo prop = propertyInfoArray[index];
            string key = typeof(T).Name + "." + prop.Name;
            settingsToDelete.AddRange(
                allSettings.Where(x =>
                    x.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase)
                )
            );
            prop = null;
        }

        propertyInfoArray = null;
        await DeleteSettings(settingsToDelete);
        settingsToDelete = null;
        allSettings = null;
    }

    /// <summary>
    /// Deletes the setting using the specified settings
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <typeparam name="TPropType">The prop type</typeparam>
    /// <param name="settings">The settings</param>
    /// <param name="keySelector">The key selector</param>
    /// <param name="organizationId">The organization id</param>
    public virtual async Task DeleteSetting<T, TPropType>(
        T settings,
        Expression<Func<T, TPropType>> keySelector,
        int organizationId = 0
    )
        where T : ISettings, new()
    {
        string key = GetSettingKey(settings, keySelector);
        key = key.Trim().ToLowerInvariant();
        IDictionary<string, IList<Setting>> allSettings = await GetAllSettingsDictionary();
        Setting settingForCaching = allSettings.ContainsKey(key)
            ? allSettings[key].FirstOrDefault(x => x.OrganizationId == organizationId)
            : null;
        Setting setting;
        if (settingForCaching == null)
        {
            key = null;
            allSettings = null;
            settingForCaching = null;
            setting = null;
        }
        else
        {
            setting = await GetSettingById(settingForCaching.Id);
            await DeleteSetting(setting);
            key = null;
            allSettings = null;
            settingForCaching = null;
            setting = null;
        }
    }

    /// <summary>
    /// Clears the cache
    /// </summary>
    public virtual async Task ClearCache()
    {
        await _staticCacheManager.Remove(CachingKey.EntityKeyWithService<Setting>("all"));
        await _staticCacheManager.RemoveByPrefix(O24OpenAPIEntityCacheDefaults<Setting>.Prefix);
    }

    /// <summary>
    /// Gets the setting key using the specified settings
    /// </summary>
    /// <typeparam name="TSettings">The settings</typeparam>
    /// <typeparam name="T">The </typeparam>
    /// <param name="settings">The settings</param>
    /// <param name="keySelector">The key selector</param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <returns>The string</returns>
    public virtual string GetSettingKey<TSettings, T>(
        TSettings settings,
        Expression<Func<TSettings, T>> keySelector
    )
        where TSettings : ISettings, new()
    {
        if (keySelector.Body is not MemberExpression body)
        {
            DefaultInterpolatedStringHandler interpolatedStringHandler = new(50, 1);
            interpolatedStringHandler.AppendLiteral("Expression '");
            interpolatedStringHandler.AppendFormatted(keySelector);
            interpolatedStringHandler.AppendLiteral("' refers to a methods, not a property.");
            throw new ArgumentException(interpolatedStringHandler.ToStringAndClear());
        }

        if (body.Member is not PropertyInfo member)
        {
            DefaultInterpolatedStringHandler interpolatedStringHandler = new(48, 1);
            interpolatedStringHandler.AppendLiteral("Expression '");
            interpolatedStringHandler.AppendFormatted(keySelector);
            interpolatedStringHandler.AppendLiteral("' refers to a field, not a property.");
            throw new ArgumentException(interpolatedStringHandler.ToStringAndClear());
        }

        return typeof(TSettings).Name + "." + member.Name;
    }

    /// <summary>
    /// Updates the by key using the specified key
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value</param>
    /// <returns>The setting</returns>
    public async Task<Setting> UpdateByKey(string key, string value)
    {
        Setting setting = await GetSetting(key);
        setting.Value = value;
        await UpdateSetting(setting);
        return setting;
    }

    /// <summary>
    /// GetById
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public virtual async Task<Setting> GetById(int id)
    {
        return await _settingRepository.GetById(id);
    }

    /// <summary>
    /// GetByPrimaryKey
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public virtual async Task<Setting> GetByPrimaryKey(string name)
    {
        Setting query = await _settingRepository
            .Table.Where(s => s.Name == name)
            .FirstOrDefaultAsync();
        return query;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public virtual async Task<Setting> View(int id)
    {
        Setting getSetting =
            await _settingRepository.GetById(id)
            ?? throw new O24OpenAPIException("CMS.Setting.Value.NotFound");
        return getSetting;
    }

    /// <summary>
    /// Simple Search
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public virtual async Task<IPagedList<Setting>> Search(SimpleSearchModel model)
    {
        IQueryable<Setting> viewTable = _settingRepository.Table.Where(s =>
            s.Name.Contains(model.SearchText, StringComparison.OrdinalIgnoreCase)
            || s.Value.Contains(model.SearchText, StringComparison.OrdinalIgnoreCase)
            || s.OrganizationId.ToString().Equals(model.SearchText)
        );

        IPagedList<Setting> result = await viewTable.ToPagedList(model.PageIndex, model.PageSize);
        return result;
    }

    /// <summary>
    /// Advance Search
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public virtual async Task<IPagedList<Setting>> Search(SettingSearchModel model)
    {
        IQueryable<Setting> query = _settingRepository.Table;
        if (model.Name.HasValue())
        {
            query = query.Where(s =>
                s.Name.Contains(model.Name, StringComparison.OrdinalIgnoreCase)
            );
        }
        if (model.Value.HasValue())
        {
            query = query.Where(s =>
                s.Value.Contains(model.Value, StringComparison.OrdinalIgnoreCase)
            );
        }
        if (model.OrganizationId.HasValue())
        {
            query = query.Where(s => s.OrganizationId.ToString() == model.OrganizationId);
        }
        return await query.ToPagedList(model.PageIndex, model.PageSize);
    }

    /// <summary>
    /// Create
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public virtual async Task<Setting> Create(SettingCreateModel model)
    {
        Setting getSetting = await GetByPrimaryKey(model.Name);
        if (getSetting != null)
        {
            throw new O24OpenAPIException("CMS.Setting.Value.Exist");
        }

        Setting newModel = new()
        {
            Name = model.Name,
            Value = model.Value,
            OrganizationId = model.OrganizationId,
        };
        await Insert(newModel, model.ReferenceId);
        await _staticCacheManager.ClearByKey(Setting.GetCacheKeyAll());
        getSetting = await GetByPrimaryKey(newModel.Name);
        // await _staticCacheManager.RemoveByPrefix(NeptuneEntityCacheDefaults<Setting>.Prefix);
        // var allSettingKey = Framework
        //     .Services
        //     .Configuration
        //     .NeptuneConfigurationDefaults
        //     .SettingsAllAsDictionaryCacheKey;
        // await _staticCacheManager.RemoveByPrefix(allSettingKey.Key);
        // await _settingService.ClearCache();
        return getSetting;
    }

    /// <summary>
    /// Insert
    /// </summary>
    /// <param name="value"></param>
    /// <param name="referenceId"></param>
    /// <returns></returns>
    public virtual async Task Insert(Setting value, string referenceId = "")
    {
        await _settingRepository.InsertAsync(value);
    }

    /// <summary>
    /// Update
    /// </summary>
    /// <param name="model"></param>
    /// <param name="referenceId"></param>
    /// <returns></returns>
    public virtual async Task<SettingUpdateModel> Update(
        SettingUpdateModel model,
        string referenceId = ""
    )
    {
        Setting getSetting =
            await GetById(model.Id) ?? throw new O24OpenAPIException("Setting not found");

        // convert
        getSetting.Name = model.Name;
        getSetting.Value = model.Value;
        getSetting.OrganizationId = model.OrganizationId;

        await _settingRepository.Update(getSetting);
        await _staticCacheManager.ClearByKey(Setting.GetCacheKeyAll());

        // await _staticCacheManager.RemoveByPrefix(NeptuneEntityCacheDefaults<Setting>.Prefix);
        // var allSettingKey = Framework
        //     .Services
        //     .Configuration
        //     .NeptuneConfigurationDefaults
        //     .SettingsAllAsDictionaryCacheKey;
        // await _staticCacheManager.RemoveByPrefix(allSettingKey.Key);
        // await _settingService.ClearCache();
        return model;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    /// <param name="referenceId"></param>
    /// <returns></returns>
    public virtual async Task<Setting> Delete(int id, string referenceId = "")
    {
        Setting getSetting = await GetById(id);
        if (getSetting == null)
        {
            throw new O24OpenAPIException("CMS.Setting.Value.NotFound");
        }

        await _settingRepository.Delete(getSetting);
        await _staticCacheManager.ClearByKey(Setting.GetCacheKeyAll());

        // await _staticCacheManager.RemoveByPrefix(NeptuneEntityCacheDefaults<Setting>.Prefix);
        // var allSettingKey = Framework
        //     .Services
        //     .Configuration
        //     .NeptuneConfigurationDefaults
        //     .SettingsAllAsDictionaryCacheKey;
        // await _staticCacheManager.RemoveByPrefix(allSettingKey.Key);
        // await _settingService.ClearCache();
        return getSetting;
    }

    /// <summary>
    /// Get by name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public virtual async Task<Setting> GetByNameAsync(string name)
    {
        Setting setting = await _settingRepository
            .Table.Where(s => s.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
            .FirstOrDefaultAsync();
        return setting;
    }
}
