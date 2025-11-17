FROM node:20-alpine AS build
WORKDIR /app

# Kopiujemy pliki z zależnościami
COPY package*.json ./

# Instalacja zależności
RUN npm install

# Kopiujemy resztę projektu
COPY . .

# Argument budowania dla URL API (można override podczas docker build)
ARG VITE_API_URL=http://109.173.167.125:32001
ENV VITE_API_URL=$VITE_API_URL

# Budujemy aplikację (Vite -> domyślnie /dist)
RUN npm run build

# ---- Stage 2: nginx ----
FROM nginx:alpine

# Kopiujemy z /app/dist zamiast /app/build
COPY --from=build /app/dist /usr/share/nginx/html

# Upewnij się, że nginx.conf jest obok Dockerfile
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]