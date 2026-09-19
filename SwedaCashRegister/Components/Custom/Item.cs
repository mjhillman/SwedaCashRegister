namespace SwedaCashRegister.Components.Custom
{
    public class Item
    {
        public const string NON_TAX = "Non-Tax";
        public const string TAXABLE = "Taxable";
        public string DeptName { get; set; }
        public bool IsTaxable { get; set; }
        public int ItemQty { get; set; }
        public string ItemName { get; set; }
        public decimal ItemAmount { get; set; }

        public Item() { }

        public Item(string deptName, bool isTaxable, int itemQty, string itemName, decimal itemAmount)
        {
            DeptName = deptName;
            IsTaxable = isTaxable;
            ItemQty = itemQty;
            ItemName = itemName;
            ItemAmount = itemAmount;
        }
    }
}
