using System;

namespace Common.Contracts
{
    [Serializable]
    public class AccountChange
    {
        public AccountChange(string exchangeName, string currency, decimal amount)
        {
            Currency = currency.ToUpper();
            Amount = amount;
            ExchangeName = exchangeName;
        }

        public string ExchangeName { get; private set; }
        public string Currency { get; private set; }
        public decimal Amount { get; private set; }
        public override string ToString()
        {
            return "AccountChange {0} {1}".FormatAs(Currency, Amount);
        }

        public void UpdateAmount(decimal amount)
        {
            Amount = amount;
        }
    }
}
