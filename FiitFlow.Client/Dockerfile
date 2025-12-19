# build
FROM node:20-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
ARG VITE_API_BASE=/api
ENV VITE_API_BASE=$VITE_API_BASE
RUN npm run build

# serve
FROM nginx:1.27-alpine AS run
WORKDIR /usr/share/nginx/html
COPY --from=build /app/dist .
EXPOSE 80

