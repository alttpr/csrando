FROM node:20-alpine AS builder
WORKDIR /app

# Install deps
COPY package*.json ./
RUN npm ci

# Copy source
COPY . .

# Build (SvelteKit adapter-node)
RUN npm run prepare && npm run build

FROM node:20-alpine AS runner
WORKDIR /app
ENV NODE_ENV=production

# Install native build dependencies for better-sqlite3
RUN apk add --no-cache python3 make g++

# Copy package files and install production dependencies
COPY package*.json ./
RUN npm ci --omit=dev

# Copy built app
COPY --from=builder /app/build ./build

# Copy drizzle migrations (required for database initialization)
COPY --from=builder /app/drizzle ./drizzle

# Expose default port used by adapter-node server
EXPOSE 3000

# Start the SvelteKit server
CMD ["node", "build"]
