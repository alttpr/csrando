CREATE TABLE `configuration_profile_revision` (
	`id` text PRIMARY KEY NOT NULL,
	`profile_id` text NOT NULL,
	`revision_number` integer NOT NULL,
	`config_schema_version` integer NOT NULL,
	`settings` text NOT NULL,
	`change_summary` text,
	`created_by` text,
	`created_at` integer DEFAULT (current_timestamp) NOT NULL,
	`published_at` integer,
	FOREIGN KEY (`profile_id`) REFERENCES `configuration_profile`(`id`) ON UPDATE no action ON DELETE cascade,
	FOREIGN KEY (`created_by`) REFERENCES `user`(`id`) ON UPDATE no action ON DELETE no action
);
--> statement-breakpoint
CREATE UNIQUE INDEX `configuration_profile_revision_unique` ON `configuration_profile_revision` (`profile_id`,`revision_number`);--> statement-breakpoint
CREATE TABLE `configuration_profile` (
	`id` text PRIMARY KEY NOT NULL,
	`owner_user_id` text,
	`scope` text NOT NULL,
	`slug` text,
	`config_id` text NOT NULL,
	`name` text NOT NULL,
	`description` text,
	`current_revision_id` text,
	`game_tags` text,
	`difficulty_tag` text,
	`is_recommended` integer DEFAULT false NOT NULL,
	`featured` integer DEFAULT false NOT NULL,
	`archived` integer DEFAULT false NOT NULL,
	`display_order` integer DEFAULT 0 NOT NULL,
	`deleted_at` integer,
	`created_at` integer DEFAULT (current_timestamp) NOT NULL,
	`updated_at` integer DEFAULT (current_timestamp) NOT NULL,
	FOREIGN KEY (`owner_user_id`) REFERENCES `user`(`id`) ON UPDATE no action ON DELETE cascade
);
--> statement-breakpoint
CREATE UNIQUE INDEX `configuration_profile_slug_unique` ON `configuration_profile` (`slug`);--> statement-breakpoint
CREATE INDEX `configuration_profile_owner_idx` ON `configuration_profile` (`owner_user_id`);--> statement-breakpoint
CREATE INDEX `configuration_profile_config_idx` ON `configuration_profile` (`config_id`,`scope`);--> statement-breakpoint
CREATE TABLE `user_profile_favorite` (
	`user_id` text NOT NULL,
	`profile_id` text NOT NULL,
	`display_order` integer DEFAULT 0 NOT NULL,
	`created_at` integer DEFAULT (current_timestamp) NOT NULL,
	PRIMARY KEY(`user_id`, `profile_id`),
	FOREIGN KEY (`user_id`) REFERENCES `user`(`id`) ON UPDATE no action ON DELETE cascade,
	FOREIGN KEY (`profile_id`) REFERENCES `configuration_profile`(`id`) ON UPDATE no action ON DELETE cascade
);
--> statement-breakpoint
CREATE TABLE `user_profile_preference` (
	`user_id` text PRIMARY KEY NOT NULL,
	`default_profile_id` text,
	`last_used_profile_id` text,
	`updated_at` integer DEFAULT (current_timestamp) NOT NULL,
	FOREIGN KEY (`user_id`) REFERENCES `user`(`id`) ON UPDATE no action ON DELETE cascade,
	FOREIGN KEY (`default_profile_id`) REFERENCES `configuration_profile`(`id`) ON UPDATE no action ON DELETE set null,
	FOREIGN KEY (`last_used_profile_id`) REFERENCES `configuration_profile`(`id`) ON UPDATE no action ON DELETE set null
);
--> statement-breakpoint
ALTER TABLE `seed` ADD `profile_id` text;--> statement-breakpoint
ALTER TABLE `seed` ADD `profile_revision_id` text;--> statement-breakpoint
ALTER TABLE `seed` ADD `differed_from_revision` integer;--> statement-breakpoint
ALTER TABLE `seed` ADD `config_schema_version` integer;--> statement-breakpoint
ALTER TABLE `seed` ADD `settings_snapshot` text;--> statement-breakpoint
ALTER TABLE `user` ADD `is_admin` integer DEFAULT false NOT NULL;