namespace jwl.Infra;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
