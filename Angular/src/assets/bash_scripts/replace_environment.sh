#!/bin/sh

grep --exclude=*.{svg,png,jpg,ico,png_1,eot,ttf,woff,woff2,txt,sh} -rl EMRO_PAYPAL_CLIENT_ID /usr/share/nginx/html/ | xargs sed -i -e 's,EMRO_PAYPAL_CLIENT_ID,'$EMRO_PAYPAL_CLIENT_ID',g'
grep --exclude=*.{svg,png,jpg,ico,png_1,eot,ttf,woff,woff2,txt,sh} -rl EMRO_GOOGLE_MAP_KEY /usr/share/nginx/html/ | xargs sed -i -e 's,EMRO_GOOGLE_MAP_KEY,'$EMRO_GOOGLE_MAP_KEY',g'
grep --exclude=*.{svg,png,jpg,ico,png_1,eot,ttf,woff,woff2,txt,sh} -rl EMRO_WEATHER_API_KEY /usr/share/nginx/html/ | xargs sed -i -e 's,EMRO_WEATHER_API_KEY,'$EMRO_WEATHER_API_KEY',g'
grep --exclude=*.{svg,png,jpg,ico,png_1,eot,ttf,woff,woff2,txt,sh} -rl EMRO_WEATHER_API /usr/share/nginx/html/ | xargs sed -i -e 's,EMRO_WEATHER_API,'$EMRO_WEATHER_API',g'
grep --exclude=*.{svg,png,jpg,ico,png_1,eot,ttf,woff,woff2,txt,sh} -rl EMRO_SELLER_ID /usr/share/nginx/html/ | xargs sed -i -e 's,EMRO_SELLER_ID,'$EMRO_SELLER_ID',g'
grep --exclude=*.{svg,png,jpg,ico,png_1,eot,ttf,woff,woff2,txt,sh} -rl EMRO_BASE_URL /usr/share/nginx/html/ | xargs sed -i -e 's,EMRO_BASE_URL,'$EMRO_BASE_URL',g'
grep --exclude=*.{svg,png,jpg,ico,png_1,eot,ttf,woff,woff2,txt,sh} -rl EMRO_HOST_URL /usr/share/nginx/html/ | xargs sed -i -e 's,EMRO_HOST_URL,'$EMRO_HOST_URL',g'
grep --exclude=*.{svg,png,jpg,ico,png_1,eot,ttf,woff,woff2,txt,sh} -rl EMRO_SIGNAL_R_URL /usr/share/nginx/html/ | xargs sed -i -e 's,EMRO_SIGNAL_R_URL,'$EMRO_SIGNAL_R_URL',g'
