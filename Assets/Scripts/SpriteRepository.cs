using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

// See also: http://answers.unity3d.com/answers/934671/view.html

/// <summary>
/// Helper class to load a sprite atlas into a dictionary, and obtain the name by index.
/// 
/// Note that this code will work if you pass to it a string in the form "PathToAtlas/AtlasName_N" 
/// where N is the index of the texture you want in AtlasName (index starts with 0).
/// </summary>
public class SpriteRepository
{
    public static Dictionary<string, Sprite[]> dict = new Dictionary<string, Sprite[]>();

    public static Sprite GetSpriteFromSheet(string name)
    {

        //Sprite[] sprites = Resources.LoadAll<Sprite>("Textures/poi_icons_18@2x");

        //Dictionary<string, Sprite> dict2 = new Dictionary<string, Sprite>();
        //foreach (Sprite sprite in sprites)
        //{
        //    dict2.Add(sprite.name, sprite);
        //}


        const string query = @"((\w|-|_|\s)+/)*((\w|-|_|\s)+)(_)(\d+)$";

        var match = Regex.Match(name, query);

        var sprName = match.Groups[3].Value;
        var sprPath = name.Remove(name.LastIndexOf('_'));
        var sprIndex = int.Parse(match.Groups[6].Value);

        Sprite[] group;
        if (!dict.TryGetValue(name, out group))
        {
            dict[name] = Resources.LoadAll<Sprite>(sprPath);
        }

        return dict[name][sprIndex];
    }
}
