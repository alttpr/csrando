# Using presets

This guide is also available in the website's Help menu at
`/content/presets`.

Presets let you save a set of randomizer settings under a name and return to
it later. They are useful for recurring races, personal presets, or any setup
you do not want to rebuild one option at a time.

The preset controls appear at the top of the configurator. You can browse
official presets without an account. Sign in to save and manage your own.

## Preset types

### Official presets

Official presets are maintained by the site administrators and are available
to everyone. The **Recommended** preset is the normal starting point for new
configurations.

Official presets have readable links, for example:

```text
/config/combo/recommended
```

You cannot overwrite an official preset. If you want to customize one, make
your changes and use **Save as new**, or use **Duplicate** from the preset
menu to create an independent personal copy.

### Personal presets

Personal presets belong to your account. Other users cannot browse or edit
them. You can change their settings, rename them, add a description, duplicate
them, or delete them.

A shared copy of a personal preset is still private by default. It only
becomes accessible through a share link that you explicitly create.

### Custom settings

**Custom** means that the configurator is not currently attached to a saved
preset. This includes settings loaded from a shared personal preset or from
a seed snapshot. Save the configuration as a new preset if you want it added
to your account.

## Choosing and changing a preset

Use the preset selector at the top of the configurator to search official and
personal presets. Selecting one replaces the settings currently shown in the
form.

When you change a setting after loading a preset, the configurator shows a
**Modified** badge. At that point your settings no longer exactly match the
saved revision. You can then:

- **Save changes** to add the new settings as the preset's latest revision.
- **Save as new** to keep the original preset unchanged and create a separate
  personal preset.
- **Revert** to discard your changes and reload the saved settings.
- Generate a seed without saving. The seed keeps the exact settings used, but
  is treated as modified rather than as an exact use of the original preset.

The site warns you before switching presets or leaving the configurator with
unsaved changes. A local draft may also be restored if the page is reloaded.

## Creating a preset

1. Open a configurator and choose the settings you want.
2. Select **Save as new** in the preset controls.
3. Enter a name and, optionally, a description.
4. Choose whether to make it your default or pin it.
5. Save the preset.

Preset names only need to be meaningful to you; personal presets do not need
a public URL name.

## Revisions

Saving changes creates a new revision instead of silently rewriting the old
one. New seeds use the latest revision, while existing seeds retain the exact
settings and revision they were generated with.

This is why a preset may display a revision number. It helps distinguish a
newer version of a recurring setup from the version used by an older seed.

## Defaults, recently used presets, and pins

- **Default** selects a preset automatically when you open that
  configurator. You can have one default preset per configurator.
- **Recently used** lets the site return to the preset you last used when no
  explicit link, recoverable draft, or default takes priority.
- **Pin** keeps frequently used presets easy to find near the top of the
  selector. Pinning does not make a preset public or change its settings.

An explicit preset link always takes priority over these preferences.

## Sharing presets

Use **Copy share link** from the preset menu.

- Official presets use their permanent readable URL, such as
  `/config/combo/recommended`.
- Personal presets use a private capability link. Anyone who has that link
  can open a snapshot of the preset's current settings, but they do not gain
  access to your preset or account.

When someone opens a personal share link, the settings load as **Custom**.
They can generate a seed or save their own independent copy; changes they make
do not affect your preset.

Use **Disable share link** to invalidate the personal preset's existing link.
You can create a new link later. Treat an active share link as sensitive: it
can be forwarded by anyone who receives it.

## Presets on seed pages

A seed records the complete settings used to generate it, even if its source
preset is later changed.

If the seed exactly matches an official preset, or a personal preset you can
access, **Open these settings in the configurator** opens that preset. For an
official preset, the link uses its readable name.

If the settings were modified, the preset is inaccessible, or no preset was
used, the link instead opens the seed's stored settings as a **Custom**
configuration. This avoids suggesting that the current preset is identical
to the seed when it is not.

Private preset attribution is only visible to its owner. Other visitors can
still view a public seed, but they do not see the name or a link to the private
preset behind it.

## Managing your presets

Open **Account > Presets**, or select **Manage presets** from the
configurator's preset menu. The management page lets you:

- search your saved presets;
- open a preset in its configurator;
- rename it or update its description;
- set or clear the default;
- pin or unpin it;
- duplicate it; and
- delete it.

Deleting a preset removes it from your account, so it can no longer be used
to generate new seeds. Seeds already generated from it keep their stored
settings.

## Troubleshooting

### A shared link is invalid

The owner may have revoked the link or deleted the preset. Ask them for a new
link. An invalid link does not expose whether the original preset still
exists.

### Some settings changed when a preset loaded

Presets can outlive changes to the randomizer's available options. When an
older preset is loaded, the configurator migrates settings where possible and
shows a notice listing values that were removed or reset.

### My changes conflict with a newer revision

The preset was updated elsewhere after you loaded it, such as in another
browser tab. Reload the latest revision before making and saving your changes
again.

### I cannot save a preset

Saving personal presets requires a signed-in account. If you are signed out,
the configurator can retain a local draft in that browser, but it is not synced
to an account and is not a saved preset.
