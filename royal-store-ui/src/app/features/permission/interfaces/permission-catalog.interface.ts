export interface PermissionItem {
  name: string;
  label: string;
}

export interface PermissionResourceGroup {
  resource: string;
  permissions: PermissionItem[];
}

export interface PermissionCatalogResponse {
  resources: PermissionResourceGroup[];
}
