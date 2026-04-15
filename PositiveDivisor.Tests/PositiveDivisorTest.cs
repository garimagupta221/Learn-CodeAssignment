using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using PositiveDivisor;

namespace PositiveDivisor.Tests
{
    public class PositiveDivisorTest
    {
        public PositiveDivisorTest()
        {
            var validator = new InputValidator();
            _calculator = new DivisorCalculator(validator);
        }

        [Fact]
        public void ValidInput()
        {
            var result = _calculator.GetTotalDivisors(15);
            Assert.Equal(2, result);
        }

        [Fact]
        public void MinValidInput()
        {
            var result = _calculator.GetTotalDivisors(3);
            Assert.Equal(1, result);
        }

        [Fact]
        public void BoundaryInput()
        {
            Assert.Throws<ArgumentException>(() => _calculator.GetTotalDivisors(2));
        }

        [Fact]
        public void BelowRangeInput()
        {
            Assert.Throws<ArgumentException>(() => _calculator.GetTotalDivisors(1));
        }

        [Fact]
        public void MinimumInput()
        {
            Assert.Throws<ArgumentException>(() => _calculator.GetTotalDivisors(0));
        }

        [Fact]
        public void NegativeInvalidInput()
        {
            Assert.Throws<ArgumentException>(() => _calculator.GetTotalDivisors(-5));
        }

        [Fact]
        public void LargeInput()
        {
            var result = _calculator.GetTotalDivisors(1000);
            Assert.True(result >= 0);
        }

        [Fact]
        public void PrimeNumberInput()
        {
            var result = _calculator.CalculateDivisorCount(7);
            Assert.Equal(2, result);
        }

        [Fact]
        public void PerfectSquareInput()
        {
            var result = _calculator.CalculateDivisorCount(4);
            Assert.Equal(3, result);
        }

        [Fact]
        public void ValidNumberInput()
        {
            var result = _calculator.CalculateDivisorCount(14);
            Assert.Equal(4, result);
        }

        [Fact]
        public void InvalidInputZero()
        {
            Assert.Throws<System.ArgumentException>(() => _calculator.CalculateDivisorCount(0));
        }

        [Fact]
        public void NegativeInvalidInputForDivisor()
        {
            Assert.Throws<System.ArgumentException>(() => _calculator.CalculateDivisorCount(-10));
        }

        private readonly DivisorCalculator _calculator;

    }
}
