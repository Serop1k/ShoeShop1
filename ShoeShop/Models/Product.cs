namespace ShoeShop.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Article { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public int ManufacturerId { get; set; }
        public string ManufacturerName { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public decimal Price { get; set; }
        public int Discount { get; set; }
        public int StockQty { get; set; }
        public string ImagePath { get; set; }

        public decimal FinalPrice => Price - (Price * Discount / 100m);
        public bool HasDiscount => Discount > 0;
        public bool IsOutOfStock => StockQty == 0;
        public bool BigDiscount => Discount > 15;
    }

    public class Order
    {
        public int OrderId { get; set; }
        public string Article { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public string PickupPoint { get; set; }
        public System.DateTime OrderDate { get; set; }
        public System.DateTime? DeliveryDate { get; set; }
    }

    public class LookupItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}