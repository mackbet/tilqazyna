public class UserModel
{
    public string Name { get; set; }
    public int Level { get; set; }
    public int Points { get; set; }
    public int Sex { get; set; }

    public UserModel(string modelName, CharacterSex modelSex, int modelLevel, int modelPoints)
    {
        Name = modelName;
        Sex = (int)modelSex;
        Level = modelLevel;
        Points = modelPoints;
    }

    public CharacterSex GetSexAsEnum()
    {
        return (CharacterSex)Sex;
    }
}