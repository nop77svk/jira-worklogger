namespace jwl.Infra;

public static class StringExt
{
    public static string ToLowerFirstChar(this string str)
        => string.Create(str.Length, str, (output, input)
            =>
            {
                input.CopyTo(output);
                output[0] = char.ToLowerInvariant(input[0]);
            });
}
