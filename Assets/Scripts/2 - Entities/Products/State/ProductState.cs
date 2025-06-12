namespace TabletopShop
{
    /// <summary>
    /// Represents the current state of a product in the shop system
    /// </summary>
    public enum ProductState
    {
        /// <summary>
        /// Product is available in inventory but not placed on a shelf
        /// </summary>
        Available,
        
        /// <summary>
        /// Product is currently placed on a shelf and available for purchase
        /// </summary>
        OnShelf,
        
        /// <summary>
        /// Product has been purchased by a customer
        /// </summary>
        Purchased
    }
}
