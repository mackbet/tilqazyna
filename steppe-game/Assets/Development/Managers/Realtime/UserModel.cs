public class UserModel
{
    public string Name { get; set; }
    public int Experience { get; set; }
    public int Points { get; set; }
    public int Sex { get; set; }

    // Уровень рассчитывается из опыта
    public int Level => Experience / 80;

    public UserModel(string name, CharacterSex sex, int experience, int points)
    {
        Name = name;
        Sex = (int)sex;
        Experience = experience;
        Points = points;
    }

    public CharacterSex GetSexAsEnum()
    {
        return (CharacterSex)Sex;
    }
}
