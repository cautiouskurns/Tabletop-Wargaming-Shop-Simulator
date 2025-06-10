using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Static utility class for consistent UI text formatting across the application.
    /// Provides centralized formatting methods for currency, time, and sales displays.
    /// </summary>
    public static class UIFormatting
    {
        #region Time Formatting
        
        /// <summary>
        /// Format time values into MM:SS countdown string.
        /// 
        /// Time Formatting Utilities:
        /// - Consistent MM:SS format across all time displays
        /// - Zero-padding for professional appearance
        /// - Efficient string formatting to minimize garbage collection
        /// 
        /// Performance Considerations:
        /// - string.Format vs StringBuilder vs direct concatenation
        /// - Called frequently, so efficiency matters
        /// - ToString("D2") for zero-padding is optimized in .NET
        /// 
        /// Example outputs: "05:23", "00:47", "12:00"
        /// </summary>
        /// <param name="minutes">Minutes component (0-59 typically)</param>
        /// <param name="seconds">Seconds component (0-59)</param>
        /// <returns>Formatted time string in MM:SS format</returns>
        public static string FormatTimeCountdown(int minutes, int seconds)
        {
            // Ensure values are within expected ranges
            minutes = Mathf.Clamp(minutes, 0, 99); // Support up to 99 minutes
            seconds = Mathf.Clamp(seconds, 0, 59);
            
            // Use efficient formatting with zero-padding
            return string.Format("{0:D2}:{1:D2}", minutes, seconds);
        }
        
        /// <summary>
        /// Format elapsed time from total seconds into MM:SS format.
        /// Convenience method for direct seconds-to-time conversion.
        /// </summary>
        /// <param name="totalSeconds">Total seconds to convert</param>
        /// <returns>Formatted time string in MM:SS format</returns>
        public static string FormatTimeFromSeconds(float totalSeconds)
        {
            totalSeconds = Mathf.Max(0, totalSeconds); // Ensure non-negative
            
            int minutes = Mathf.FloorToInt(totalSeconds / 60.0f);
            int seconds = Mathf.FloorToInt(totalSeconds % 60.0f);
            
            return FormatTimeCountdown(minutes, seconds);
        }
        
        #endregion
        
        #region Currency Formatting
        
        /// <summary>
        /// Format currency values with consistent formatting rules.
        /// 
        /// Currency Formatting Utilities:
        /// - Centralized currency formatting for consistency
        /// - Handles different value ranges (dollars, cents, thousands)
        /// - Configurable precision for different use cases
        /// 
        /// Formatting Rules:
        /// - No decimals for whole dollar amounts (cleaner appearance)
        /// - Thousands separators for readability
        /// - Dollar sign prefix for clear currency indication
        /// 
        /// Example outputs: "$1,234", "$500", "$12,000"
        /// </summary>
        /// <param name="amount">Currency amount to format</param>
        /// <param name="includeCents">Whether to include cents (default: false)</param>
        /// <returns>Formatted currency string</returns>
        public static string FormatCurrency(float amount, bool includeCents = false)
        {
            // Use appropriate formatting based on cents requirement
            if (includeCents)
            {
                return string.Format("${0:N2}", amount); // $1,234.56
            }
            else
            {
                return string.Format("${0:N0}", amount); // $1,234
            }
        }
        
        /// <summary>
        /// Format currency with automatic decimal detection.
        /// Shows cents only if the amount has a fractional part.
        /// </summary>
        /// <param name="amount">Currency amount to format</param>
        /// <returns>Formatted currency string with smart decimal handling</returns>
        public static string FormatCurrencyAuto(float amount)
        {
            // Check if amount has fractional part
            bool hasFractionalPart = Mathf.Abs(amount - Mathf.Floor(amount)) > 0.001f;
            
            return FormatCurrency(amount, hasFractionalPart);
        }
        
        /// <summary>
        /// Format currency without dollar sign (for calculations display).
        /// Useful for showing raw numeric values in certain contexts.
        /// </summary>
        /// <param name="amount">Currency amount to format</param>
        /// <param name="includeCents">Whether to include cents (default: false)</param>
        /// <returns>Formatted numeric string without currency symbol</returns>
        public static string FormatNumber(float amount, bool includeCents = false)
        {
            if (includeCents)
            {
                return string.Format("{0:N2}", amount); // 1,234.56
            }
            else
            {
                return string.Format("{0:N0}", amount); // 1,234
            }
        }
        
        #endregion
        
        #region Sales and Count Formatting
        
        /// <summary>
        /// Format sales count with appropriate singular/plural handling.
        /// 
        /// Sales Formatting Utilities:
        /// - Proper pluralization for professional appearance
        /// - Handles edge cases (0, 1, multiple sales)
        /// - Consistent format across all sales displays
        /// 
        /// Example outputs: "No sales", "1 sale", "5 sales"
        /// </summary>
        /// <param name="count">Number of sales</param>
        /// <returns>Formatted sales count string</returns>
        public static string FormatSalesCount(int count)
        {
            if (count == 0)
            {
                return "No sales";
            }
            else if (count == 1)
            {
                return "1 sale";
            }
            else
            {
                return string.Format("{0} sales", count);
            }
        }
        
        /// <summary>
        /// Format sales count with revenue information.
        /// Combines count and revenue into a single display string.
        /// </summary>
        /// <param name="count">Number of sales</param>
        /// <param name="revenue">Total revenue amount</param>
        /// <returns>Formatted sales and revenue string</returns>
        public static string FormatSalesWithRevenue(int count, float revenue)
        {
            if (count == 0)
            {
                return "No sales today";
            }
            else
            {
                string salesText = FormatSalesCount(count);
                string revenueText = FormatCurrency(revenue);
                return string.Format("{0} - {1}", salesText, revenueText);
            }
        }
        
        /// <summary>
        /// Format generic count with proper pluralization.
        /// Useful for customers, items, days, etc.
        /// </summary>
        /// <param name="count">Number to format</param>
        /// <param name="singularWord">Singular form of the word</param>
        /// <param name="pluralWord">Plural form of the word (optional - adds 's' to singular if not provided)</param>
        /// <returns>Formatted count string with proper pluralization</returns>
        public static string FormatCount(int count, string singularWord, string pluralWord = null)
        {
            if (string.IsNullOrEmpty(pluralWord))
            {
                pluralWord = singularWord + "s";
            }
            
            if (count == 0)
            {
                return string.Format("No {0}", pluralWord);
            }
            else if (count == 1)
            {
                return string.Format("1 {0}", singularWord);
            }
            else
            {
                return string.Format("{0} {1}", count, pluralWord);
            }
        }
        
        #endregion
        
        #region Percentage and Progress Formatting
        
        /// <summary>
        /// Format percentage values with consistent display.
        /// </summary>
        /// <param name="value">Percentage value (0-1 or 0-100 depending on isDecimal)</param>
        /// <param name="isDecimal">True if value is 0-1, false if value is 0-100</param>
        /// <param name="decimalPlaces">Number of decimal places to show</param>
        /// <returns>Formatted percentage string</returns>
        public static string FormatPercentage(float value, bool isDecimal = true, int decimalPlaces = 1)
        {
            float percentage = isDecimal ? value * 100f : value;
            string format = decimalPlaces == 0 ? "{0:F0}%" : $"{{0:F{decimalPlaces}}}%";
            return string.Format(format, percentage);
        }
        
        /// <summary>
        /// Format progress as "X of Y" format.
        /// Useful for showing current/total progress.
        /// </summary>
        /// <param name="current">Current progress value</param>
        /// <param name="total">Total/maximum value</param>
        /// <returns>Formatted progress string</returns>
        public static string FormatProgress(int current, int total)
        {
            return string.Format("{0} of {1}", current, total);
        }
        
        #endregion
        
        #region Day/Time Display Formatting
        
        /// <summary>
        /// Format day and cycle information for time displays.
        /// </summary>
        /// <param name="day">Current day number</param>
        /// <param name="isDayTime">Whether it's currently day time</param>
        /// <param name="timeString">Formatted time string (MM:SS)</param>
        /// <returns>Complete day/time display string</returns>
        public static string FormatDayTime(int day, bool isDayTime, string timeString)
        {
            string cycleIndicator = isDayTime ? "Day" : "Night";
            return string.Format("{0} {1} - {2}", cycleIndicator, day, timeString);
        }
        
        /// <summary>
        /// Format day and cycle with progress calculation.
        /// Combines day formatting with automatic time calculation.
        /// </summary>
        /// <param name="day">Current day number</param>
        /// <param name="isDayTime">Whether it's currently day time</param>
        /// <param name="progress">Progress through current cycle (0-1)</param>
        /// <param name="cycleDurationSeconds">Duration of current cycle in seconds</param>
        /// <returns>Complete formatted day/time string</returns>
        public static string FormatDayTimeWithProgress(int day, bool isDayTime, float progress, float cycleDurationSeconds)
        {
            float elapsedSeconds = cycleDurationSeconds * progress;
            elapsedSeconds = Mathf.Clamp(elapsedSeconds, 0, cycleDurationSeconds);
            
            string timeString = FormatTimeFromSeconds(elapsedSeconds);
            return FormatDayTime(day, isDayTime, timeString);
        }
        
        #endregion
    }
}
