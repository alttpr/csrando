-- Add randomizer_id to randomizer_version for game identification (e.g., 'alttpr', 'z1r')
ALTER TABLE `randomizer_version` ADD COLUMN `randomizer_id` text;

