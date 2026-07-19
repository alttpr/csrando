PRAGMA foreign_keys=OFF;--> statement-breakpoint
ALTER TABLE `configuration_profile_revision` RENAME TO `configuration_preset_revision`;--> statement-breakpoint
ALTER TABLE `configuration_profile` RENAME TO `configuration_preset`;--> statement-breakpoint
ALTER TABLE `user_profile_favorite` RENAME TO `user_preset_favorite`;--> statement-breakpoint
ALTER TABLE `user_profile_preference` RENAME TO `user_preset_preference`;--> statement-breakpoint
ALTER TABLE `configuration_preset_revision` RENAME COLUMN `profile_id` TO `preset_id`;--> statement-breakpoint
ALTER TABLE `user_preset_favorite` RENAME COLUMN `profile_id` TO `preset_id`;--> statement-breakpoint
ALTER TABLE `user_preset_preference` RENAME COLUMN `default_profile_id` TO `default_preset_id`;--> statement-breakpoint
ALTER TABLE `user_preset_preference` RENAME COLUMN `last_used_profile_id` TO `last_used_preset_id`;--> statement-breakpoint
ALTER TABLE `seed` RENAME COLUMN `profile_id` TO `preset_id`;--> statement-breakpoint
ALTER TABLE `seed` RENAME COLUMN `profile_revision_id` TO `preset_revision_id`;--> statement-breakpoint
DROP INDEX `configuration_profile_revision_unique`;--> statement-breakpoint
DROP INDEX `configuration_profile_slug_unique`;--> statement-breakpoint
DROP INDEX `configuration_profile_share_token_unique`;--> statement-breakpoint
DROP INDEX `configuration_profile_owner_idx`;--> statement-breakpoint
DROP INDEX `configuration_profile_config_idx`;--> statement-breakpoint
CREATE UNIQUE INDEX `configuration_preset_revision_unique` ON `configuration_preset_revision` (`preset_id`, `revision_number`);--> statement-breakpoint
CREATE UNIQUE INDEX `configuration_preset_slug_unique` ON `configuration_preset` (`slug`);--> statement-breakpoint
CREATE UNIQUE INDEX `configuration_preset_share_token_unique` ON `configuration_preset` (`share_token`);--> statement-breakpoint
CREATE INDEX `configuration_preset_owner_idx` ON `configuration_preset` (`owner_user_id`);--> statement-breakpoint
CREATE INDEX `configuration_preset_config_idx` ON `configuration_preset` (`config_id`, `scope`);--> statement-breakpoint
PRAGMA foreign_keys=ON;
