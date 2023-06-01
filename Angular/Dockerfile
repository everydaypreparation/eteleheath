# Create new image from nginx
FROM nginx

#set env variables
ENV EMRO_PAYPAL_CLIENT_ID="Aa-tqrGgg29jyM7v6tKkIdK89NpZsLq4MtdtTrfse89mVfayYPV-_BDRw3wyAmgZNX7x2yEc8uV_is2Y"
ENV EMRO_SELLER_ID="sb-pj6bp5145300@business.example.com"
ENV EMRO_GOOGLE_MAP_KEY="AIzaSyDgr_7V9SLRItD1SGgePa1dw0p7NaR0DT0"
ENV EMRO_WEATHER_API="https://api.openweathermap.org/data/2.5/weather?q="
ENV EMRO_WEATHER_API_KEY="32838566376f685b3ad9e56e751f08de"
ENV EMRO_BASE_URL="https://api.emro.cloud/api/"
ENV EMRO_HOST_URL="https://www.emro.cloud"
ENV EMRO_SIGNAL_R_URL="https://api.emro.cloud/signalr"

# Copy build code from  node image to nginx image
COPY ./prod/. /usr/share/nginx/html/

COPY ./prod/assets/bash_scripts/replace_environment.sh /docker-entrypoint.d/40-replace_environment.sh
RUN chmod +x /docker-entrypoint.d/40-replace_environment.sh


# ENTRYPOINT ["/bin/sh", "/usr/share/nginx/html/assets/bash_scripts/replace_environment.sh"]
# CMD ["nginx", "-g", "daemon off;"]

