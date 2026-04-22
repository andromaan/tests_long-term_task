import http, { RefinedResponse, ResponseType } from "k6/http";
import { ENDPOINTS, HEADERS, CreateInquiryRequest } from "./config.ts";

export function getProperties(params?: string): RefinedResponse<ResponseType> {
  const url = params
    ? `${ENDPOINTS.properties}?${params}`
    : ENDPOINTS.properties;
  return http.get(url, { headers: HEADERS });
}

export function getPropertyById(id: number): RefinedResponse<ResponseType> {
  return http.get(ENDPOINTS.propertyById(id), { headers: HEADERS });
}

export function submitInquiry(
  propertyId: number,
  payload: CreateInquiryRequest,
): RefinedResponse<ResponseType> {
  return http.post(ENDPOINTS.inquiries(propertyId), JSON.stringify(payload), {
    headers: HEADERS,
  });
}
