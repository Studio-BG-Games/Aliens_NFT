public class CheckingEnteredName : CheckingEnteredValue
{
    protected override bool Validate(string valueString)
    {
        if (valueString == "")
            return false;

        return true;
    }
}