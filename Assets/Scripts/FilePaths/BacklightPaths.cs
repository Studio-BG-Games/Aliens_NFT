using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BacklightPaths
{
    private const string Folder = "Tiles/Hex/Backlight";
    
    public const string CorrectBacklight = Folder + "/CorrectBacklight";
    public const string SelectedCorrectBacklight = Folder + "/SelectedCorrectBacklight";

    public const string Incorrect = Folder + "/Incorrect";
    public const string SelectedIncorrect = Folder + "/SelectedIncorrect";

    public static string GetPath(BacklightType type)
    {
        switch (type)
        {
            case BacklightType.CorrectBacklight:
                return CorrectBacklight;
            case BacklightType.SelectedCorrectBacklight:
                return SelectedCorrectBacklight;
            case BacklightType.Incorrect:
                return Incorrect;
            case BacklightType.SelectedIncorrect:
                return SelectedIncorrect;
            default:
                throw new System.Exception("No suitable backlighting");
        }
    }
}

public enum BacklightType
{
    CorrectBacklight,
    SelectedCorrectBacklight,
    Incorrect,
    SelectedIncorrect
}
