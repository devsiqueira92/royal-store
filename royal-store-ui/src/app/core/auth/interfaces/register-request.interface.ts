import { DocumentType } from '../../../shared/interfaces/document-type.enum';

export interface RegisterRequest {
  email: string;
  document: string;
  documentType: DocumentType;
  storeName: string;
  password: string;
  confirmPassword: string;
}
