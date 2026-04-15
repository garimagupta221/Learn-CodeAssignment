using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PositiveDivisor
{
    public class DivisorCalculator
    {
        public DivisorCalculator(InputValidator validator)
        {
            _validator = validator;
        }
        public int CalculateDivisorCount(int number)
        {
            _validator.ValidateNumber(number);
            int count = 0;
            for (int index = 1; index * index <= number; index++)
            {
                if (number % index == 0)
                {
                    count += (index * index == number) ? 1 : 2;
                }
            }

            return count;
        }

        public int GetTotalDivisors(int limit)
        {
            _validator.ValidateLimit(limit);

            int matchCount = 0;
            for (int current = 2; current < limit; current++)
            {
                if (CalculateDivisorCount(current) == CalculateDivisorCount(current + 1))
                {
                    matchCount++;
                }
            }

            return matchCount;
        }

        private readonly InputValidator _validator;

    }
}
