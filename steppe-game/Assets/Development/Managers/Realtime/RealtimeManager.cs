using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine;

public class RealtimeManager
{
    FirebaseFirestore firestore;

    public void Initialize()
    {
        firestore = FirebaseFirestore.DefaultInstance;
    }

    public async Task SaveUserData(string userName, CharacterSex userSex, int userLevel, int userPoints)
    {
        // Используем словарь вместо UserModel
        var userData = new Dictionary<string, object>
        {
            { "Name", userName },
            { "Sex", (int)userSex },
            { "Level", userLevel },
            { "Points", userPoints }
        };

        await firestore.Collection("Users").Document(userName).SetAsync(userData);
    }

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

    public async Task<List<UserModel>> ReadOtherUsersData(string userName)
    {
        List<UserModel> users = new();

        var snapshot = await firestore.Collection("Users").OrderByDescending("Points").Limit(30).GetSnapshotAsync();

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

        return users;
    }
}