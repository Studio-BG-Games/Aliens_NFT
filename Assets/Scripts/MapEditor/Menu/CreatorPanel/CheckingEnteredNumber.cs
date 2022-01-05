using System;
using System.Collections;
using System.Collections.Generic;

public class CheckingEnteredNumber : CheckingEnteredValue
{
    protected override string StandartValue { get; set; } = "0";

    protected override bool Validate(string valueString)
    {
        try
        {
            int value = Convert.ToInt32(valueString);
            if (value <= 0)
                return false;
        }
        catch
        {
            return false;
        }

        return true;
    }
}