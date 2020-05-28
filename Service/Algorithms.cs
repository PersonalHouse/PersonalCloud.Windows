using System.Collections.Generic;
using System.Linq;

namespace NSPersonalCloud.WindowsService
{
    public static class Algorithms
    {
        public static char LowestMissingLetter(IEnumerable<char> sequence)
        {
            if (sequence is null) throw new System.ArgumentNullException(nameof(sequence));

            var holes = Enumerable.Repeat(false, 23).ToArray();
            foreach (var letter in sequence)
            {
                if (letter < 68) continue;
                holes[letter - 68] = true;
            }

            for (var i = 0; i < 23; i++) if (!holes[i]) return (char) (i + 68);
            return char.MinValue;
        }
    }
}
