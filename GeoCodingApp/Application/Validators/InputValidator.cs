using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoCodingApp.Application.Validators
{
    public static class InputValidator
    {
        public static bool IsValid(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false; 
            }

            if (!input.Any(char.IsLetter))
            {
                return false;
            }

            return true;
        }
    }
}
