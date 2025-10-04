-- Create table for randomizer versions and snapshot data
CREATE TABLE IF NOT EXISTS `randomizer_version` (
  `id` text PRIMARY KEY NOT NULL,
  `version_tag` text NOT NULL UNIQUE,
  `options_metadata` text NOT NULL,
  `post_gen_settings` text NOT NULL,
  `ips_base_patch_base64` text NOT NULL,
  `base_patch_sha256` text,
  `is_active` integer DEFAULT 0 NOT NULL,
  `created_at` integer DEFAULT (current_timestamp) NOT NULL
);

--> statement-breakpoint

-- Add nullable foreign key to seeds referencing randomizer_version
ALTER TABLE `seed` ADD COLUMN `randomizer_version_id` text REFERENCES `randomizer_version`(`id`);
