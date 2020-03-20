using System.Collections.Generic;
using System.Linq;

namespace Unishare.Apps.WindowsService
{
    public static class Algorithms
    {
        public static char LowestMissingLetter(IEnumerable<char> sequence)
        {
            var holes = Enumerable.Repeat(false, 23).ToArray();
            foreach (var letter in sequence)
            {
                if (letter < 68) continue;
                holes[letter - 68] = true;
            }

            for (var i = 0; i < 23; i++) if (!holes[i]) return (char)(i + 68);
            return char.MinValue;
        }
    }
}
