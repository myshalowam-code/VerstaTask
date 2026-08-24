FROM node:22-alpine AS build
WORKDIR /app
ENV NODE_OPTIONS=--dns-result-order=ipv4first
COPY web/package*.json ./
RUN npm ci --no-audit --fetch-retries=5 --fetch-retry-mintimeout=2000
COPY web .
ARG VITE_API_URL=http://localhost:5080
ENV VITE_API_URL=$VITE_API_URL
RUN npm run build

FROM nginx:1.27-alpine
COPY web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
