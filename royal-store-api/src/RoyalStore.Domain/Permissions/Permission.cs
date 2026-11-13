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

    CatalogManageTaxProfiles,

    ExpensesView,
    ExpensesCreate,
    ExpensesCancel,
    ExpensesRegisterPayment,
    ExpensesReversePayment,
    ExpensesManageCategories,
    ExpensesManageRecurrences,
    ExpensesViewReports,
}
