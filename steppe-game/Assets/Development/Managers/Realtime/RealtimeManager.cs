using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

#if !UNITY_EDITOR
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
#endif

public class RealtimeManager
{
#if !UNITY_EDITOR
    private FirebaseFirestore firestore;
    private FirebaseApp app;
#endif
    private bool isInitialized = false;

    public async Task Initialize()
    {
#if UNITY_EDITOR
        Debug.LogWarning("[Firebase] Running in Unity Editor. Firebase is disabled. Build to iOS/Android device to test Firebase features.");
        isInitialized = false;
        await Task.CompletedTask;
        return;
#else
        try
        {
            Debug.Log("[Firebase] Starting initialization...");

            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

            if (dependencyStatus == DependencyStatus.Available)
            {
                app = FirebaseApp.DefaultInstance;
                firestore = FirebaseFirestore.DefaultInstance;

                isInitialized = true;
                Debug.Log("[Firebase] ✓ Initialization successful");
            }
            else
            {
                Debug.LogError($"[Firebase] ✗ Could not resolve dependencies: {dependencyStatus}");
                isInitialized = false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[Firebase] ✗ Initialization failed: {e.Message}");
            isInitialized = false;
        }
#endif
    }

    public async Task SaveUserData(string userName, CharacterSex userSex, int userLevel, int userPoints)
    {
#if UNITY_EDITOR
        Debug.LogWarning($"[Firebase] MOCK: Would save user {userName} with {userPoints} points (Editor mode)");
        await Task.CompletedTask;
        return;
#else
        if (!isInitialized)
        {
            Debug.LogError("[Firebase] Not initialized. Cannot save user data.");
            return;
        }

        var userData = new Dictionary<string, object>
        {
            { "Name", userName },
            { "Sex", (int)userSex },
            { "Level", userLevel },
            { "Points", userPoints },
            { "LastUpdated", FieldValue.ServerTimestamp }
        };

        try
        {
            await firestore.Collection("Users").Document(userName).SetAsync(userData);
            Debug.Log($"[Firebase] ✓ User data saved: {userName}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Firebase] ✗ Failed to save user data: {e.Message}");
        }
#endif
    }

    public async Task<UserModel> ReadUserData(string userName)
    {
#if UNITY_EDITOR
        Debug.LogWarning($"[Firebase] MOCK: Would read user {userName} (Editor mode)");
        // Возвращаем mock данные для тестирования в Editor
        await Task.CompletedTask;
        return new UserModel(userName, CharacterSex.Boy, 1, 100);
#else
        if (!isInitialized)
        {
            Debug.LogError("[Firebase] Not initialized. Cannot read user data.");
            return null;
        }

        try
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

                Debug.Log($"[Firebase] ✓ User data loaded: {userModel.Name}");
                return userModel;
            }

            Debug.Log($"[Firebase] User not found: {userName}");
            return null;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Firebase] ✗ Failed to read user data: {e.Message}");
            return null;
        }
#endif
    }

    public async Task<List<UserModel>> ReadOtherUsersData(string userName)
    {
#if UNITY_EDITOR
        Debug.LogWarning($"[Firebase] MOCK: Would read leaderboard (Editor mode)");
        // Возвращаем mock данные для тестирования
        await Task.CompletedTask;
        return new List<UserModel>
        {
            new UserModel("Player1", CharacterSex.Boy, 5, 500),
            new UserModel("Player2", CharacterSex.Girl, 3, 300),
            new UserModel("Player3", CharacterSex.Boy, 2, 200)
        };
#else
        if (!isInitialized)
        {
            Debug.LogError("[Firebase] Not initialized. Cannot read other users.");
            return new List<UserModel>();
        }

        List<UserModel> users = new List<UserModel>();

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
                    data["Name"].ToString(),
                    (CharacterSex)Convert.ToInt32(data["Sex"]),
                    Convert.ToInt32(data["Level"]),
                    Convert.ToInt32(data["Points"])
                );

                if (userModel.Name != userName)
                {
                    users.Add(userModel);
                }
            }

            Debug.Log($"[Firebase] ✓ Loaded {users.Count} users from leaderboard");
            return users;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Firebase] ✗ Failed to read leaderboard: {e.Message}");
            return new List<UserModel>();
        }
#endif
    }

    public bool IsInitialized => isInitialized;
}
