using System.Collections.Generic;
using UnityEngine;

public class SpriteManager
{
    public enum SpriteType { Building, Terrain, UI, Entity };
    private static SpriteManager instance;

    private Dictionary<SpriteType, string> prefixes;
    private Dictionary<string, Sprite> sprites;
    private bool suppress_error_logging;

    private SpriteManager()
    {
        suppress_error_logging = false;
        prefixes = new Dictionary<SpriteType, string>();
        sprites = new Dictionary<string, Sprite>();

        prefixes.Add(SpriteType.Building, "building");
        prefixes.Add(SpriteType.Terrain, "terrain");
        prefixes.Add(SpriteType.UI, "ui");
        prefixes.Add(SpriteType.Entity, "entity");

        CustomLogger.Instance.Debug("Loading sprites...");
        foreach (Sprite texture in Resources.LoadAll<Sprite>("images/buildings")) {
            sprites.Add(prefixes[SpriteType.Building] + "_" + texture.name, texture);
            CustomLogger.Instance.Debug("Building sprite loaded: " + texture.name);
        }

        foreach (Sprite texture in Resources.LoadAll<Sprite>("images/terrain")) {
            sprites.Add(prefixes[SpriteType.Terrain] + "_" + texture.name, texture);
            CustomLogger.Instance.Debug("Terrain sprite loaded: " + texture.name);
        }

        foreach (Sprite texture in Resources.LoadAll<Sprite>("images/ui")) {
            sprites.Add(prefixes[SpriteType.UI] + "_" + texture.name, texture);
            CustomLogger.Instance.Debug("UI sprite loaded: " + texture.name);
        }

        foreach (Sprite texture in Resources.LoadAll<Sprite>("images/entities")) {
            sprites.Add(prefixes[SpriteType.Entity] + "_" + texture.name, texture);
            CustomLogger.Instance.Debug("Entity sprite loaded: " + texture.name);
        }

        CustomLogger.Instance.Debug("All sprites loaded");
    }

    /// <summary>
    /// Accessor for singleton instance
    /// </summary>
    public static SpriteManager Instance
    {
        get {
            if (instance == null) {
                instance = new SpriteManager();
            }
            return instance;
        }
    }

    /// <summary>
    /// Get sprite by name and type
    /// </summary>
    /// <param name="sprite_name"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public Sprite Get(string sprite_name, SpriteType type, bool transparent = false)
    {
        if (transparent && !sprite_name.EndsWith("_transparent")) {
            sprite_name += "_transparent";
        }
        if (sprites.ContainsKey(prefixes[type] + "_" + sprite_name)) {
            return sprites[prefixes[type] + "_" + sprite_name];
        }
        if (!suppress_error_logging) {
            CustomLogger.Instance.Warning("Sprite " + prefixes[type] + "_" + sprite_name + " does not exist!");
        }
        return null;
    }

    public Sprite Get(IHasSprite obj)
    {
        return Get(obj.Sprite_Name, obj.Sprite_Type);
    }
}

public interface IHasSprite
{
    string Sprite_Name { get; }
    SpriteManager.SpriteType Sprite_Type { get; }
}
