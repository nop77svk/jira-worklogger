namespace Jwl.Inputs;

public class InputRowException
    : Exception
{
    public int Row { get; }

    public InputRowException(int row)
        : base($"Error on row {row}")
    {
        Row = row;
    }

    public InputRowException(int row, Exception innerException)
        : base($"Error on row {row}", innerException)
    {
        Row = row;
    }
}
