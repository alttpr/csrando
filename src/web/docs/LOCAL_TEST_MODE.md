# Local Test Mode

The application supports a local-only test mode that allows developers to run and debug the application without requiring the .NET backend to be running.

## How to Enable

Set the `LOCAL_TEST_MODE` environment variable to `"true"` in your `.env` file:

```bash
LOCAL_TEST_MODE=true
```

## What it Does

When local test mode is enabled:

1. **Metadata API**: Instead of calling the backend, the application serves mock metadata from static JSON files
2. **Randomization API**: Returns mock randomization responses instead of calling the backend
3. **Error Handling**: Provides helpful error messages about available mock game IDs

## Mock Data Files

Currently supported mock data:

- `settings_zelda3.json` - Complete metadata configuration for A Link to the Past
  - Available under game IDs: `alttp`, `alttpr`, `zelda3`
  - Includes global settings and game-specific settings
  - Demonstrates all setting types: SingleChoice, MultipleChoice, Slider, Toggle, Input

## Game ID Mapping

The mock data service maps the following IDs to the Zelda 3 metadata:

- `alttp` - Standard game ID from game-static-info.json
- `alttpr` - Common abbreviation for "A Link to the Past Randomizer"
- `zelda3` - Alternative identifier

## Adding New Mock Data

To add mock data for additional games:

1. Create a JSON file with metadata following the `MetadataSchema` format
2. Update `src/lib/services/mock-data.ts` to load your new file
3. Add appropriate game ID mappings

## Testing

The local test mode allows you to:

- Test the configuration UI without a backend
- Verify form validation and user interactions
- Debug frontend issues in isolation
- Develop new features independently

## Disable Test Mode

To return to normal backend operation:

- Set `LOCAL_TEST_MODE=false` or remove the variable entirely
- Restart the development server

## Example Configuration

A working example is provided in `settings_zelda3.json` that demonstrates:

- Global settings with multiple categories (Core, Goals, Items, etc.)
- Game-specific settings for ALttP
- All supported setting types and their proper structure
- Realistic default values and option lists
