using System.Text;

namespace Cod.IoT
{
    public static class StringExtensions
    {
        public static string ReplaceSlashIntoBackSlash(this string input)
        {
            var result = new StringBuilder(input.Length);
            for (int i = 0; i < input.Length; i++)
            {
                var c = input[i];
                if (c == '/')
                {
                    result.Append('\\');
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }
    }
}
