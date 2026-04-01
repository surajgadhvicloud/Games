import type { UserType } from '../enums';

export interface Member {
  id: number;
  firstName: string;
  middleName: string | null;
  lastName: string;
  address: string;
  phoneNumber: string;
  email: string;
  typeOfUser: UserType;
}

export interface CreateMemberRequest {
  firstName: string;
  middleName?: string | null;
  lastName: string;
  address: string;
  phoneNumber: string;
  email: string;
  typeOfUser: UserType;
}

export interface UpdateMemberRequest extends CreateMemberRequest {}
