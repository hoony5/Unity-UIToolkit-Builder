using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public class USSReader : MonoBehaviour
{
    private const int Capacity = 64;
    public StyleSheet uss;
    public string path;

    public List<string> classNames = new List<string>(Capacity);
    // Test
    public void Read()
    {
        if (uss is null) return;
        
        using FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
        using StreamReader reader = new StreamReader(fs);
        string css = reader.ReadToEnd();
        
        // .selector , must have block nested
        string pattern = @"(?<=^|\n)(?<className>\.[a-zA-Z0-9_\-]+)";
        classNames.Clear();
        MatchCollection matches = Regex.Matches(css, pattern, RegexOptions.Multiline);

        if (matches.Count == 0) return;
        foreach (Match match in matches)
        {
            string selector = match.Groups["className"].Value.TrimStart('.');
            if(!classNames.Contains(selector))
                classNames.Add(selector);
        }
        reader.Close();
        fs.Close();
    }
}
