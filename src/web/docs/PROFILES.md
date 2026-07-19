# Using profiles

This guide is also available in the website's Help menu at
`/content/profiles`.

Profiles let you save a set of randomizer settings under a name and return to
it later. They are useful for recurring races, personal presets, or any setup
you do not want to rebuild one option at a time.

The profile controls appear at the top of the configurator. You can browse
official profiles without an account. Sign in to save and manage your own.

## Profile types

### Official profiles

Official profiles are maintained by the site administrators and are available
to everyone. The **Recommended** profile is the normal starting point for new
configurations.

Official profiles have readable links, for example:

```text
/config/combo/recommended
```

You cannot overwrite an official profile. If you want to customize one, make
your changes and use **Save as new**, or use **Duplicate** from the profile
menu to create an independent personal copy.

### Personal profiles

Personal profiles belong to your account. Other users cannot browse or edit
them. You can change their settings, rename them, add a description, duplicate
them, or delete them.

A shared copy of a personal profile is still private by default. It only
becomes accessible through a share link that you explicitly create.

### Custom settings

**Custom** means that the configurator is not currently attached to a saved
profile. This includes settings loaded from a shared personal profile or from
a seed snapshot. Save the configuration as a new profile if you want it added
to your account.

## Choosing and changing a profile

Use the profile selector at the top of the configurator to search official and
personal profiles. Selecting one replaces the settings currently shown in the
form.

When you change a setting after loading a profile, the configurator shows a
**Modified** badge. At that point your settings no longer exactly match the
saved revision. You can then:

- **Save changes** to add the new settings as the profile's latest revision.
- **Save as new** to keep the original profile unchanged and create a separate
  personal profile.
- **Revert** to discard your changes and reload the saved settings.
- Generate a seed without saving. The seed keeps the exact settings used, but
  is treated as modified rather than as an exact use of the original profile.

The site warns you before switching profiles or leaving the configurator with
unsaved changes. A local draft may also be restored if the page is reloaded.

## Creating a profile

1. Open a configurator and choose the settings you want.
2. Select **Save as new** in the profile controls.
3. Enter a name and, optionally, a description.
4. Choose whether to make it your default or pin it.
5. Save the profile.

Profile names only need to be meaningful to you; personal profiles do not need
a public URL name.

## Revisions

Saving changes creates a new revision instead of silently rewriting the old
one. New seeds use the latest revision, while existing seeds retain the exact
settings and revision they were generated with.

This is why a profile may display a revision number. It helps distinguish a
newer version of a recurring setup from the version used by an older seed.

## Defaults, recently used profiles, and pins

- **Default** selects a profile automatically when you open that
  configurator. You can have one default profile per configurator.
- **Recently used** lets the site return to the profile you last used when no
  explicit link, recoverable draft, or default takes priority.
- **Pin** keeps frequently used profiles easy to find near the top of the
  selector. Pinning does not make a profile public or change its settings.

An explicit profile link always takes priority over these preferences.

## Sharing profiles

Use **Copy share link** from the profile menu.

- Official profiles use their permanent readable URL, such as
  `/config/combo/recommended`.
- Personal profiles use a private capability link. Anyone who has that link
  can open a snapshot of the profile's current settings, but they do not gain
  access to your profile or account.

When someone opens a personal share link, the settings load as **Custom**.
They can generate a seed or save their own independent copy; changes they make
do not affect your profile.

Use **Disable share link** to invalidate the personal profile's existing link.
You can create a new link later. Treat an active share link as sensitive: it
can be forwarded by anyone who receives it.

## Profiles on seed pages

A seed records the complete settings used to generate it, even if its source
profile is later changed.

If the seed exactly matches an official profile, or a personal profile you can
access, **Open these settings in the configurator** opens that profile. For an
official profile, the link uses its readable name.

If the settings were modified, the profile is inaccessible, or no profile was
used, the link instead opens the seed's stored settings as a **Custom**
configuration. This avoids suggesting that the current profile is identical
to the seed when it is not.

Private profile attribution is only visible to its owner. Other visitors can
still view a public seed, but they do not see the name or a link to the private
profile behind it.

## Managing your profiles

Open **Account > Profiles**, or select **Manage profiles** from the
configurator's profile menu. The management page lets you:

- search your saved profiles;
- open a profile in its configurator;
- rename it or update its description;
- set or clear the default;
- pin or unpin it;
- duplicate it; and
- delete it.

Deleting a profile removes it from your account, so it can no longer be used
to generate new seeds. Seeds already generated from it keep their stored
settings.

## Troubleshooting

### A shared link is invalid

The owner may have revoked the link or deleted the profile. Ask them for a new
link. An invalid link does not expose whether the original profile still
exists.

### Some settings changed when a profile loaded

Profiles can outlive changes to the randomizer's available options. When an
older profile is loaded, the configurator migrates settings where possible and
shows a notice listing values that were removed or reset.

### My changes conflict with a newer revision

The profile was updated elsewhere after you loaded it, such as in another
browser tab. Reload the latest revision before making and saving your changes
again.

### I cannot save a profile

Saving personal profiles requires a signed-in account. If you are signed out,
the configurator can retain a local draft in that browser, but it is not synced
to an account and is not a saved profile.
