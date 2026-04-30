# syntax=docker/dockerfile:1.7

FROM node:20-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN --mount=type=cache,id=npm,target=/root/.npm \
    npm ci

COPY . .
RUN npm run build

FROM nginx:1.27-alpine AS final
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
