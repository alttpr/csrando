CREATE TABLE `api_key` (
	`id` text PRIMARY KEY NOT NULL,
	`user_id` text NOT NULL,
	`name` text NOT NULL,
	`token_hash` text NOT NULL,
	`token_prefix` text NOT NULL,
	`created_at` integer DEFAULT (current_timestamp) NOT NULL,
	`last_used_at` integer,
	`revoked_at` integer,
	FOREIGN KEY (`user_id`) REFERENCES `user`(`id`) ON UPDATE no action ON DELETE cascade
);
--> statement-breakpoint
CREATE UNIQUE INDEX `api_key_token_hash_unique` ON `api_key` (`token_hash`);--> statement-breakpoint
CREATE INDEX `api_key_user_idx` ON `api_key` (`user_id`);