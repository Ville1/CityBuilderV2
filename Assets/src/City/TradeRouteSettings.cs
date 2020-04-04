using System.Linq;

public class TradeRouteSettings {
    public static readonly int CARAVAN_INTERVAL = 10;

    public enum TradeAction { Buy, Sell }

    public ForeignCity Partner { get; set; }
    public TradeAction Action { get; set; }
    public Resource Resource { get; set; }
    public float Amount { get; set; }
    public float Caravan_Cooldown { get; set; }
    public Building Building { get; set; }
    public bool Set { get { return Partner != null && Resource != null && Amount != 0.0f; } }
    public float Effective_Amount { get { return Building.Efficency >= 1.0f ? Amount : Amount * Building.Efficency; } }

    public TradeRouteSettings(Building building)
    {
        Partner = null;
        Action = TradeAction.Buy;
        Resource = null;
        Amount = 0.0f;
        Caravan_Cooldown = CARAVAN_INTERVAL;
        Building = building;
    }

    public TradeRouteSettings(Building building, TradeRouteSettingsSaveData data)
    {
        Building = building;
        bool defaults = false;
        if (data.Partner < 0) {
            defaults = true;
        } else {
            Partner = Contacts.Instance.Cities.FirstOrDefault(x => x.Id == data.Partner);
            if (Partner == null) {
                CustomLogger.Instance.Error(string.Format("City not found: #{0}", data.Partner));
                defaults = true;
            } else {
                Action = (TradeAction)data.Action;
                Resource = data.Resource < 0 ? null : Resource.All.First(x => (int)x.Type == data.Resource);
                Amount = data.Amount;
                Caravan_Cooldown = data.Caravan_Cooldown;
            }
        }
        if (defaults) {
            Partner = null;
            Action = TradeAction.Buy;
            Resource = null;
            Amount = 0.0f;
            Caravan_Cooldown = CARAVAN_INTERVAL;
        }
    }

    public TradeRouteSettings Clone()
    {
        return new TradeRouteSettings(Building) {
            Partner = Partner,
            Action = Action,
            Resource = Resource,
            Amount = Amount
        };
    }

    public void Apply(TradeRouteSettings settings)
    {
        Partner = settings.Partner;
        Action = settings.Action;
        Resource = settings.Resource;
        Amount = settings.Amount;
    }
    
    public float Cash_Delta
    {
        get {
            if(!Set) {
                return 0.0f;
            }
            return (Action == TradeAction.Buy ? (-1.0f) : (1.0f)) * ((Effective_Amount * (Action == TradeAction.Buy ? Partner.Get_Export_Price(Resource) : Partner.Get_Import_Price(Resource))) / CARAVAN_INTERVAL);
        }
    }

    public float Resource_Delta
    {
        get {
            if (!Set) {
                return 0.0f;
            }
            return (Action == TradeAction.Buy ? (1.0f) : (-1.0f)) * (Effective_Amount / CARAVAN_INTERVAL);
        }
    }

    public void Validate()
    {
        if (Set) {
            if(Amount < 0.0f) {
                Amount = 1.0f;
            } else if(Amount > Building.INPUT_OUTPUT_STORAGE_LIMIT) {
                Amount = Building.INPUT_OUTPUT_STORAGE_LIMIT;
            }
            if(Action == TradeAction.Buy && (!Partner.Cheap_Exports.Contains(Resource) && !Partner.Exports.Contains(Resource) && !Partner.Expensive_Exports.Contains(Resource))) {
                Resource = null;
            } else if(Action == TradeAction.Sell && Partner.Unaccepted_Imports.Contains(Resource)) {
                Resource = null;
            }
        }
    }

    public TradeRouteSettingsSaveData Save_Data
    {
        get {
            return new TradeRouteSettingsSaveData() {
                Partner = Partner == null ? -1 : Partner.Id,
                Action = (int)Action,
                Resource = Resource == null ? -1 : (int)Resource.Type,
                Amount = Amount,
                Caravan_Cooldown = Caravan_Cooldown
            };
        }
    }
}
