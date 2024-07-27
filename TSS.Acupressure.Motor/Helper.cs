using System.Collections;

namespace TSS.Acupressure.Motor
{
    public class Helper
    {
        public static string JoinMotorSequence(ArrayList input)
        {
            var result = string.Empty;
            for (int i = 0; i < input.Count; i++)
            {
                result += ((int)input[i]).ToString();
                if (i != input.Count - 1)
                {
                    result += ',';
                }
            }

            return result;
        }

        public static ArrayList ParseMotorSequence(string input)
        {
            var result = new ArrayList();
            if (input != null)
            {
                var segments = input.Split(',');
                if (segments.Length > 0)
                {
                    foreach (string seq in segments)
                    {
                        if (!int.TryParse(seq, out int i))
                        {
                            result.Clear();
                            break;
                        }

                        result.Add(i);
                    }
                }
            }

            return result;
        }
    }
}
