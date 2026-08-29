import httpClient from "./httpClient";
import type {
  ApplicationUser,
  CreateApplicationUserRequest,
  UpdateApplicationUserRequest,
  UpdateApplicationUserRolesRequest,
} from "../types/applicationUser";

const applicationUsersPath =
  "/api/application-users";

export async function getApplicationUsers():
  Promise<ApplicationUser[]> {
  const response =
    await httpClient.get<ApplicationUser[]>(
      `${applicationUsersPath}/`,
    );

  return response.data;
}

export async function getApplicationUser(
  id: string,
): Promise<ApplicationUser> {
  const response =
    await httpClient.get<ApplicationUser>(
      `${applicationUsersPath}/${id}`,
    );

  return response.data;
}

export async function createApplicationUser(
  request: CreateApplicationUserRequest,
): Promise<ApplicationUser> {
  const response =
    await httpClient.post<ApplicationUser>(
      `${applicationUsersPath}/`,
      request,
    );

  return response.data;
}

export async function updateApplicationUser(
  id: string,
  request: UpdateApplicationUserRequest,
): Promise<ApplicationUser> {
  const response =
    await httpClient.put<ApplicationUser>(
      `${applicationUsersPath}/${id}`,
      request,
    );

  return response.data;
}

export async function updateApplicationUserRoles(
  id: string,
  request: UpdateApplicationUserRolesRequest,
): Promise<ApplicationUser> {
  const response =
    await httpClient.put<ApplicationUser>(
      `${applicationUsersPath}/${id}/roles`,
      request,
    );

  return response.data;
}