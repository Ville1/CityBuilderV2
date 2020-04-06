using System.Collections.Generic;
using System.Linq;

public class Contacts {
    private static Contacts instance;

    public static readonly int MIN_CONTACTS = 8;
    public static readonly int MAX_CONTACTS = 12;

    public List<ForeignCity> Cities { get; private set; }

    private Contacts()
    {
        Cities = new List<ForeignCity>();
    }

    public static Contacts Instance
    {
        get {
            if(instance == null) {
                instance = new Contacts();
            }
            return instance;
        }
    }

    public void Generate_New()
    {
        Cities.Clear();
        for(int i = 0; i < RNG.Instance.Next(MIN_CONTACTS, MAX_CONTACTS); i++) {
            Cities.Add(new ForeignCity());
        }
    }

    public void Update(float delta_time)
    {
        if (!TimeManager.Instance.Paused) {
            foreach (ForeignCity city in Cities) {
                city.Update(delta_time);
            }
        }
    }

    public ContactsSaveData Save()
    {
        ContactsSaveData data = new ContactsSaveData();
        data.Cities = new List<ForeignCitySaveData>();
        foreach(ForeignCity city in Cities) {
            data.Cities.Add(new ForeignCitySaveData() {
                Id = city.Id,
                Name = city.Name,
                Opinion = city.Opinion,
                Opinion_Resting_Point = city.Opinion_Resting_Point,
                City_Type = (int)city.City_Type,
                Trade_Route_Type = (int)city.Trade_Route_Type,
                Preferred_Imports = city.Preferred_Imports.Select(x => (int)x.Type).ToList(),
                Disliked_Imports = city.Disliked_Imports.Select(x => (int)x.Type).ToList(),
                Unaccepted_Imports = city.Unaccepted_Imports.Select(x => (int)x.Type).ToList(),
                Exports = city.Exports.Select(x => (int)x.Type).ToList(),
                Cheap_Exports = city.Cheap_Exports.Select(x => (int)x.Type).ToList(),
                Expensive_Exports = city.Expensive_Exports.Select(x => (int)x.Type).ToList()
            });
        }
        return data;
    }

    public void Load(ContactsSaveData data)
    {
        ForeignCity.Reset_Current_Id();
        Cities.Clear();
        foreach(ForeignCitySaveData city_data in data.Cities) {
            Cities.Add(new ForeignCity(city_data));
        }
    }
}
