import { SellerRole } from '../../interfaces/seller.interface';

/**
 * Permissions that a role always carries. When a role is selected these are
 * shown checked + disabled in the UI and are NOT persisted as explicit grants —
 * the backend already unions them through the role. Keep this map as the single
 * source of truth for role-implied access.
 */
export const ROLE_IMPLIED_PERMISSIONS: Record<SellerRole, readonly string[]> = {
  seller: [
    'sales.create',
    'inventory.view',
    'cashRegister.open',
    'cashRegister.close',
  ],
  manager: [
    'sales.cancel',
    'sales.refund',
    'sales.applyDiscount',
    'cashRegister.cashDrop',
    'inventory.view',
    'customers.manageCredit',
    'customers.create',
  ],
  stock: [],
};

export function impliedPermissionsFor(
  role: SellerRole | null | undefined,
): readonly string[] {
  return role ? ROLE_IMPLIED_PERMISSIONS[role] : [];
}

/** Drops any permission that the given role already implies. */
export function stripImplied(
  role: SellerRole | null | undefined,
  permissions: readonly string[],
): string[] {
  const implied = new Set(impliedPermissionsFor(role));
  return permissions.filter(permission => !implied.has(permission));
}
