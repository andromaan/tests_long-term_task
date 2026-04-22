import { RefinedResponse, ResponseType } from "k6/http";

export const BASE_URL: string = __ENV.BASE_URL || "http://localhost:5229";

export const ENDPOINTS = {
  properties: `${BASE_URL}/api/properties`,
  propertyById: (id: number) => `${BASE_URL}/api/properties/${id}`,
  inquiries: (propertyId: number) =>
    `${BASE_URL}/api/properties/${propertyId}/inquiries`,
  agentsProperties: (agentId: number) =>
    `${BASE_URL}/api/agents/${agentId}/properties`,
  agentsInquiries: (agentId: number) =>
    `${BASE_URL}/api/agents/${agentId}/inquiries`,
} as const;

export const HEADERS: Record<string, string> = {
  "Content-Type": "application/json",
};

export const THRESHOLDS: Record<string, string[]> = {
  http_req_duration: ["p(95)<500", "p(99)<1000"],
  http_req_failed: ["rate<0.01"],
};

export interface CreateInquiryRequest {
  name: string;
  email: string;
  phone: string;
  message: string;
}

export interface PropertyResponse {
  id: number;
  title: string;
  city: string;
  type: number;
  price: number;
  bedrooms: number;
  status: number;
  agentId: number;
}

export function parseBody<T>(res: RefinedResponse<ResponseType>): T {
  return JSON.parse(res.body as string) as T;
}
