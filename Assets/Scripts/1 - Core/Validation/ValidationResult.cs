using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Simple result class for validation operations.
    /// Encapsulates success/failure state with optional error message.
    /// </summary>
    public struct ValidationResult
    {
        public bool IsSuccess { get; private set; }
        public string ErrorMessage { get; private set; }
        
        /// <summary>
        /// Create a successful validation result
        /// </summary>
        /// <returns>ValidationResult indicating success</returns>
        public static ValidationResult Success()
        {
            return new ValidationResult
            {
                IsSuccess = true,
                ErrorMessage = null
            };
        }
        
        /// <summary>
        /// Create a failed validation result with error message
        /// </summary>
        /// <param name="errorMessage">Error message describing the failure</param>
        /// <returns>ValidationResult indicating failure</returns>
        public static ValidationResult Failure(string errorMessage)
        {
            return new ValidationResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }
        
        /// <summary>
        /// Implicit conversion to bool for easy checking
        /// </summary>
        public static implicit operator bool(ValidationResult result)
        {
            return result.IsSuccess;
        }
    }
}
