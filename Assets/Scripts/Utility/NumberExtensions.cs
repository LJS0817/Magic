namespace Magic.Utility
{
    public static class NumberExtensions
    {
        /// <summary>
        /// 숫자를 k(천), m(백만) 단위로 축약하여 문자열로 반환합니다.
        /// 예: 1500 -> 1.5k, 1500000 -> 1.5m
        /// </summary>
        public static string ToShortFormat(this int amount)
        {
            if (amount >= 1000000)
                return (amount / 1000000f).ToString("0.#") + "m";
            if (amount >= 1000)
                return (amount / 1000f).ToString("0.#") + "k";
            
            return amount.ToString("N0");
        }

        /// <summary>
        /// 숫자를 k(천), m(백만) 단위로 축약하여 문자열로 반환합니다.
        /// 예: 1500 -> 1.5k, 1500000 -> 1.5m
        /// </summary>
        public static string ToShortFormat(this long amount)
        {
            if (amount >= 1000000)
                return (amount / 1000000f).ToString("0.#") + "m";
            if (amount >= 1000)
                return (amount / 1000f).ToString("0.#") + "k";
            
            return amount.ToString("N0");
        }
    }
}
