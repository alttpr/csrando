FROM node:20-alpine AS builder
WORKDIR /app

# Install deps
COPY package*.json ./
RUN npm ci

# Copy source
COPY . .

# Build (SvelteKit adapter-node)
RUN npm run build

FROM node:20-alpine AS runner
WORKDIR /app
ENV NODE_ENV=production

# Copy built app only
COPY --from=builder /app/build ./build

# Expose default port used by adapter-node server
EXPOSE 3000

# Start the SvelteKit server
CMD ["node", "build"]

