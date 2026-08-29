import type { ApplicationRole } from "./auth";

export interface ApplicationUser {
  id: string;
  email: string;
  displayName: string;
  isEnabled: boolean;
  roles: ApplicationRole[];
  createdAt: string;
  updatedAt: string;
}

export interface CreateApplicationUserRequest {
  email: string;
  displayName: string;
  password: string;
  roles: ApplicationRole[];
}

export interface UpdateApplicationUserRequest {
  email: string;
  displayName: string;
  isEnabled: boolean;
}

export interface UpdateApplicationUserRolesRequest {
  roles: ApplicationRole[];
}