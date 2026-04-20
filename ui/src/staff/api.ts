import { useQuery } from "@tanstack/react-query";
import ky from "ky";

export async function staffLogin(accessCode: string): Promise<void> {
  const response = await ky.post("api/staff/login", {
    headers: { "X-Staff-Code": accessCode },
    throwHttpErrors: false,
  });
  if (response.status === 401) throw new Error("Invalid access code");
  if (!response.ok) throw new Error("Login failed");
}

export function useStaffAuthCheck() {
  return useQuery({
    queryKey: ["staff", "auth"],
    queryFn: async () => {
      const response = await ky.get("api/staff/check", { throwHttpErrors: false });
      return response.ok;
    },
    retry: false,
  });
}
