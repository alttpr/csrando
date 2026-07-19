ALTER TABLE `configuration_profile` ADD `share_token` text;--> statement-breakpoint
CREATE UNIQUE INDEX `configuration_profile_share_token_unique` ON `configuration_profile` (`share_token`);