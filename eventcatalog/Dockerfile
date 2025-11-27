FROM node:lts

WORKDIR /app

COPY package.json package-lock.json ./
RUN npm install

COPY . .

# Fix for Astro in Docker: https://github.com/withastro/astro/issues/2596
ENV NODE_OPTIONS=--max_old_space_size=4096
ENV PORT=3000

RUN chmod +x scripts/docker-entrypoint.sh

EXPOSE 3000

ENTRYPOINT ["./scripts/docker-entrypoint.sh"]
