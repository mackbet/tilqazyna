using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine;

public class RealtimeManager
{
    private FirebaseFirestore firestore;

    public void Initialize()
    {
        firestore = FirebaseFirestore.DefaultInstance;
    }

    /// <summary>
    /// Сохранить данные пользователя (использует userId для идентификации)
    /// </summary>
    public async Task SaveUserData(string userName, CharacterSex userSex, int userLevel, int userPoints)
    {
        string odl = AuthenticationManager.Instance?.GetUserId();

        if (string.IsNullOrEmpty(odl))
        {
            Debug.LogWarning("RealtimeManager: userId не найден, используем старый метод");
            await SaveUserDataLegacy(userName, userSex, userLevel, userPoints);
            return;
        }

        var userData = new Dictionary<string, object>
        {
            { "Name", userName },
            { "Sex", (int)userSex },
            { "Level", userLevel },
            { "Points", userPoints },
            { "UpdatedAt", FieldValue.ServerTimestamp }
        };

        try
        {
            await firestore.Collection("Users").Document(odl).SetAsync(userData, SetOptions.MergeAll);
            Debug.Log($"Данные сохранены для userId: {odl}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка сохранения данных: {e.Message}");
        }
    }

    /// <summary>
    /// Старый метод сохранения (для обратной совместимости)
    /// </summary>
    private async Task SaveUserDataLegacy(string userName, CharacterSex userSex, int userLevel, int userPoints)
    {
        var userData = new Dictionary<string, object>
        {
            { "Name", userName },
            { "Sex", (int)userSex },
            { "Level", userLevel },
            { "Points", userPoints }
        };

        await firestore.Collection("Users").Document(userName).SetAsync(userData);
    }

    /// <summary>
    /// Прочитать данные текущего пользователя
    /// </summary>
    public async Task<UserModel> ReadCurrentUserData()
    {
        string userId = AuthenticationManager.Instance?.GetUserId();

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("RealtimeManager: userId не найден");
            return null;
        }

        return await ReadUserDataById(userId);
    }

    /// <summary>
    /// Прочитать данные пользователя по userId
    /// </summary>
    public async Task<UserModel> ReadUserDataById(string userId)
    {
        try
        {
            var snapshot = await firestore.Collection("Users").Document(userId).GetSnapshotAsync();

            if (snapshot.Exists)
            {
                var data = snapshot.ToDictionary();

                var userModel = new UserModel(
                    data.ContainsKey("Name") ? data["Name"].ToString() : "Unknown",
                    data.ContainsKey("Sex") ? (CharacterSex)Convert.ToInt32(data["Sex"]) : CharacterSex.Boy,
                    data.ContainsKey("Level") ? Convert.ToInt32(data["Level"]) : 0,
                    data.ContainsKey("Points") ? Convert.ToInt32(data["Points"]) : 0
                );

                Debug.Log($"Данные загружены: {userModel.Name}");
                return userModel;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка чтения данных: {e.Message}");
        }

        return null;
    }

    /// <summary>
    /// Прочитать данные пользователя по имени (старый метод)
    /// </summary>
    public async Task<UserModel> ReadUserData(string userName)
    {
        var snapshot = await firestore.Collection("Users").Document(userName).GetSnapshotAsync();

        if (snapshot.Exists)
        {
            var data = snapshot.ToDictionary();

            var userModel = new UserModel(
                data["Name"].ToString(),
                (CharacterSex)Convert.ToInt32(data["Sex"]),
                Convert.ToInt32(data["Level"]),
                Convert.ToInt32(data["Points"])
            );

            Debug.Log("Name: " + userModel.Name);
            return userModel;
        }

        return null;
    }

    /// <summary>
    /// Получить список пользователей для лидерборда
    /// </summary>
    public async Task<List<UserModel>> ReadOtherUsersData(string excludeUserName)
    {
        List<UserModel> users = new();

        try
        {
            var snapshot = await firestore.Collection("Users")
                .OrderByDescending("Points")
                .Limit(30)
                .GetSnapshotAsync();

            foreach (var doc in snapshot.Documents)
            {
                var data = doc.ToDictionary();

                var userModel = new UserModel(
                    data.ContainsKey("Name") ? data["Name"].ToString() : "Unknown",
                    data.ContainsKey("Sex") ? (CharacterSex)Convert.ToInt32(data["Sex"]) : CharacterSex.Boy,
                    data.ContainsKey("Level") ? Convert.ToInt32(data["Level"]) : 0,
                    data.ContainsKey("Points") ? Convert.ToInt32(data["Points"]) : 0
                );

                if (userModel.Name != excludeUserName)
                {
                    users.Add(userModel);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка загрузки лидерборда: {e.Message}");
        }

        return users;
    }

    /// <summary>
    /// Проверить, существует ли пользователь в базе
    /// </summary>
    public async Task<bool> UserExists()
    {
        string userId = AuthenticationManager.Instance?.GetUserId();

        if (string.IsNullOrEmpty(userId))
            return false;

        try
        {
            var snapshot = await firestore.Collection("Users").Document(userId).GetSnapshotAsync();
            return snapshot.Exists;
        }
        catch
        {
            return false;
        }
    }
}
