namespace libs.workspace.angular.apps.src.lib.objects.purchaseorderitem.edit
{
    using Allors.Database.Domain;

    public static partial class PurchaseOrderItemEditComponentExtensions
    {
        public static PurchaseOrderItemEditComponent BuildForNonSerializedPartDefaults(this PurchaseOrderItemEditComponent @this, PurchaseOrderItem purchaseOrderItem)
        {
            @this.OrderItemDescription_1.Set(purchaseOrderItem.Description);
            @this.Comment.Set(purchaseOrderItem.Comment);
            @this.InternalComment.Set(purchaseOrderItem.InternalComment);

            return @this;
        }

        public static PurchaseOrderItemEditComponent BuildForSerializedPartDefaults(this PurchaseOrderItemEditComponent @this, PurchaseOrderItem purchaseOrderItem)
        {
            @this.QuantityOrdered.Set(purchaseOrderItem.QuantityOrdered.ToString());
            @this.OrderItemDescription_1.Set(purchaseOrderItem.Description);
            @this.Comment.Set(purchaseOrderItem.Comment);
            @this.InternalComment.Set(purchaseOrderItem.InternalComment);

            return @this;
        }
    }
}
