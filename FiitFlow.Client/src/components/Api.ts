import axios, { type AxiosInstance } from "axios";

const API_BASE = import.meta.env.VITE_API_BASE ?? "/api";

const api: AxiosInstance = axios.create({
  baseURL: API_BASE,
});

api.interceptors.request.use((config) => {
  const s = localStorage.getItem("session");
  if (s) {
    // имя заголовка может отличаться. Начни с этого.
    config.headers["X-Session"] = s;
  }
  return config;
});

export default api;
export { api };

