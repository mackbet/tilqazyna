using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models.Data.Player;
using UnityEngine;
using SaveOptions = Unity.Services.CloudSave.Models.Data.Player.SaveOptions;
using DeleteOptions = Unity.Services.CloudSave.Models.Data.Player.DeleteOptions;

public class RealtimeManager
{
    public static RealtimeManager Instance { get; private set; }

    // Ключи для Cloud Save
    private const string KEY_NAME = "playerName";
    private const string KEY_SEX = "playerSex";
    private const string KEY_EXPERIENCE = "playerExperience";
    private const string KEY_POINTS = "playerPoints";
    private const string KEY_COINS = "playerCoins";

    public void Initialize()
    {
        Instance = this;
        Debug.Log("[CloudSave] RealtimeManager инициализирован");
    }

    /// <summary>
    /// Сохранить данные пользователя и отправить в лидерборд
    /// </summary>
    public async Task SaveUserData(string userName, CharacterSex userSex, int userExperience, int userPoints)
    {
        if (!LoginManager.Instance?.IsAuthenticated ?? true)
        {
            Debug.LogWarning("[CloudSave] Пользователь не авторизован, сохранение невозможно");
            return;
        }

        var data = new Dictionary<string, object>
        {
            { KEY_NAME, userName },
            { KEY_SEX, (int)userSex },
            { KEY_EXPERIENCE, userExperience },
            { KEY_POINTS, userPoints }
        };

        try
        {
            await CloudSaveService.Instance.Data.Player.SaveAsync(data);
            Debug.Log($"[CloudSave] Данные сохранены: {userName}, Exp: {userExperience}, Points: {userPoints}");

            // Отправляем данные в лидерборд (уровень рассчитываем на месте)
            if (LeaderboardManager.Instance != null)
            {
                int level = userExperience / 80;
                await LeaderboardManager.Instance.SubmitScore(userPoints, userName, userSex, level);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[CloudSave] Ошибка сохранения: {e.Message}");
        }
    }

    /// <summary>
    /// Сохранить монеты игрока
    /// </summary>
    public async Task SaveCoins(int coins)
    {
        if (!LoginManager.Instance?.IsAuthenticated ?? true)
        {
            Debug.LogWarning("[CloudSave] Пользователь не авторизован");
            return;
        }

        var data = new Dictionary<string, object>
        {
            { KEY_COINS, coins }
        };

        try
        {
            await CloudSaveService.Instance.Data.Player.SaveAsync(data);
            Debug.Log($"[CloudSave] Монеты сохранены: {coins}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[CloudSave] Ошибка сохранения монет: {e.Message}");
        }
    }

    /// <summary>
    /// Сохранить произвольные данные
    /// </summary>
    public async Task SaveData(Dictionary<string, object> data)
    {
        if (!LoginManager.Instance?.IsAuthenticated ?? true)
        {
            Debug.LogWarning("[CloudSave] Пользователь не авторизован");
            return;
        }

        try
        {
            await CloudSaveService.Instance.Data.Player.SaveAsync(data);
            Debug.Log("[CloudSave] Данные сохранены");
        }
        catch (Exception e)
        {
            Debug.LogError($"[CloudSave] Ошибка сохранения: {e.Message}");
        }
    }

    /// <summary>
    /// Прочитать данные текущего пользователя
    /// </summary>
    public async Task<UserModel> ReadCurrentUserData()
    {
        if (!LoginManager.Instance?.IsAuthenticated ?? true)
        {
            Debug.LogWarning("[CloudSave] Пользователь не авторизован");
            return null;
        }

        try
        {
            var keys = new HashSet<string> { KEY_NAME, KEY_SEX, KEY_EXPERIENCE, KEY_POINTS };
            var result = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);

            if (result.Count == 0)
            {
                Debug.Log("[CloudSave] Данные пользователя не найдены");
                return null;
            }

            string name = result.TryGetValue(KEY_NAME, out var nameItem)
                ? nameItem.Value.GetAs<string>()
                : "Unknown";

            int sex = result.TryGetValue(KEY_SEX, out var sexItem)
                ? sexItem.Value.GetAs<int>()
                : 0;

            int experience = result.TryGetValue(KEY_EXPERIENCE, out var expItem)
                ? expItem.Value.GetAs<int>()
                : 0;

            int points = result.TryGetValue(KEY_POINTS, out var pointsItem)
                ? pointsItem.Value.GetAs<int>()
                : 0;

            var userModel = new UserModel(name, (CharacterSex)sex, experience: experience, points: points);
            Debug.Log($"[CloudSave] Данные загружены: {userModel.Name}, Exp: {experience}, Level: {userModel.Level}");
            return userModel;
        }
        catch (Exception e)
        {
            Debug.LogError($"[CloudSave] Ошибка чтения данных: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Загрузить монеты игрока
    /// </summary>
    public async Task<int> LoadCoins()
    {
        if (!LoginManager.Instance?.IsAuthenticated ?? true)
        {
            Debug.LogWarning("[CloudSave] Пользователь не авторизован");
            return 0;
        }

        try
        {
            var keys = new HashSet<string> { KEY_COINS };
            var result = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);

            int coins = result.TryGetValue(KEY_COINS, out var coinsItem)
                ? coinsItem.Value.GetAs<int>()
                : 0;

            Debug.Log($"[CloudSave] Монеты загружены: {coins}");
            return coins;
        }
        catch (Exception e)
        {
            Debug.LogError($"[CloudSave] Ошибка загрузки монет: {e.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Загрузить произвольные данные
    /// </summary>
    public async Task<Dictionary<string, object>> LoadData(HashSet<string> keys)
    {
        if (!LoginManager.Instance?.IsAuthenticated ?? true)
        {
            Debug.LogWarning("[CloudSave] Пользователь не авторизован");
            return null;
        }

        try
        {
            var result = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);
            var data = new Dictionary<string, object>();

            foreach (var kvp in result)
            {
                data[kvp.Key] = kvp.Value.Value.GetAs<object>();
            }

            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"[CloudSave] Ошибка загрузки: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Прочитать данные пользователя по имени (для совместимости)
    /// </summary>
    public async Task<UserModel> ReadUserData(string userName)
    {
        // Cloud Save не поддерживает чтение данных других игроков
        // Возвращаем данные текущего пользователя
        return await ReadCurrentUserData();
    }

    /// <summary>
    /// Прочитать данные пользователя по userId (для совместимости)
    /// </summary>
    public async Task<UserModel> ReadUserDataById(string odl)
    {
        // Cloud Save не поддерживает чтение данных других игроков
        return await ReadCurrentUserData();
    }

    /// <summary>
    /// Получить список пользователей для лидерборда
    /// ВАЖНО: Cloud Save не поддерживает чтение данных других игроков!
    /// Для лидерборда используй Unity Leaderboards
    /// </summary>
    public async Task<List<UserModel>> ReadOtherUsersData(string excludeUserName)
    {
        Debug.LogWarning("[CloudSave] Cloud Save не поддерживает лидерборды. Используйте Unity Leaderboards.");
        // TODO: Реализовать через Unity Leaderboards
        return new List<UserModel>();
    }

    /// <summary>
    /// Проверить, существует ли пользователь в базе
    /// </summary>
    public async Task<bool> UserExists()
    {
        if (!LoginManager.Instance?.IsAuthenticated ?? true)
            return false;

        try
        {
            var keys = new HashSet<string> { KEY_NAME };
            var result = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);
            return result.ContainsKey(KEY_NAME);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Удалить все данные пользователя
    /// </summary>
    public async Task DeleteAllData()
    {
        if (!LoginManager.Instance?.IsAuthenticated ?? true)
        {
            Debug.LogWarning("[CloudSave] Пользователь не авторизован");
            return;
        }

        try
        {
            var keys = new List<string> { KEY_NAME, KEY_SEX, KEY_EXPERIENCE, KEY_POINTS, KEY_COINS };
            var options = new DeleteOptions();

            foreach (var key in keys)
            {
                try
                {
                    await CloudSaveService.Instance.Data.Player.DeleteAsync(key, options);
                }
                catch { }
            }
            Debug.Log("[CloudSave] Все данные удалены");
        }
        catch (Exception e)
        {
            Debug.LogError($"[CloudSave] Ошибка удаления данных: {e.Message}");
        }
    }
}
