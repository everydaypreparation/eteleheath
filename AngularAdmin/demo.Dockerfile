FROM node:14.15.1 AS build
WORKDIR /app
COPY . .
RUN npm install && node --max_old_space_size=5000 ./node_modules/@angular/cli/bin/ng build --prod --build-optimizer --base-href / --output-path=./prod --crossOrigin=anonymous
  
FROM nginx

COPY --from=build /app/prod/ /usr/share/nginx/html