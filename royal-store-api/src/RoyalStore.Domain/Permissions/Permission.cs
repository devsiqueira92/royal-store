namespace RoyalStore.Domain.Permissions;

public enum Permission
{
    SalesCreate,
    SalesCancel,
    SalesRefund,
    SalesApplyDiscount,
    SalesOverridePrice,

    CashRegisterOpen,
    CashRegisterClose,
    CashRegisterCashDrop,

    InventoryView,
    InventoryAdjust,
    InventoryReceive,
    InventoryViewCost,

    CustomersManageCredit,
    CustomersCreate,

    ReportsViewFinancials,

    UsersManage,

    RolesManage,

    SystemManagerOverride,
}
