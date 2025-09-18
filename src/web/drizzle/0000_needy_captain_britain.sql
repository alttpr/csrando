CREATE TABLE `seed` (
	`id` text PRIMARY KEY NOT NULL,
	`options` text NOT NULL,
	`patch_data` text NOT NULL,
	`placement_info` text NOT NULL,
	`created_at` integer DEFAULT '"2025-05-08T07:03:47.413Z"' NOT NULL
);
--> statement-breakpoint
CREATE TABLE `session` (
	`id` text PRIMARY KEY NOT NULL,
	`user_id` text NOT NULL,
	`expires_at` integer NOT NULL,
	FOREIGN KEY (`user_id`) REFERENCES `user`(`id`) ON UPDATE no action ON DELETE no action
);
--> statement-breakpoint
CREATE TABLE `user_seed` (
	`user_id` text NOT NULL,
	`seed_id` text NOT NULL,
	`created_at` integer DEFAULT '"2025-05-08T07:03:47.413Z"' NOT NULL,
	FOREIGN KEY (`user_id`) REFERENCES `user`(`id`) ON UPDATE no action ON DELETE no action,
	FOREIGN KEY (`seed_id`) REFERENCES `seed`(`id`) ON UPDATE no action ON DELETE no action
);
--> statement-breakpoint
CREATE TABLE `user` (
	`id` text PRIMARY KEY NOT NULL,
	`username` text NOT NULL,
	`github_id` integer,
	`hashed_password` text
);
--> statement-breakpoint
CREATE UNIQUE INDEX `user_username_unique` ON `user` (`username`);--> statement-breakpoint
CREATE UNIQUE INDEX `user_github_id_unique` ON `user` (`github_id`);